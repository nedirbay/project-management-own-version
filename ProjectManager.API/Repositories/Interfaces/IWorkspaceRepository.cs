using ProjectManager.API.Models;

namespace ProjectManager.API.Repositories.Interfaces;

public interface IWorkspaceRepository
{
    Task<IEnumerable<Workspace>> GetAllAsync();
    Task<Workspace?> GetByIdAsync(Guid id);
    Task<Workspace> CreateAsync(Workspace workspace);
    Task<Workspace> UpdateAsync(Workspace workspace);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Workspace>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<User>> GetMembersAsync(Guid workspaceId);
    Task<bool> AddMemberAsync(Guid workspaceId, Guid userId);
    Task<bool> RemoveMemberAsync(Guid workspaceId, Guid userId);
    Task<int> GetMemberCountAsync(Guid workspaceId);
    Task<int> GetProjectCountAsync(Guid workspaceId);
}