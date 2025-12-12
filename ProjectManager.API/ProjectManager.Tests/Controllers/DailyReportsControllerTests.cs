using Dinfo.Test.helpers;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.Controllers;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Controllers;

public class DailyReportsControllerTests : ControllerTestBase
{
    private readonly DailyReportsController _controller;
    private readonly DailyReportRepository _dailyReportRepository;
    private readonly WorkspaceRepository _workspaceRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly UserRepository _userRepository;

    public DailyReportsControllerTests()
    {
        _dailyReportRepository = new DailyReportRepository(Context);
        _workspaceRepository = new WorkspaceRepository(Context);
        _projectRepository = new ProjectRepository(Context);
        _userRepository = new UserRepository(Context);

        _controller = new DailyReportsController(
            _dailyReportRepository,
            _workspaceRepository,
            _projectRepository,
            _userRepository,
            Mapper);
    }

    [Fact]
    public async Task CreateReport_ShouldReturnCreatedReport()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");
        var workspace = (await _workspaceRepository.GetAllAsync()).First();
        var project = await _projectRepository.CreateAsync(new Project
        {
            Name = $"Report Project {Guid.NewGuid():N}",
            WorkspaceId = workspace.Id,
            OwnerId = admin.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#cccccc",
            Tags = "daily"
        });

        SetUser(_controller, admin);

        var dto = new CreateDailyReportDto
        {
            Date = DateTime.UtcNow.Date,
            WorkspaceId = workspace.Id,
            ProjectId = project.Id,
            WorkDescription = "Completed integration tasks",
            YumusesCompleted = new List<Guid>(),
            Notes = "Controller test"
        };

        var result = await _controller.CreateReport(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var reportDto = Assert.IsType<DailyReportDto>(created.Value);
        Assert.Equal(dto.WorkDescription, reportDto.WorkDescription);
        Assert.Equal(admin.Id, reportDto.UserId);
    }

    [Fact]
    public async Task GetMyReports_ShouldReturnReportsForCurrentUser()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");
        var workspace = (await _workspaceRepository.GetAllAsync()).First();

        Context.DailyReports.Add(new DailyReport
        {
            UserId = admin.Id,
            Date = DateTime.UtcNow.Date,
            WorkspaceId = workspace.Id,
            WorkDescription = "My report",
            YumusesCompleted = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        SetUser(_controller, admin);

        var result = await _controller.GetMyReports();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var reports = Assert.IsAssignableFrom<IEnumerable<DailyReportDto>>(okResult.Value);
        Assert.NotEmpty(reports);
    }
}


