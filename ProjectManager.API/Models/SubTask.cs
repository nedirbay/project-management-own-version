using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class SubTask
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaskId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public bool IsCompleted { get; set; } = false;
    
    public int Order { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Yumus Task { get; set; } = null!;
}