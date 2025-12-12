using System.Linq;
using Dinfo.Test.helpers;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Repositories;

public class WorkspaceRepositoryTests : TestBase
{
    private readonly WorkspaceRepository _sut;
    private readonly UserRepository _userRepository;

    public WorkspaceRepositoryTests()
    {
        _sut = new WorkspaceRepository(Context);
        _userRepository = new UserRepository(Context);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnSeededWorkspaceForAdmin()
    {
        var admin = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");

        var workspaces = await _sut.GetByUserIdAsync(admin.Id);

        Assert.NotEmpty(workspaces);
    }

    [Fact]
    public async Task AddMemberAsync_ShouldIncreaseMemberCount()
    {
        var workspace = (await _sut.GetAllAsync()).First();
        var unique = Guid.NewGuid().ToString("N");

        var newUser = new User
        {
            Username = $"workspace_member_{unique}",
            Email = $"{unique}@example.com",
            FullName = "Workspace Member",
            PasswordHash = "hashed-password",
            Role = UserRole.Member,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(newUser);
        var beforeCount = await _sut.GetMemberCountAsync(workspace.Id);

        var added = await _sut.AddMemberAsync(workspace.Id, createdUser.Id);
        var afterCount = await _sut.GetMemberCountAsync(workspace.Id);

        Assert.True(added);
        Assert.Equal(beforeCount + 1, afterCount);
    }
}

