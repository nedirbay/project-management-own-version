using ProjectManager.API.Models;

namespace ProjectManager.API.Repositories.Interfaces;

public interface IDailyReportRepository
{
    Task<IEnumerable<DailyReport>> GetAllAsync();
    Task<DailyReport?> GetByIdAsync(Guid id);
    Task<DailyReport> CreateAsync(DailyReport report);
    Task<DailyReport> UpdateAsync(DailyReport report);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<DailyReport>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<DailyReport>> GetByWorkspaceIdAsync(Guid workspaceId);
    Task<IEnumerable<DailyReport>> GetByProjectIdAsync(Guid projectId);
    Task<DailyReport?> GetByUserAndDateAsync(Guid userId, DateTime date);
    Task<bool> ReportExistsForDateAsync(Guid userId, DateTime date);
}