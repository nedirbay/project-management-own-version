using ProjectManager.API.Models;

namespace ProjectManager.API.Repositories.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync();
    Task<Project?> GetByIdAsync(Guid id);
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Project>> GetByWorkspaceIdAsync(Guid workspaceId);
    Task<IEnumerable<User>> GetMembersAsync(Guid projectId);
    Task<bool> AddMemberAsync(Guid projectId, Guid userId);
    Task<bool> RemoveMemberAsync(Guid projectId, Guid userId);
    Task<int> GetTaskCountAsync(Guid projectId);
    Task<int> GetCompletedTaskCountAsync(Guid projectId);
}