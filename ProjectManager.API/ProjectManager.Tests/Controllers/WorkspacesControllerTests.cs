using Dinfo.Test.helpers;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.Controllers;
using ProjectManager.API.DTOs;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Controllers;

public class WorkspacesControllerTests : ControllerTestBase
{
    private readonly WorkspaceRepository _workspaceRepository;
    private readonly UserRepository _userRepository;

    public WorkspacesControllerTests()
    {
        _workspaceRepository = new WorkspaceRepository(Context);
        _userRepository = new UserRepository(Context);
    }

    [Fact]
    public async Task GetWorkspace_ShouldReturnWorkspaceForAdmin()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");
        var workspace = (await _workspaceRepository.GetAllAsync()).First();

        var controller = new WorkspacesController(_workspaceRepository, _userRepository, Mapper);
        SetUser(controller, admin);

        var result = await controller.GetWorkspace(workspace.Id);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var workspaceDto = Assert.IsAssignableFrom<WorkspaceDetailDto>(okResult.Value);
        Assert.Equal(workspace.Id, workspaceDto.Id);
        Assert.Equal(workspace.Name, workspaceDto.Name);
    }
}

