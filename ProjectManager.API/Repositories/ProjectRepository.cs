using Microsoft.EntityFrameworkCore;
using ProjectManager.API.Data;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories.Interfaces;

namespace ProjectManager.API.Repositories;

public class ProjectRepository : BaseRepository, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects.ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateAsync(Project project)
    {
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Add(project);
        await SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Update(project);
        await SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return false;

        // Soft delete
        project.IsActive = false;
        project.UpdatedAt = DateTime.UtcNow;
        _context.Projects.Update(project);
        return await SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Projects.AnyAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Owner)
            .Where(p => p.WorkspaceId == workspaceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetMembersAsync(Guid projectId)
    {
        return await _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Include(pm => pm.User)
            .Select(pm => pm.User)
            .ToListAsync();
    }

    public async Task<bool> AddMemberAsync(Guid projectId, Guid userId)
    {
        var projectMember = new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(projectMember);
        return await SaveChangesAsync();
    }

    public async Task<bool> RemoveMemberAsync(Guid projectId, Guid userId)
    {
        var projectMember = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);

        if (projectMember == null) return false;

        _context.ProjectMembers.Remove(projectMember);
        return await SaveChangesAsync();
    }

    public async Task<int> GetTaskCountAsync(Guid projectId)
    {
        return await _context.Yumuses
            .CountAsync(t => t.ProjectId == projectId);
    }

    public async Task<int> GetCompletedTaskCountAsync(Guid projectId)
    {
        return await _context.Yumuses
            .CountAsync(t => t.ProjectId == projectId && t.Status == TaskYagdaylar.Done);
    }
}