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
public class WorkspacesController : ControllerBase
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public WorkspacesController(
        IWorkspaceRepository workspaceRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _workspaceRepository = workspaceRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkspaceDto>>> GetAllWorkspaces()
    {
        var workspaces = await _workspaceRepository.GetAllAsync();
        var workspaceDtos = workspaces.Select(w => new WorkspaceDto
        {
            Id = w.Id,
            Name = w.Name,
            Description = w.Description,
            Color = w.Color,
            Icon = w.Icon ?? string.Empty,
            AdminId = w.AdminId,
            AdminName = w.Admin.FullName,
            MemberCount = 0, // Will be populated later
            ProjectCount = 0, // Will be populated later
            CreatedAt = w.CreatedAt
        }).ToList();

        // Populate counts
        foreach (var workspaceDto in workspaceDtos)
        {
            var workspace = workspaces.FirstOrDefault(w => w.Id == workspaceDto.Id);
            if (workspace != null)
            {
                workspaceDto.MemberCount = await _workspaceRepository.GetMemberCountAsync(workspace.Id);
                workspaceDto.ProjectCount = await _workspaceRepository.GetProjectCountAsync(workspace.Id);
            }
        }

        return Ok(workspaceDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkspaceDetailDto>> GetWorkspace(Guid id)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }

        // Check if user has access to this workspace
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var isMember = workspace.Members.Any(m => m.UserId == currentUserId);
        var hasAccess = isMember || workspace.OwnerId == currentUserId || workspace.AdminId == currentUserId || currentUserRole == "Admin";
        
        if (!hasAccess)
        {
            return Forbid();
        }

        var workspaceDto = new WorkspaceDetailDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            Color = workspace.Color,
            Icon = workspace.Icon ?? string.Empty,
            AdminId = workspace.AdminId,
            AdminName = workspace.Admin.FullName,
            MemberCount = await _workspaceRepository.GetMemberCountAsync(workspace.Id),
            ProjectCount = await _workspaceRepository.GetProjectCountAsync(workspace.Id),
            CreatedAt = workspace.CreatedAt
        };

        // Get members
        var members = await _workspaceRepository.GetMembersAsync(id);
        workspaceDto.Members = _mapper.Map<List<UserDto>>(members);

        // Get projects (will be implemented when ProjectsController is created)
        workspaceDto.Projects = new List<ProjectDto>();

        return Ok(workspaceDto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,WorkspaceAdmin")]
    public async Task<ActionResult<WorkspaceDto>> CreateWorkspace([FromBody] CreateWorkspaceDto createWorkspaceDto)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        var workspace = new Workspace
        {
            Name = createWorkspaceDto.Name,
            Description = createWorkspaceDto.Description,
            Color = createWorkspaceDto.Color,
            OwnerId = currentUserId,
            AdminId = currentUserId,
            IsActive = true
        };

        var createdWorkspace = await _workspaceRepository.CreateAsync(workspace);

        // Add members
        foreach (var memberId in createWorkspaceDto.MemberIds)
        {
            await _workspaceRepository.AddMemberAsync(createdWorkspace.Id, memberId);
        }

        var workspaceDto = new WorkspaceDto
        {
            Id = createdWorkspace.Id,
            Name = createdWorkspace.Name,
            Description = createdWorkspace.Description,
            Color = createdWorkspace.Color,
            Icon = createdWorkspace.Icon ?? string.Empty,
            AdminId = createdWorkspace.AdminId,
            AdminName = createdWorkspace.Admin.FullName,
            MemberCount = await _workspaceRepository.GetMemberCountAsync(createdWorkspace.Id),
            ProjectCount = await _workspaceRepository.GetProjectCountAsync(createdWorkspace.Id),
            CreatedAt = createdWorkspace.CreatedAt
        };

        return CreatedAtAction(nameof(GetWorkspace), new { id = createdWorkspace.Id }, workspaceDto);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,WorkspaceAdmin")]
    public async Task<ActionResult<WorkspaceDto>> UpdateWorkspace(Guid id, [FromBody] UpdateWorkspaceDto updateWorkspaceDto)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var hasPermission = workspace.OwnerId == currentUserId || workspace.AdminId == currentUserId || currentUserRole == "Admin";
        if (!hasPermission)
        {
            return Forbid();
        }

        // Update workspace
        workspace.Name = updateWorkspaceDto.Name;
        workspace.Description = updateWorkspaceDto.Description;
        workspace.Color = updateWorkspaceDto.Color;
        workspace.UpdatedAt = DateTime.UtcNow;

        var updatedWorkspace = await _workspaceRepository.UpdateAsync(workspace);

        var workspaceDto = new WorkspaceDto
        {
            Id = updatedWorkspace.Id,
            Name = updatedWorkspace.Name,
            Description = updatedWorkspace.Description,
            Color = updatedWorkspace.Color,
            Icon = updatedWorkspace.Icon ?? string.Empty,
            AdminId = updatedWorkspace.AdminId,
            AdminName = updatedWorkspace.Admin.FullName,
            MemberCount = await _workspaceRepository.GetMemberCountAsync(updatedWorkspace.Id),
            ProjectCount = await _workspaceRepository.GetProjectCountAsync(updatedWorkspace.Id),
            CreatedAt = updatedWorkspace.CreatedAt
        };

        return Ok(workspaceDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteWorkspace(Guid id)
    {
        var workspaceExists = await _workspaceRepository.ExistsAsync(id);
        if (!workspaceExists)
        {
            return NotFound();
        }

        var result = await _workspaceRepository.DeleteAsync(id);
        if (!result)
        {
            return StatusCode(500, new { Message = "Error deleting workspace" });
        }

        return NoContent();
    }

    [HttpGet("{id}/members")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetWorkspaceMembers(Guid id)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }

        // Check if user has access to this workspace
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var isMember = workspace.Members.Any(m => m.UserId == currentUserId);
        var hasAccess = isMember || workspace.OwnerId == currentUserId || workspace.AdminId == currentUserId || currentUserRole == "Admin";
        
        if (!hasAccess)
        {
            return Forbid();
        }

        var members = await _workspaceRepository.GetMembersAsync(id);
        var memberDtos = _mapper.Map<List<UserDto>>(members);

        return Ok(memberDtos);
    }

    [HttpPost("{id}/members")]
    [Authorize(Roles = "Admin,WorkspaceAdmin")]
    public async Task<IActionResult> AddWorkspaceMember(Guid id, [FromBody] AddWorkspaceMemberDto addMemberDto)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var hasPermission = workspace.OwnerId == currentUserId || workspace.AdminId == currentUserId || currentUserRole == "Admin";
        if (!hasPermission)
        {
            return Forbid();
        }

        // Check if user exists
        var userExists = await _userRepository.ExistsAsync(addMemberDto.UserId);
        if (!userExists)
        {
            return BadRequest(new { Message = "User not found" });
        }

        // Add member
        var result = await _workspaceRepository.AddMemberAsync(id, addMemberDto.UserId);
        if (!result)
        {
            return StatusCode(500, new { Message = "Error adding member" });
        }

        return Ok(new { Message = "Member added successfully" });
    }

    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Roles = "Admin,WorkspaceAdmin")]
    public async Task<IActionResult> RemoveWorkspaceMember(Guid id, Guid userId)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
        
        var hasPermission = workspace.OwnerId == currentUserId || workspace.AdminId == currentUserId || currentUserRole == "Admin";
        if (!hasPermission)
        {
            return Forbid();
        }

        // Prevent removing workspace admin
        if (workspace.AdminId == userId)
        {
            return BadRequest(new { Message = "Cannot remove workspace admin" });
        }

        // Remove member
        var result = await _workspaceRepository.RemoveMemberAsync(id, userId);
        if (!result)
        {
            return NotFound(new { Message = "Member not found" });
        }

        return Ok(new { Message = "Member removed successfully" });
    }
}