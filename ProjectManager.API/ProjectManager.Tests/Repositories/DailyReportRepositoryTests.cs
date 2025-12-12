using System.Linq;
using Dinfo.Test.helpers;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Repositories;

public class DailyReportRepositoryTests : TestBase
{
    private readonly DailyReportRepository _dailyReportRepository;
    private readonly WorkspaceRepository _workspaceRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly UserRepository _userRepository;

    public DailyReportRepositoryTests()
    {
        _dailyReportRepository = new DailyReportRepository(Context);
        _workspaceRepository = new WorkspaceRepository(Context);
        _projectRepository = new ProjectRepository(Context);
        _userRepository = new UserRepository(Context);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistReport()
    {
        var (workspace, project, user) = await CreateProjectForReportAsync();

        var report = new DailyReport
        {
            UserId = user.Id,
            Date = DateTime.UtcNow.Date,
            WorkspaceId = workspace.Id,
            ProjectId = project.Id,
            WorkDescription = "Completed tasks",
            YumusesCompleted = string.Empty,
            Notes = "Integration test"
        };

        var created = await _dailyReportRepository.CreateAsync(report);
        var fetched = await _dailyReportRepository.GetByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal(report.WorkDescription, fetched!.WorkDescription);
        Assert.Equal(user.Id, fetched.UserId);
    }

    [Fact]
    public async Task GetByUserAndDateAsync_ShouldReturnReport()
    {
        var (workspace, project, user) = await CreateProjectForReportAsync();
        var reportDate = DateTime.UtcNow.Date;

        Context.DailyReports.Add(new DailyReport
        {
            UserId = user.Id,
            Date = reportDate,
            WorkspaceId = workspace.Id,
            ProjectId = project.Id,
            WorkDescription = "Daily summary",
            YumusesCompleted = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var report = await _dailyReportRepository.GetByUserAndDateAsync(user.Id, reportDate);

        Assert.NotNull(report);
        Assert.Equal("Daily summary", report!.WorkDescription);
    }

    [Fact]
    public async Task ReportExistsForDateAsync_ShouldReturnTrueWhenReportExists()
    {
        var (workspace, project, user) = await CreateProjectForReportAsync();
        var reportDate = DateTime.UtcNow.Date;

        Context.DailyReports.Add(new DailyReport
        {
            UserId = user.Id,
            Date = reportDate,
            WorkspaceId = workspace.Id,
            ProjectId = project.Id,
            WorkDescription = "Report exists",
            YumusesCompleted = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var exists = await _dailyReportRepository.ReportExistsForDateAsync(user.Id, reportDate);
        Assert.True(exists);
    }

    private async Task<(Workspace workspace, Project project, User user)> CreateProjectForReportAsync()
    {
        var workspace = (await _workspaceRepository.GetAllAsync()).First();
        var user = await _userRepository.GetByUsernameAsync("admin")
                   ?? throw new InvalidOperationException("Seeded admin user not found.");

        var project = await _projectRepository.CreateAsync(new Project
        {
            Name = $"Daily Report Project {Guid.NewGuid():N}",
            WorkspaceId = workspace.Id,
            OwnerId = user.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#654321",
            Tags = "daily"
        });

        return (workspace, project, user);
    }
}


