using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.DTOs;

// Project Response
public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Progress { get; set; }
    public string Color { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Project Detail Response
public class ProjectDetailDto : ProjectDto
{
    public List<UserDto> Members { get; set; } = new();
    public List<YumusDto> Tasks { get; set; } = new();
}

// Create Project Request
public class CreateProjectDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public Guid WorkspaceId { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public string Priority { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public string Color { get; set; } = string.Empty;
    
    public List<string> Tags { get; set; } = new();
    
    public List<Guid> MemberIds { get; set; } = new();
}

// Update Project Request
public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Progress { get; set; }
    public string Color { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}