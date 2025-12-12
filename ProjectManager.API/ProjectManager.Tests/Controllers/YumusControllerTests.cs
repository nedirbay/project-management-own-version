using Dinfo.Test.helpers;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.Controllers;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Controllers;

public class YumusControllerTests : ControllerTestBase
{
    private readonly YumusController _controller;
    private readonly TaskRepository _taskRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly WorkspaceRepository _workspaceRepository;
    private readonly UserRepository _userRepository;

    public YumusControllerTests()
    {
        _taskRepository = new TaskRepository(Context);
        _projectRepository = new ProjectRepository(Context);
        _workspaceRepository = new WorkspaceRepository(Context);
        _userRepository = new UserRepository(Context);

        _controller = new YumusController(
            _taskRepository,
            _projectRepository,
            _workspaceRepository,
            _userRepository,
            Mapper);
    }

    [Fact]
    public async Task CreateYumus_ShouldReturnCreatedTask()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");
        var workspace = (await _workspaceRepository.GetAllAsync()).First();

        var project = await _projectRepository.CreateAsync(new Project
        {
            Name = $"Task Controller Project {Guid.NewGuid():N}",
            WorkspaceId = workspace.Id,
            OwnerId = admin.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#ff9900",
            Tags = "yumus"
        });

        SetUser(_controller, admin);

        var dto = new CreateYumusDto
        {
            Title = "Controller Task",
            Description = "Created via YumusController test",
            ProjectId = project.Id,
            Status = TaskYagdaylar.Todo.ToString(),
            Priority = Priority.High.ToString(),
            Tags = new List<string> { "controller" },
            AssigneeIds = new List<Guid>()
        };

        var result = await _controller.CreateYumus(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var taskDto = Assert.IsType<YumusDto>(created.Value);
        Assert.Equal(dto.Title, taskDto.Title);
        Assert.Equal(project.Id, taskDto.ProjectId);
    }

    [Fact]
    public async Task GetYumus_ShouldReturnTaskForAuthorizedUser()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");
        var workspace = (await _workspaceRepository.GetAllAsync()).First();

        var project = await _projectRepository.CreateAsync(new Project
        {
            Name = $"Existing Task Project {Guid.NewGuid():N}",
            WorkspaceId = workspace.Id,
            OwnerId = admin.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#00ffaa",
            Tags = "yumus"
        });

        var task = await _taskRepository.CreateAsync(new Yumus
        {
            Title = "Existing Task",
            Description = "Seeded for controller test",
            ProjectId = project.Id,
            CreatedBy = admin.Id,
            Status = TaskYagdaylar.Todo,
            Priority = Priority.Low,
            Tags = string.Empty
        });

        SetUser(_controller, admin);

        var result = await _controller.GetYumus(task.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var detail = Assert.IsType<YumusDetailDto>(okResult.Value);
        Assert.Equal(task.Id, detail.Id);
        Assert.Equal(task.Title, detail.Title);
    }
}


