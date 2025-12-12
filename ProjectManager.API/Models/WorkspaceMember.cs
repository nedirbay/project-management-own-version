namespace ProjectManager.API.Models;

public class WorkspaceMember
{
    public Guid WorkspaceId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; }

    // Navigation
    public Workspace Workspace { get; set; } = null!;
    public User User { get; set; } = null!;
}