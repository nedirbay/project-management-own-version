namespace ProjectManager.API.Models;

public class TaskAssignee
{
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AssignedAt { get; set; }

    // Navigation
    public Yumus Task { get; set; } = null!;
    public User User { get; set; } = null!;
}