using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class TaskAttachment
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid TaskId { get; set; }
    
    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string FileUrl { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [Required]
    [StringLength(50)]
    public string FileType { get; set; } = string.Empty;
    
    [Required]
    public Guid UploadedBy { get; set; }
    
    public DateTime UploadedAt { get; set; }

    // Navigation
    public Yumus Task { get; set; } = null!;
    public User Uploader { get; set; } = null!;
}