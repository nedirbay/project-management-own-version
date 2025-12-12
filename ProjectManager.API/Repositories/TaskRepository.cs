using Microsoft.EntityFrameworkCore;
using ProjectManager.API.Data;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories.Interfaces;

namespace ProjectManager.API.Repositories;

public class TaskRepository : BaseRepository, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Yumus>> GetAllAsync()
    {
        return await _context.Yumuses.ToListAsync();
    }

    public async Task<Yumus?> GetByIdAsync(Guid id)
    {
        return await _context.Yumuses
            .Include(t => t.Project)
            .Include(t => t.Creator)
            .Include(t => t.Assignees)!
                .ThenInclude(ta => ta.User)
            .Include(t => t.Comments)!
                .ThenInclude(tc => tc.User)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Yumus> CreateAsync(Yumus task)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;
        _context.Yumuses.Add(task);
        await SaveChangesAsync();
        return task;
    }

    public async Task<Yumus> UpdateAsync(Yumus task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _context.Yumuses.Update(task);
        await SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var task = await _context.Yumuses.FindAsync(id);
        if (task == null) return false;

        // Soft delete
        task.IsActive = false;
        task.UpdatedAt = DateTime.UtcNow;
        _context.Yumuses.Update(task);
        return await SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Yumuses.AnyAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Yumus>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Yumuses
            .Include(t => t.Project)
            .Include(t => t.Creator)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAssigneesAsync(Guid taskId)
    {
        return await _context.TaskAssignees
            .Where(ta => ta.TaskId == taskId)
            .Include(ta => ta.User)
            .Select(ta => ta.User)
            .ToListAsync();
    }

    public async Task<bool> AssignToUserAsync(Guid taskId, Guid userId)
    {
        var taskAssignee = new TaskAssignee
        {
            TaskId = taskId,
            UserId = userId,
            AssignedAt = DateTime.UtcNow
        };

        _context.TaskAssignees.Add(taskAssignee);
        return await SaveChangesAsync();
    }

    public async Task<bool> RemoveAssigneeAsync(Guid taskId, Guid userId)
    {
        var taskAssignee = await _context.TaskAssignees
            .FirstOrDefaultAsync(ta => ta.TaskId == taskId && ta.UserId == userId);

        if (taskAssignee == null) return false;

        _context.TaskAssignees.Remove(taskAssignee);
        return await SaveChangesAsync();
    }

    public async Task<IEnumerable<SubTask>> GetSubTasksAsync(Guid taskId)
    {
        return await _context.SubTasks
            .Where(st => st.TaskId == taskId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskComment>> GetCommentsAsync(Guid taskId)
    {
        return await _context.TaskComments
            .Include(tc => tc.User)
            .Where(tc => tc.TaskId == taskId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskAttachment>> GetAttachmentsAsync(Guid taskId)
    {
        return await _context.TaskAttachments
            .Where(ta => ta.TaskId == taskId)
            .ToListAsync();
    }

    public async Task<int> GetSubTaskCountAsync(Guid taskId)
    {
        return await _context.SubTasks
            .CountAsync(st => st.TaskId == taskId);
    }

    public async Task<int> GetCompletedSubTaskCountAsync(Guid taskId)
    {
        return await _context.SubTasks
            .CountAsync(st => st.TaskId == taskId && st.IsCompleted);
    }
}