using ProjectManager.API.Models;

namespace ProjectManager.API.Repositories.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<Yumus>> GetAllAsync();
    Task<Yumus?> GetByIdAsync(Guid id);
    Task<Yumus> CreateAsync(Yumus task);
    Task<Yumus> UpdateAsync(Yumus task);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Yumus>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<User>> GetAssigneesAsync(Guid taskId);
    Task<bool> AssignToUserAsync(Guid taskId, Guid userId);
    Task<bool> RemoveAssigneeAsync(Guid taskId, Guid userId);
    Task<IEnumerable<SubTask>> GetSubTasksAsync(Guid taskId);
    Task<IEnumerable<TaskComment>> GetCommentsAsync(Guid taskId);
    Task<IEnumerable<TaskAttachment>> GetAttachmentsAsync(Guid taskId);
    Task<int> GetSubTaskCountAsync(Guid taskId);
    Task<int> GetCompletedSubTaskCountAsync(Guid taskId);
}