using Dinfo.Test.helpers;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.Controllers;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories;
using System.Linq;

namespace ProjectManager.Tests.Controllers;

public class ProjectsControllerTests : ControllerTestBase
{
    private readonly ProjectsController _controller;
    private readonly ProjectRepository _projectRepository;
    private readonly WorkspaceRepository _workspaceRepository;
    private readonly UserRepository _userRepository;

    public ProjectsControllerTests()
    {
        _projectRepository = new ProjectRepository(Context);
        _workspaceRepository = new WorkspaceRepository(Context);
        _userRepository = new UserRepository(Context);
        _controller = new ProjectsController(_projectRepository, _workspaceRepository, _userRepository, Mapper);
    }

    [Fact]
    public async Task GetProject_ShouldReturnProjectForAdmin()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");
        var workspace = (await _workspaceRepository.GetAllAsync()).First();

        var project = await _projectRepository.CreateAsync(new Project
        {
            Name = "Controller Project",
            Description = "Project for controller test",
            WorkspaceId = workspace.Id,
            OwnerId = admin.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#abcdef",
            Tags = "controller"
        });

        SetUser(_controller, admin);

        var result = await _controller.GetProject(project.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var detail = Assert.IsType<ProjectDetailDto>(okResult.Value);
        Assert.Equal(project.Id, detail.Id);
        Assert.Equal(project.Name, detail.Name);
    }

    [Fact]
    public async Task CreateProject_ShouldReturnCreatedProject()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");
        var workspace = (await _workspaceRepository.GetAllAsync()).First();

        SetUser(_controller, admin);

        var dto = new CreateProjectDto
        {
            Name = "New Project",
            Description = "Created via controller test",
            WorkspaceId = workspace.Id,
            Status = ProjectStatus.Planning.ToString(),
            Priority = Priority.High.ToString(),
            StartDate = DateTime.UtcNow,
            Color = "#123123",
            Tags = new List<string> { "api", "test" },
            MemberIds = new List<Guid>()
        };

        var result = await _controller.CreateProject(dto);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var projectDto = Assert.IsType<ProjectDto>(created.Value);
        Assert.Equal(dto.Name, projectDto.Name);
        Assert.Equal(workspace.Id, projectDto.WorkspaceId);
    }
}


