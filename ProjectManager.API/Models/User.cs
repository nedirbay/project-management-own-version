using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class User
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty; // Unique
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty; // Unique
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } // Enum: Admin, WorkspaceAdmin, Member
    
    public string? Avatar { get; set; } // Base64 or URL
    
    public string? Phone { get; set; }
    
    public string? Bio { get; set; }
    
    public bool IsActive { get; set; } // Soft delete
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public ICollection<Workspace> OwnedWorkspaces { get; set; } = new List<Workspace>();
    public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<TaskAssignee> AssignedYumuses { get; set; } = new List<TaskAssignee>();
    public ICollection<Yumus> CreatedYumuses { get; set; } = new List<Yumus>();
    public ICollection<DailyReport> DailyReports { get; set; } = new List<DailyReport>();
    public UserSettings? Settings { get; set; }
}

public enum UserRole
{
    Admin = 0,
    WorkspaceAdmin = 1,
    Member = 2
}