using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class TaskComment
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaskId { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Text { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Yumus Task { get; set; } = null!;
    public User User { get; set; } = null!;
}