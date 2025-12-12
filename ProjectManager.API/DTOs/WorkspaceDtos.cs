using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.DTOs;

// Workspace Response
public class WorkspaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Guid AdminId { get; set; }
    public string AdminName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int ProjectCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Workspace Detail Response
public class WorkspaceDetailDto : WorkspaceDto
{
    public List<UserDto> Members { get; set; } = new();
    public List<ProjectDto> Projects { get; set; } = new();
}

// Create Workspace Request
public class CreateWorkspaceDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
    
    public List<Guid> MemberIds { get; set; } = new();
}

// Update Workspace Request
public class UpdateWorkspaceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

// Add Member Request
public class AddWorkspaceMemberDto
{
    [Required]
    public Guid UserId { get; set; }
}