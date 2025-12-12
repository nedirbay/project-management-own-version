using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class Project
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public Guid WorkspaceId { get; set; }
    
    [Required]
    public Guid OwnerId { get; set; }
    
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    
    public Priority Priority { get; set; } = Priority.Medium;
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public int Progress { get; set; } = 0; // 0-100
    
    [Required]
    public string Color { get; set; } = string.Empty;
    
    public string Tags { get; set; } = string.Empty; // JSON array or comma-separated
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Workspace Workspace { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<Yumus> Yumuses { get; set; } = new List<Yumus>();
    public ICollection<DailyReport> DailyReports { get; set; } = new List<DailyReport>();
}

public enum ProjectStatus
{
    Planning = 0,
    Active = 1,
    OnHold = 2,
    Completed = 3,
    Cancelled = 4
}

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}