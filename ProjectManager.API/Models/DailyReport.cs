using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class DailyReport
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public DateTime Date { get; set; } // Report date
    
    [Required]
    public Guid WorkspaceId { get; set; }
    
    public Guid? ProjectId { get; set; }
    
    [Required]
    public string WorkDescription { get; set; } = string.Empty;
    
    public string YumusesCompleted { get; set; } = string.Empty; // JSON array of yumus IDs
    
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
    public Project? Project { get; set; }
}