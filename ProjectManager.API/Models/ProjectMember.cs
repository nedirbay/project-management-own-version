namespace ProjectManager.API.Models;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}