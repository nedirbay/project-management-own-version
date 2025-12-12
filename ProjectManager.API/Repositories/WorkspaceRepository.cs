using Microsoft.EntityFrameworkCore;
using ProjectManager.API.Data;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories.Interfaces;

namespace ProjectManager.API.Repositories;

public class WorkspaceRepository : BaseRepository, IWorkspaceRepository
{
    public WorkspaceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Workspace>> GetAllAsync()
    {
        return await _context.Workspaces.ToListAsync();
    }

    public async Task<Workspace?> GetByIdAsync(Guid id)
    {
        return await _context.Workspaces
            .Include(w => w.Owner)
            .Include(w => w.Admin)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Workspace> CreateAsync(Workspace workspace)
    {
        workspace.CreatedAt = DateTime.UtcNow;
        workspace.UpdatedAt = DateTime.UtcNow;
        _context.Workspaces.Add(workspace);
        await SaveChangesAsync();
        return workspace;
    }

    public async Task<Workspace> UpdateAsync(Workspace workspace)
    {
        workspace.UpdatedAt = DateTime.UtcNow;
        _context.Workspaces.Update(workspace);
        await SaveChangesAsync();
        return workspace;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null) return false;

        // Soft delete
        workspace.IsActive = false;
        workspace.UpdatedAt = DateTime.UtcNow;
        _context.Workspaces.Update(workspace);
        return await SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Workspaces.AnyAsync(w => w.Id == id);
    }

    public async Task<IEnumerable<Workspace>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Workspaces
            .Include(w => w.Owner)
            .Include(w => w.Admin)
            .Where(w => w.Members.Any(m => m.UserId == userId) || w.OwnerId == userId || w.AdminId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetMembersAsync(Guid workspaceId)
    {
        return await _context.WorkspaceMembers
            .Where(wm => wm.WorkspaceId == workspaceId)
            .Include(wm => wm.User)
            .Select(wm => wm.User)
            .ToListAsync();
    }

    public async Task<bool> AddMemberAsync(Guid workspaceId, Guid userId)
    {
        var workspaceMember = new WorkspaceMember
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _context.WorkspaceMembers.Add(workspaceMember);
        return await SaveChangesAsync();
    }

    public async Task<bool> RemoveMemberAsync(Guid workspaceId, Guid userId)
    {
        var workspaceMember = await _context.WorkspaceMembers
            .FirstOrDefaultAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userId);

        if (workspaceMember == null) return false;

        _context.WorkspaceMembers.Remove(workspaceMember);
        return await SaveChangesAsync();
    }

    public async Task<int> GetMemberCountAsync(Guid workspaceId)
    {
        return await _context.WorkspaceMembers
            .CountAsync(wm => wm.WorkspaceId == workspaceId);
    }

    public async Task<int> GetProjectCountAsync(Guid workspaceId)
    {
        return await _context.Projects
            .CountAsync(p => p.WorkspaceId == workspaceId);
    }
}