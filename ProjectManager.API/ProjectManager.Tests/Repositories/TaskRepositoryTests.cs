using System.Linq;
using Dinfo.Test.helpers;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Repositories;

public class TaskRepositoryTests : TestBase
{
    private readonly TaskRepository _taskRepository;
    private readonly ProjectRepository _projectRepository;
    private readonly WorkspaceRepository _workspaceRepository;
    private readonly UserRepository _userRepository;

    public TaskRepositoryTests()
    {
        _taskRepository = new TaskRepository(Context);
        _projectRepository = new ProjectRepository(Context);
        _workspaceRepository = new WorkspaceRepository(Context);
        _userRepository = new UserRepository(Context);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistTask()
    {
        var (project, creator) = await CreateProjectAsync();

        var task = new Yumus
        {
            Title = "Integration Task",
            Description = "Created via integration test",
            ProjectId = project.Id,
            CreatedBy = creator.Id,
            Status = TaskYagdaylar.Todo,
            Priority = Priority.Medium,
            Tags = "integration"
        };

        var created = await _taskRepository.CreateAsync(task);
        var fetched = await _taskRepository.GetByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal(task.Title, fetched!.Title);
        Assert.Equal(project.Id, fetched.ProjectId);
    }

    [Fact]
    public async Task AssignToUserAsync_ShouldAddAssignee()
    {
        var (project, creator) = await CreateProjectAsync();
        var task = await _taskRepository.CreateAsync(new Yumus
        {
            Title = "Task With Assignee",
            ProjectId = project.Id,
            CreatedBy = creator.Id,
            Status = TaskYagdaylar.Todo,
            Priority = Priority.Low,
            Tags = string.Empty
        });

        var assignee = await _userRepository.CreateAsync(new User
        {
            Username = $"task_assignee_{Guid.NewGuid():N}",
            Email = $"{Guid.NewGuid():N}@example.com",
            FullName = "Task Assignee",
            PasswordHash = "hashed-password",
            Role = UserRole.Member,
            IsActive = true
        });

        var assigned = await _taskRepository.AssignToUserAsync(task.Id, assignee.Id);
        var assignees = await _taskRepository.GetAssigneesAsync(task.Id);

        Assert.True(assigned);
        Assert.Contains(assignees, a => a.Id == assignee.Id);
    }

    [Fact]
    public async Task GetSubTaskCountAsync_ShouldReturnNumberOfSubtasks()
    {
        var (project, creator) = await CreateProjectAsync();
        var task = await _taskRepository.CreateAsync(new Yumus
        {
            Title = "Task With Subtasks",
            ProjectId = project.Id,
            CreatedBy = creator.Id,
            Status = TaskYagdaylar.Todo,
            Priority = Priority.Medium,
            Tags = string.Empty
        });

        Context.SubTasks.Add(new SubTask
        {
            TaskId = task.Id,
            Title = "Subtask 1",
            CreatedAt = DateTime.UtcNow
        });
        Context.SubTasks.Add(new SubTask
        {
            TaskId = task.Id,
            Title = "Subtask 2",
            CreatedAt = DateTime.UtcNow
        });
        await Context.SaveChangesAsync();

        var count = await _taskRepository.GetSubTaskCountAsync(task.Id);
        Assert.Equal(2, count);
    }

    private async Task<(Project project, User owner)> CreateProjectAsync()
    {
        var workspace = (await _workspaceRepository.GetAllAsync()).First();
        var owner = await _userRepository.GetByUsernameAsync("admin")
                    ?? throw new InvalidOperationException("Seeded admin user not found.");

        var project = await _projectRepository.CreateAsync(new Project
        {
            Name = $"Task Repo Project {Guid.NewGuid():N}",
            WorkspaceId = workspace.Id,
            OwnerId = owner.Id,
            Status = ProjectStatus.Planning,
            Priority = Priority.Medium,
            StartDate = DateTime.UtcNow,
            Color = "#123456",
            Tags = "task"
        });

        return (project, owner);
    }
}


