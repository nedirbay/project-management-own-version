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
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public ProjectsController(
        IProjectRepository projectRepository,
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAllProjects()
    {
        var projects = await _projectRepository.GetAllAsync();
        var projectDtos = projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            WorkspaceId = p.WorkspaceId,
            WorkspaceName = p.Workspace.Name,
            Status = p.Status.ToString(),
            Priority = p.Priority.ToString(),
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Progress = p.Progress,
            Color = p.Color,
            Tags = string.IsNullOrEmpty(p.Tags) ? new List<string>() : p.Tags.Split(',').ToList(),
            TaskCount = 0, // Will be populated later
            CompletedTaskCount = 0, // Will be populated later
            CreatedAt = p.CreatedAt
        }).ToList();

        // Populate counts
        foreach (var projectDto in projectDtos)
        {
            var project = projects.FirstOrDefault(p => p.Id == projectDto.Id);
            if (project != null)
            {
                projectDto.TaskCount = await _projectRepository.GetTaskCountAsync(project.Id);
                projectDto.CompletedTaskCount = await _projectRepository.GetCompletedTaskCountAsync(project.Id);
            }
        }

        return Ok(projectDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailDto>> GetProject(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Check if user has access to this project
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        var hasAccess = isWorkspaceMember || project.OwnerId == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasAccess)
        {
            return Forbid();
        }

        var projectDto = new ProjectDetailDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            WorkspaceId = project.WorkspaceId,
            WorkspaceName = project.Workspace.Name,
            Status = project.Status.ToString(),
            Priority = project.Priority.ToString(),
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Progress = project.Progress,
            Color = project.Color,
            Tags = string.IsNullOrEmpty(project.Tags) ? new List<string>() : project.Tags.Split(',').ToList(),
            TaskCount = await _projectRepository.GetTaskCountAsync(project.Id),
            CompletedTaskCount = await _projectRepository.GetCompletedTaskCountAsync(project.Id),
            CreatedAt = project.CreatedAt
        };

        // Get members
        var members = await _projectRepository.GetMembersAsync(id);
        projectDto.Members = _mapper.Map<List<UserDto>>(members);

        // Get tasks (will be populated when TasksController is implemented)
        projectDto.Tasks = new List<YumusDto>();

        return Ok(projectDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto createProjectDto)
    {
        // Check if workspace exists
        var workspaceExists = await _workspaceRepository.ExistsAsync(createProjectDto.WorkspaceId);
        if (!workspaceExists)
        {
            return BadRequest(new { Message = "Workspace not found" });
        }

        // Check if user has permission to create project in this workspace
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(createProjectDto.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        var hasPermission = isWorkspaceMember || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasPermission)
        {
            return Forbid();
        }

        var project = new Project
        {
            Name = createProjectDto.Name,
            Description = createProjectDto.Description,
            WorkspaceId = createProjectDto.WorkspaceId,
            OwnerId = currentUserId,
            Status = Enum.TryParse<ProjectStatus>(createProjectDto.Status, out var status) ? status : ProjectStatus.Planning,
            Priority = Enum.TryParse<Priority>(createProjectDto.Priority, out var priority) ? priority : Priority.Medium,
            StartDate = createProjectDto.StartDate,
            EndDate = createProjectDto.EndDate,
            Color = createProjectDto.Color,
            Tags = string.Join(",", createProjectDto.Tags),
            IsActive = true
        };

        var createdProject = await _projectRepository.CreateAsync(project);

        // Add members
        foreach (var memberId in createProjectDto.MemberIds)
        {
            await _projectRepository.AddMemberAsync(createdProject.Id, memberId);
        }

        var projectDto = new ProjectDto
        {
            Id = createdProject.Id,
            Name = createdProject.Name,
            Description = createdProject.Description,
            WorkspaceId = createdProject.WorkspaceId,
            WorkspaceName = createdProject.Workspace.Name,
            Status = createdProject.Status.ToString(),
            Priority = createdProject.Priority.ToString(),
            StartDate = createdProject.StartDate,
            EndDate = createdProject.EndDate,
            Progress = createdProject.Progress,
            Color = createdProject.Color,
            Tags = string.IsNullOrEmpty(createdProject.Tags) ? new List<string>() : createdProject.Tags.Split(',').ToList(),
            TaskCount = await _projectRepository.GetTaskCountAsync(createdProject.Id),
            CompletedTaskCount = await _projectRepository.GetCompletedTaskCountAsync(createdProject.Id),
            CreatedAt = createdProject.CreatedAt
        };

        return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, projectDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProjectDto>> UpdateProject(Guid id, [FromBody] UpdateProjectDto updateProjectDto)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        var hasPermission = isWorkspaceMember || project.OwnerId == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasPermission)
        {
            return Forbid();
        }

        // Update project
        project.Name = updateProjectDto.Name;
        project.Description = updateProjectDto.Description;
        project.Status = Enum.TryParse<ProjectStatus>(updateProjectDto.Status, out var status) ? status : project.Status;
        project.Priority = Enum.TryParse<Priority>(updateProjectDto.Priority, out var priority) ? priority : project.Priority;
        project.StartDate = updateProjectDto.StartDate;
        project.EndDate = updateProjectDto.EndDate;
        project.Progress = updateProjectDto.Progress;
        project.Color = updateProjectDto.Color;
        project.Tags = string.Join(",", updateProjectDto.Tags);
        project.UpdatedAt = DateTime.UtcNow;

        var updatedProject = await _projectRepository.UpdateAsync(project);

        var projectDto = new ProjectDto
        {
            Id = updatedProject.Id,
            Name = updatedProject.Name,
            Description = updatedProject.Description,
            WorkspaceId = updatedProject.WorkspaceId,
            WorkspaceName = updatedProject.Workspace.Name,
            Status = updatedProject.Status.ToString(),
            Priority = updatedProject.Priority.ToString(),
            StartDate = updatedProject.StartDate,
            EndDate = updatedProject.EndDate,
            Progress = updatedProject.Progress,
            Color = updatedProject.Color,
            Tags = string.IsNullOrEmpty(updatedProject.Tags) ? new List<string>() : updatedProject.Tags.Split(',').ToList(),
            TaskCount = await _projectRepository.GetTaskCountAsync(updatedProject.Id),
            CompletedTaskCount = await _projectRepository.GetCompletedTaskCountAsync(updatedProject.Id),
            CreatedAt = updatedProject.CreatedAt
        };

        return Ok(projectDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var hasPermission = project.OwnerId == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        if (!hasPermission)
        {
            return Forbid();
        }

        var result = await _projectRepository.DeleteAsync(id);
        if (!result)
        {
            return StatusCode(500, new { Message = "Error deleting project" });
        }

        return NoContent();
    }

    [HttpPatch("{id}/progress")]
    public async Task<ActionResult<ProjectDto>> UpdateProjectProgress(Guid id, [FromBody] dynamic progressData)
    {
        var progress = progressData.progress?.ToString();
        int progressValue = 0;
        if (string.IsNullOrEmpty(progress) || !int.TryParse(progress, out progressValue) || progressValue < 0 || progressValue > 100)
        {
            return BadRequest(new { Message = "Progress must be a number between 0 and 100" });
        }

        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        var hasPermission = isWorkspaceMember || project.OwnerId == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasPermission)
        {
            return Forbid();
        }

        project.Progress = progressValue;
        project.UpdatedAt = DateTime.UtcNow;

        var updatedProject = await _projectRepository.UpdateAsync(project);

        var projectDto = new ProjectDto
        {
            Id = updatedProject.Id,
            Name = updatedProject.Name,
            Description = updatedProject.Description,
            WorkspaceId = updatedProject.WorkspaceId,
            WorkspaceName = updatedProject.Workspace.Name,
            Status = updatedProject.Status.ToString(),
            Priority = updatedProject.Priority.ToString(),
            StartDate = updatedProject.StartDate,
            EndDate = updatedProject.EndDate,
            Progress = updatedProject.Progress,
            Color = updatedProject.Color,
            Tags = string.IsNullOrEmpty(updatedProject.Tags) ? new List<string>() : updatedProject.Tags.Split(',').ToList(),
            TaskCount = await _projectRepository.GetTaskCountAsync(updatedProject.Id),
            CompletedTaskCount = await _projectRepository.GetCompletedTaskCountAsync(updatedProject.Id),
            CreatedAt = updatedProject.CreatedAt
        };

        return Ok(projectDto);
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetProjectMembers(Guid id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Check if user has access to this project
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        // Check if user is member of the workspace
        var workspaceMembers = await _workspaceRepository.GetMembersAsync(project.WorkspaceId);
        var isWorkspaceMember = workspaceMembers.Any(m => m.Id == currentUserId);
        var hasAccess = isWorkspaceMember || project.OwnerId == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        if (!hasAccess)
        {
            return Forbid();
        }

        var members = await _projectRepository.GetMembersAsync(id);
        var memberDtos = _mapper.Map<List<UserDto>>(members);

        return Ok(memberDtos);
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddProjectMember(Guid id, [FromBody] dynamic memberData)
    {
        var userId = memberData.userId?.ToString();
        if (string.IsNullOrEmpty(userId) || !Guid.Parse(userId) == null)
        {
            return BadRequest(new { Message = "Valid user ID is required" });
        }

        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var hasPermission = project.OwnerId == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        if (!hasPermission)
        {
            return Forbid();
        }

        // Check if user exists
        var userExists = await _userRepository.ExistsAsync(userId);
        if (!userExists)
        {
            return BadRequest(new { Message = "User not found" });
        }

        // Add member
        var result = await _projectRepository.AddMemberAsync(id, userId);
        if (!result)
        {
            return StatusCode(500, new { Message = "Error adding member" });
        }

        return Ok(new { Message = "Member added successfully" });
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveProjectMember(Guid id, Guid userId)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var hasPermission = project.OwnerId == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        if (!hasPermission)
        {
            return Forbid();
        }

        // Remove member
        var result = await _projectRepository.RemoveMemberAsync(id, userId);
        if (!result)
        {
            return NotFound(new { Message = "Member not found" });
        }

        return Ok(new { Message = "Member removed successfully" });
    }
}