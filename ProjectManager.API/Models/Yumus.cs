using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class Yumus
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public Guid ProjectId { get; set; }
    
    [Required]
    public Guid CreatedBy { get; set; }
    
    public TaskYagdaylar Status { get; set; } = TaskYagdaylar.Todo;
    
    public Priority Priority { get; set; } = Priority.Medium;
    
    public DateTime? DueDate { get; set; }
    
    public decimal? EstimatedHours { get; set; }
    
    public decimal? ActualHours { get; set; }
    
    public string Tags { get; set; } = string.Empty; // JSON array
    
    public int Order { get; set; } = 0; // For Kanban ordering
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public Project Project { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<TaskAssignee> Assignees { get; set; } = new List<TaskAssignee>();
    public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}

public enum TaskYagdaylar
{
    Todo = 0,
    InProgress = 1,
    Review = 2,
    Done = 3
}