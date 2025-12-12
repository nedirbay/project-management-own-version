using System.Linq;
using Dinfo.Test.helpers;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Repositories;

public class ProjectRepositoryTests : TestBase
{
    private readonly ProjectRepository _projectRepository;
    private readonly WorkspaceRepository _workspaceRepository;
    private readonly UserRepository _userRepository;

    public ProjectRepositoryTests()
    {
        _projectRepository = new ProjectRepository(Context);
        _workspaceRepository = new WorkspaceRepository(Context);
        _userRepository = new UserRepository(Context);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistProject()
    {
        var workspace = (await _workspaceRepository.GetAllAsync()).First();
        var owner = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");

        var project = new Project
        {
            Name = "Integration Project",
            Description = "Created via integration test",
            WorkspaceId = workspace.Id,
            OwnerId = owner.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#FF0000",
            Tags = "integration"
        };

        var created = await _projectRepository.CreateAsync(project);
        var fetched = await _projectRepository.GetByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal(project.Name, fetched!.Name);
        Assert.Equal(workspace.Id, fetched.WorkspaceId);
    }

    [Fact]
    public async Task AddMemberAsync_ShouldAddUserToProject()
    {
        var workspace = (await _workspaceRepository.GetAllAsync()).First();
        var owner = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");

        var project = await _projectRepository.CreateAsync(new Project
        {
            Name = "Project With Member",
            WorkspaceId = workspace.Id,
            OwnerId = owner.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#00FF00",
            Tags = "member"
        });

        var newUser = await _userRepository.CreateAsync(new User
        {
            Username = $"project_member_{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@example.com",
            FullName = "Project Member",
            PasswordHash = "hashed-password",
            Role = UserRole.Member,
            IsActive = true
        });

        var added = await _projectRepository.AddMemberAsync(project.Id, newUser.Id);
        var members = await _projectRepository.GetMembersAsync(project.Id);

        Assert.True(added);
        Assert.Contains(members, m => m.Id == newUser.Id);
    }

    [Fact]
    public async Task GetTaskCountAsync_ShouldReturnNumberOfTasks()
    {
        var workspace = (await _workspaceRepository.GetAllAsync()).First();
        var owner = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");

        var project = await _projectRepository.CreateAsync(new Project
        {
            Name = "Project With Tasks",
            WorkspaceId = workspace.Id,
            OwnerId = owner.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#0000FF",
            Tags = "tasks"
        });

        var task = new Yumus
        {
            Title = "Sample Task",
            Description = "Task for count",
            ProjectId = project.Id,
            CreatedBy = owner.Id,
            Status = TaskYagdaylar.Todo,
            Priority = Priority.Medium,
            Tags = string.Empty
        };

        Context.Yumuses.Add(task);
        await Context.SaveChangesAsync();

        var count = await _projectRepository.GetTaskCountAsync(project.Id);
        Assert.Equal(1, count);
    }
}


