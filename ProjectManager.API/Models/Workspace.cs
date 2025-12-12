using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class Workspace
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty; // Hex color
    
    public string? Icon { get; set; }
    
    [Required]
    public Guid OwnerId { get; set; } // Creator
    
    [Required]
    public Guid AdminId { get; set; } // Workspace Admin
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public User Owner { get; set; } = null!;
    public User Admin { get; set; } = null!;
    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<DailyReport> DailyReports { get; set; } = new List<DailyReport>();
}