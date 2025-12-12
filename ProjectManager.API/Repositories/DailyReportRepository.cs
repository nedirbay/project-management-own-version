using Microsoft.EntityFrameworkCore;
using ProjectManager.API.Data;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories.Interfaces;

namespace ProjectManager.API.Repositories;

public class DailyReportRepository : BaseRepository, IDailyReportRepository
{
    public DailyReportRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DailyReport>> GetAllAsync()
    {
        return await _context.DailyReports
            .Include(dr => dr.User)
            .Include(dr => dr.Workspace)
            .Include(dr => dr.Project)
            .ToListAsync();
    }

    public async Task<DailyReport?> GetByIdAsync(Guid id)
    {
        return await _context.DailyReports
            .Include(dr => dr.User)
            .Include(dr => dr.Workspace)
            .Include(dr => dr.Project)
            .FirstOrDefaultAsync(dr => dr.Id == id);
    }

    public async Task<DailyReport> CreateAsync(DailyReport report)
    {
        report.CreatedAt = DateTime.UtcNow;
        report.UpdatedAt = DateTime.UtcNow;
        _context.DailyReports.Add(report);
        await SaveChangesAsync();
        return report;
    }

    public async Task<DailyReport> UpdateAsync(DailyReport report)
    {
        report.UpdatedAt = DateTime.UtcNow;
        _context.DailyReports.Update(report);
        await SaveChangesAsync();
        return report;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var report = await _context.DailyReports.FindAsync(id);
        if (report == null) return false;

        _context.DailyReports.Remove(report);
        return await SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.DailyReports.AnyAsync(dr => dr.Id == id);
    }

    public async Task<IEnumerable<DailyReport>> GetByUserIdAsync(Guid userId)
    {
        return await _context.DailyReports
            .Include(dr => dr.User)
            .Include(dr => dr.Workspace)
            .Include(dr => dr.Project)
            .Where(dr => dr.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyReport>> GetByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context.DailyReports
            .Include(dr => dr.User)
            .Include(dr => dr.Workspace)
            .Include(dr => dr.Project)
            .Where(dr => dr.WorkspaceId == workspaceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyReport>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.DailyReports
            .Include(dr => dr.User)
            .Include(dr => dr.Workspace)
            .Include(dr => dr.Project)
            .Where(dr => dr.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<DailyReport?> GetByUserAndDateAsync(Guid userId, DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.DailyReports
            .Include(dr => dr.User)
            .Include(dr => dr.Workspace)
            .Include(dr => dr.Project)
            .FirstOrDefaultAsync(dr => dr.UserId == userId && dr.Date.Date == dateOnly);
    }

    public async Task<bool> ReportExistsForDateAsync(Guid userId, DateTime date)
    {
        var dateOnly = date.Date;
        return await _context.DailyReports
            .AnyAsync(dr => dr.UserId == userId && dr.Date.Date == dateOnly);
    }
}