using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories.Interfaces;
using AutoMapper;
using System.Security.Claims;

namespace ProjectManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class YumusController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public YumusController(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<YumusDto>>> GetAllYumuses()
    {
        var tasks = await _taskRepository.GetAllAsync();
        var taskDtos = tasks.Select(t => new YumusDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            ProjectId = t.ProjectId,
            ProjectName = t.Project.Name,
            Status = t.Status.ToString(),
            Priority = t.Priority.ToString(),
            DueDate = t.DueDate,
            EstimatedHours = t.EstimatedHours,
            Tags = string.IsNullOrEmpty(t.Tags) ? new List<string>() : t.Tags.Split(',').ToList(),
            Order = t.Order,
            Assignees = new List<UserDto>(), // Will be populated later
            SubTaskCount = 0, // Will be populated later
            CompletedSubTaskCount = 0, // Will be populated later
            CreatedAt = t.CreatedAt
        }).ToList();

        // Populate counts and assignees
        foreach (var taskDto in taskDtos)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskDto.Id);
            if (task != null)
            {
                taskDto.SubTaskCount = await _taskRepository.GetSubTaskCountAsync(task.Id);
                taskDto.CompletedSubTaskCount = await _taskRepository.GetCompletedSubTaskCountAsync(task.Id);
                
                var assignees = await _taskRepository.GetAssigneesAsync(task.Id);
                taskDto.Assignees = _mapper.Map<List<UserDto>>(assignees);
            }
        }

        return Ok(taskDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<YumusDetailDto>> GetYumus(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        // Check if user has access to this task
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the project
        var projectMembers = await _projectRepository.GetMembersAsync(task.ProjectId);
        var isProjectMember = projectMembers.Any(m => m.Id == currentUserId);
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(task.Project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        
        var hasAccess = isProjectMember || isWorkspaceMember || task.CreatedBy == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasAccess)
        {
            return Forbid();
        }

        var taskDto = new YumusDetailDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            ProjectId = task.ProjectId,
            ProjectName = task.Project.Name,
            Status = task.Status.ToString(),
            Priority = task.Priority.ToString(),
            DueDate = task.DueDate,
            EstimatedHours = task.EstimatedHours,
            Tags = string.IsNullOrEmpty(task.Tags) ? new List<string>() : task.Tags.Split(',').ToList(),
            Order = task.Order,
            SubTaskCount = await _taskRepository.GetSubTaskCountAsync(task.Id),
            CompletedSubTaskCount = await _taskRepository.GetCompletedSubTaskCountAsync(task.Id),
            CreatedAt = task.CreatedAt
        };

        // Get assignees
        var assignees = await _taskRepository.GetAssigneesAsync(id);
        taskDto.Assignees = _mapper.Map<List<UserDto>>(assignees);

        // Get subtasks
        var subTasks = await _taskRepository.GetSubTasksAsync(id);
        taskDto.SubTasks = _mapper.Map<List<SubTaskDto>>(subTasks);

        // Get comments
        var comments = await _taskRepository.GetCommentsAsync(id);
        taskDto.Comments = _mapper.Map<List<TaskCommentDto>>(comments);

        // Get attachments
        var attachments = await _taskRepository.GetAttachmentsAsync(id);
        taskDto.Attachments = _mapper.Map<List<TaskAttachmentDto>>(attachments);

        return Ok(taskDto);
    }

    [HttpPost]
    public async Task<ActionResult<YumusDto>> CreateYumus([FromBody] CreateYumusDto createYumusDto)
    {
        // Check if project exists
        var project = await _projectRepository.GetByIdAsync(createYumusDto.ProjectId);
        if (project == null)
        {
            return BadRequest(new { Message = "Project not found" });
        }

        // Check if user has permission to create task in this project
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the project
        var projectMembers = await _projectRepository.GetMembersAsync(createYumusDto.ProjectId);
        var isProjectMember = projectMembers.Any(m => m.Id == currentUserId);
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        
        var hasPermission = isProjectMember || isWorkspaceMember || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasPermission)
        {
            return Forbid();
        }

        var task = new Yumus
        {
            Title = createYumusDto.Title,
            Description = createYumusDto.Description,
            ProjectId = createYumusDto.ProjectId,
            CreatedBy = currentUserId,
            Status = Enum.TryParse<TaskYagdaylar>(createYumusDto.Status, out var status) ? status : TaskYagdaylar.Todo,
            Priority = Enum.TryParse<Priority>(createYumusDto.Priority, out var priority) ? priority : Priority.Medium,
            DueDate = createYumusDto.DueDate,
            EstimatedHours = createYumusDto.EstimatedHours,
            Tags = string.Join(",", createYumusDto.Tags),
            Order = 0, // Will be set based on status
            IsActive = true
        };

        var createdTask = await _taskRepository.CreateAsync(task);

        // Assign to users
        foreach (var assigneeId in createYumusDto.AssigneeIds)
        {
            await _taskRepository.AssignToUserAsync(createdTask.Id, assigneeId);
        }

        var taskDto = new YumusDto
        {
            Id = createdTask.Id,
            Title = createdTask.Title,
            Description = createdTask.Description,
            ProjectId = createdTask.ProjectId,
            ProjectName = createdTask.Project.Name,
            Status = createdTask.Status.ToString(),
            Priority = createdTask.Priority.ToString(),
            DueDate = createdTask.DueDate,
            EstimatedHours = createdTask.EstimatedHours,
            Tags = string.IsNullOrEmpty(createdTask.Tags) ? new List<string>() : createdTask.Tags.Split(',').ToList(),
            Order = createdTask.Order,
            SubTaskCount = await _taskRepository.GetSubTaskCountAsync(createdTask.Id),
            CompletedSubTaskCount = await _taskRepository.GetCompletedSubTaskCountAsync(createdTask.Id),
            CreatedAt = createdTask.CreatedAt
        };

        // Get assignees
        var assignees = await _taskRepository.GetAssigneesAsync(createdTask.Id);
        taskDto.Assignees = _mapper.Map<List<UserDto>>(assignees);

        return CreatedAtAction(nameof(GetYumus), new { id = createdTask.Id }, taskDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<YumusDto>> UpdateYumus(Guid id, [FromBody] UpdateYumusDto updateYumusDto)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the project
        var projectMembers = await _projectRepository.GetMembersAsync(task.ProjectId);
        var isProjectMember = projectMembers.Any(m => m.Id == currentUserId);
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(task.Project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        
        // Check if user is assigned to the task
        var taskAssignees = await _taskRepository.GetAssigneesAsync(id);
        var isTaskAssignee = taskAssignees.Any(a => a.Id == currentUserId);
        
        var hasPermission = isProjectMember || isWorkspaceMember || isTaskAssignee || task.CreatedBy == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasPermission)
        {
            return Forbid();
        }

        // Update task
        task.Title = updateYumusDto.Title;
        task.Description = updateYumusDto.Description;
        task.Status = Enum.TryParse<TaskYagdaylar>(updateYumusDto.Status, out var status) ? status : task.Status;
        task.Priority = Enum.TryParse<Priority>(updateYumusDto.Priority, out var priority) ? priority : task.Priority;
        task.DueDate = updateYumusDto.DueDate;
        task.EstimatedHours = updateYumusDto.EstimatedHours;
        task.Tags = string.Join(",", updateYumusDto.Tags);
        task.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _taskRepository.UpdateAsync(task);

        var taskDto = new YumusDto
        {
            Id = updatedTask.Id,
            Title = updatedTask.Title,
            Description = updatedTask.Description,
            ProjectId = updatedTask.ProjectId,
            ProjectName = updatedTask.Project.Name,
            Status = updatedTask.Status.ToString(),
            Priority = updatedTask.Priority.ToString(),
            DueDate = updatedTask.DueDate,
            EstimatedHours = updatedTask.EstimatedHours,
            Tags = string.IsNullOrEmpty(updatedTask.Tags) ? new List<string>() : updatedTask.Tags.Split(',').ToList(),
            Order = updatedTask.Order,
            SubTaskCount = await _taskRepository.GetSubTaskCountAsync(updatedTask.Id),
            CompletedSubTaskCount = await _taskRepository.GetCompletedSubTaskCountAsync(updatedTask.Id),
            CreatedAt = updatedTask.CreatedAt
        };

        // Get assignees
        var assignees = await _taskRepository.GetAssigneesAsync(updatedTask.Id);
        taskDto.Assignees = _mapper.Map<List<UserDto>>(assignees);

        return Ok(taskDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteYumus(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var hasPermission = task.CreatedBy == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        if (!hasPermission)
        {
            return Forbid();
        }

        var result = await _taskRepository.DeleteAsync(id);
        if (!result)
        {
            return StatusCode(500, new { Message = "Error deleting task" });
        }

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<YumusDto>> UpdateYumusStatus(Guid id, [FromBody] UpdateYumusStatusDto updateYumusStatusDto)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        // Anyone with access to the task can update its status
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the project
        var projectMembers = await _projectRepository.GetMembersAsync(task.ProjectId);
        var isProjectMember = projectMembers.Any(m => m.Id == currentUserId);
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(task.Project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        
        // Check if user is assigned to the task
        var taskAssignees = await _taskRepository.GetAssigneesAsync(id);
        var isTaskAssignee = taskAssignees.Any(a => a.Id == currentUserId);
        
        var hasAccess = isProjectMember || isWorkspaceMember || isTaskAssignee || task.CreatedBy == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasAccess)
        {
            return Forbid();
        }

        if (!Enum.TryParse<TaskYagdaylar>(updateYumusStatusDto.Status, out var status))
        {
            return BadRequest(new { Message = "Invalid status" });
        }

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _taskRepository.UpdateAsync(task);

        var taskDto = new YumusDto
        {
            Id = updatedTask.Id,
            Title = updatedTask.Title,
            Description = updatedTask.Description,
            ProjectId = updatedTask.ProjectId,
            ProjectName = updatedTask.Project.Name,
            Status = updatedTask.Status.ToString(),
            Priority = updatedTask.Priority.ToString(),
            DueDate = updatedTask.DueDate,
            EstimatedHours = updatedTask.EstimatedHours,
            Tags = string.IsNullOrEmpty(updatedTask.Tags) ? new List<string>() : updatedTask.Tags.Split(',').ToList(),
            Order = updatedTask.Order,
            SubTaskCount = await _taskRepository.GetSubTaskCountAsync(updatedTask.Id),
            CompletedSubTaskCount = await _taskRepository.GetCompletedSubTaskCountAsync(updatedTask.Id),
            CreatedAt = updatedTask.CreatedAt
        };

        // Get assignees
        var assignees = await _taskRepository.GetAssigneesAsync(updatedTask.Id);
        taskDto.Assignees = _mapper.Map<List<UserDto>>(assignees);

        return Ok(taskDto);
    }

    [HttpPatch("{id}/order")]
    public async Task<ActionResult<YumusDto>> UpdateYumusOrder(Guid id, [FromBody] UpdateYumusOrderDto updateYumusOrderDto)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        // Anyone with access to the task can update its order
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the project
        var projectMembers = await _projectRepository.GetMembersAsync(task.ProjectId);
        var isProjectMember = projectMembers.Any(m => m.Id == currentUserId);
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(task.Project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        
        // Check if user is assigned to the task
        var taskAssignees = await _taskRepository.GetAssigneesAsync(id);
        var isTaskAssignee = taskAssignees.Any(a => a.Id == currentUserId);
        
        var hasAccess = isProjectMember || isWorkspaceMember || isTaskAssignee || task.CreatedBy == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasAccess)
        {
            return Forbid();
        }

        if (!Enum.TryParse<TaskYagdaylar>(updateYumusOrderDto.Status, out var status))
        {
            return BadRequest(new { Message = "Invalid status" });
        }

        task.Status = status;
        task.Order = updateYumusOrderDto.Order;
        task.UpdatedAt = DateTime.UtcNow;

        var updatedTask = await _taskRepository.UpdateAsync(task);

        var taskDto = new YumusDto
        {
            Id = updatedTask.Id,
            Title = updatedTask.Title,
            Description = updatedTask.Description,
            ProjectId = updatedTask.ProjectId,
            ProjectName = updatedTask.Project.Name,
            Status = updatedTask.Status.ToString(),
            Priority = updatedTask.Priority.ToString(),
            DueDate = updatedTask.DueDate,
            EstimatedHours = updatedTask.EstimatedHours,
            Tags = string.IsNullOrEmpty(updatedTask.Tags) ? new List<string>() : updatedTask.Tags.Split(',').ToList(),
            Order = updatedTask.Order,
            SubTaskCount = await _taskRepository.GetSubTaskCountAsync(updatedTask.Id),
            CompletedSubTaskCount = await _taskRepository.GetCompletedSubTaskCountAsync(updatedTask.Id),
            CreatedAt = updatedTask.CreatedAt
        };

        // Get assignees
        var assignees = await _taskRepository.GetAssigneesAsync(updatedTask.Id);
        taskDto.Assignees = _mapper.Map<List<UserDto>>(assignees);

        return Ok(taskDto);
    }

    // SubTasks endpoints
    [HttpGet("{id}/subtasks")]
    public async Task<ActionResult<IEnumerable<SubTaskDto>>> GetSubTasks(Guid id)
    {
        var taskExists = await _taskRepository.ExistsAsync(id);
        if (!taskExists)
        {
            return NotFound();
        }

        var subTasks = await _taskRepository.GetSubTasksAsync(id);
        var subTaskDtos = _mapper.Map<List<SubTaskDto>>(subTasks);

        return Ok(subTaskDtos);
    }

    [HttpPost("{id}/subtasks")]
    public async Task<ActionResult<SubTaskDto>> CreateSubTask(Guid id, [FromBody] CreateSubTaskDto createSubTaskDto)
    {
        var taskExists = await _taskRepository.ExistsAsync(id);
        if (!taskExists)
        {
            return NotFound();
        }

        // In a real implementation, you would save the subtask through a repository
        // For now, we'll just return a placeholder
        return Ok(new { Message = "SubTask creation endpoint - implementation needed" });
    }

    [HttpPut("{taskId}/subtasks/{id}")]
    public async Task<ActionResult<SubTaskDto>> UpdateSubTask(Guid taskId, Guid id, [FromBody] CreateSubTaskDto updateSubTaskDto)
    {
        // In a real implementation, you would update the subtask through a repository
        // For now, we'll just return a placeholder
        await Task.CompletedTask;
        return Ok(new { Message = "SubTask update endpoint - implementation needed" });
    }

    [HttpDelete("{taskId}/subtasks/{id}")]
    public async Task<IActionResult> DeleteSubTask(Guid taskId, Guid id)
    {
        // In a real implementation, you would delete the subtask through a repository
        // For now, we'll just return a placeholder
        await Task.CompletedTask;
        return Ok(new { Message = "SubTask deletion endpoint - implementation needed" });
    }

    [HttpPatch("{taskId}/subtasks/{id}/toggle")]
    public async Task<ActionResult<SubTaskDto>> ToggleSubTask(Guid taskId, Guid id)
    {
        // In a real implementation, you would toggle the subtask through a repository
        // For now, we'll just return a placeholder
        await Task.CompletedTask;
        return Ok(new { Message = "SubTask toggle endpoint - implementation needed" });
    }

    // Comments endpoints
    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<TaskCommentDto>>> GetComments(Guid id)
    {
        var taskExists = await _taskRepository.ExistsAsync(id);
        if (!taskExists)
        {
            return NotFound();
        }

        var comments = await _taskRepository.GetCommentsAsync(id);
        var commentDtos = _mapper.Map<List<TaskCommentDto>>(comments);

        return Ok(commentDtos);
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult<TaskCommentDto>> CreateComment(Guid id, [FromBody] CreateTaskCommentDto createCommentDto)
    {
        var taskExists = await _taskRepository.ExistsAsync(id);
        if (!taskExists)
        {
            return NotFound();
        }

        // In a real implementation, you would save the comment through a repository
        // For now, we'll just return a placeholder
        return Ok(new { Message = "Comment creation endpoint - implementation needed" });
    }

    [HttpPut("{taskId}/comments/{id}")]
    public async Task<ActionResult<TaskCommentDto>> UpdateComment(Guid taskId, Guid id, [FromBody] CreateTaskCommentDto updateCommentDto)
    {
        // In a real implementation, you would update the comment through a repository
        // For now, we'll just return a placeholder
        await Task.CompletedTask;
        return Ok(new { Message = "Comment update endpoint - implementation needed" });
    }

    [HttpDelete("{taskId}/comments/{id}")]
    public async Task<IActionResult> DeleteComment(Guid taskId, Guid id)
    {
        // In a real implementation, you would delete the comment through a repository
        // For now, we'll just return a placeholder
        await Task.CompletedTask;
        return Ok(new { Message = "Comment deletion endpoint - implementation needed" });
    }

    // Attachments endpoints
    [HttpGet("{id}/attachments")]
    public async Task<ActionResult<IEnumerable<TaskAttachmentDto>>> GetAttachments(Guid id)
    {
        var taskExists = await _taskRepository.ExistsAsync(id);
        if (!taskExists)
        {
            return NotFound();
        }

        var attachments = await _taskRepository.GetAttachmentsAsync(id);
        var attachmentDtos = _mapper.Map<List<TaskAttachmentDto>>(attachments);

        return Ok(attachmentDtos);
    }

    [HttpPost("{id}/attachments")]
    public async Task<ActionResult<TaskAttachmentDto>> UploadAttachment(Guid id)
    {
        var taskExists = await _taskRepository.ExistsAsync(id);
        if (!taskExists)
        {
            return NotFound();
        }

        // In a real implementation, you would handle file upload here
        // For now, we'll just return a placeholder

        return Ok(new { Message = "File upload endpoint - implementation needed" });
    }

    [HttpDelete("{taskId}/attachments/{id}")]
    public async Task<IActionResult> DeleteAttachment(Guid taskId, Guid id)
    {
        // In a real implementation, you would delete the attachment through a repository
        // For now, we'll just return a placeholder
        await Task.CompletedTask;
        return Ok(new { Message = "Attachment deletion endpoint - implementation needed" });
    }
}