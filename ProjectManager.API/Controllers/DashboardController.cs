using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.Repositories.Interfaces;
using System.Security.Claims;

namespace ProjectManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IDailyReportRepository _dailyReportRepository;

    public DashboardController(
        IWorkspaceRepository workspaceRepository,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        IDailyReportRepository dailyReportRepository)
    {
        _workspaceRepository = workspaceRepository;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _dailyReportRepository = dailyReportRepository;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetDashboardData()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Get workspaces
        var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
        var workspaceCount = workspaces.Count();

        // Get projects
        var projects = new List<Models.Project>();
        foreach (var workspace in workspaces)
        {
            var workspaceProjects = await _projectRepository.GetByWorkspaceIdAsync(workspace.Id);
            projects.AddRange(workspaceProjects);
        }
        var projectCount = projects.Count;

        // Get tasks
        var tasks = new List<Models.Yumus>();
        foreach (var project in projects)
        {
            var projectTasks = await _taskRepository.GetByProjectIdAsync(project.Id);
            tasks.AddRange(projectTasks);
        }
        
        var taskCount = tasks.Count;
        var completedTaskCount = tasks.Count(t => t.Status == Models.TaskYagdaylar.Done);
        var inProgressTaskCount = tasks.Count(t => t.Status == Models.TaskYagdaylar.InProgress);
        var overdueTaskCount = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != Models.TaskYagdaylar.Done);

        // Get recent reports
        var recentReports = await _dailyReportRepository.GetByUserIdAsync(currentUserId);
        var recentReportsList = recentReports.OrderByDescending(r => r.Date).Take(5).ToList();

        return Ok(new
        {
            Summary = new
            {
                WorkspaceCount = workspaceCount,
                ProjectCount = projectCount,
                TaskCount = taskCount,
                CompletedTaskCount = completedTaskCount,
                InProgressTaskCount = inProgressTaskCount,
                OverdueTaskCount = overdueTaskCount,
                CompletionRate = taskCount > 0 ? (double)completedTaskCount / taskCount * 100 : 0
            },
            RecentReports = recentReportsList.Select(r => new
            {
                r.Id,
                r.Date,
                r.WorkDescription,
                WorkspaceName = r.Workspace.Name
            }),
            UpcomingDeadlines = tasks
                .Where(t => t.DueDate.HasValue && t.DueDate.Value >= DateTime.UtcNow && t.Status != Models.TaskYagdaylar.Done)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.DueDate,
                    ProjectName = t.Project.Name
                })
        });
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetStats()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Get workspaces
        var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
        var workspaceCount = workspaces.Count();

        // Get projects
        var projects = new List<Models.Project>();
        foreach (var workspace in workspaces)
        {
            var workspaceProjects = await _projectRepository.GetByWorkspaceIdAsync(workspace.Id);
            projects.AddRange(workspaceProjects);
        }
        var projectCount = projects.Count;

        // Get tasks
        var tasks = new List<Models.Yumus>();
        foreach (var project in projects)
        {
            var projectTasks = await _taskRepository.GetByProjectIdAsync(project.Id);
            tasks.AddRange(projectTasks);
        }
        
        var taskCount = tasks.Count;
        var completedTaskCount = tasks.Count(t => t.Status == Models.TaskYagdaylar.Done);
        var inProgressTaskCount = tasks.Count(t => t.Status == Models.TaskYagdaylar.InProgress);
        var overdueTaskCount = tasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.Status != Models.TaskYagdaylar.Done);

        return Ok(new
        {
            WorkspaceCount = workspaceCount,
            ProjectCount = projectCount,
            TaskCount = taskCount,
            CompletedTaskCount = completedTaskCount,
            InProgressTaskCount = inProgressTaskCount,
            OverdueTaskCount = overdueTaskCount,
            CompletionRate = taskCount > 0 ? (double)completedTaskCount / taskCount * 100 : 0
        });
    }

    [HttpGet("recent-activities")]
    public async Task<ActionResult<object>> GetRecentActivities()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Get recent reports
        var recentReports = await _dailyReportRepository.GetByUserIdAsync(currentUserId);
        var recentReportsList = recentReports.OrderByDescending(r => r.CreatedAt).Take(10).ToList();

        // Get recently updated tasks
        var tasks = new List<Models.Yumus>();
        var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
        foreach (var workspace in workspaces)
        {
            var projects = await _projectRepository.GetByWorkspaceIdAsync(workspace.Id);
            foreach (var project in projects)
            {
                var projectTasks = await _taskRepository.GetByProjectIdAsync(project.Id);
                tasks.AddRange(projectTasks);
            }
        }
        
        var recentTasks = tasks.OrderByDescending(t => t.UpdatedAt).Take(10).ToList();

        return Ok(new
        {
            RecentReports = recentReportsList.Select(r => new
            {
                r.Id,
                r.Date,
                r.WorkDescription,
                WorkspaceName = r.Workspace.Name,
                r.CreatedAt
            }),
            RecentTasks = recentTasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Status,
                ProjectName = t.Project.Name,
                t.UpdatedAt
            })
        });
    }

    [HttpGet("upcoming-deadlines")]
    public async Task<ActionResult<object>> GetUpcomingDeadlines()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Get tasks with upcoming deadlines
        var tasks = new List<Models.Yumus>();
        var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
        foreach (var workspace in workspaces)
        {
            var projects = await _projectRepository.GetByWorkspaceIdAsync(workspace.Id);
            foreach (var project in projects)
            {
                var projectTasks = await _taskRepository.GetByProjectIdAsync(project.Id);
                tasks.AddRange(projectTasks);
            }
        }
        
        var upcomingTasks = tasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value >= DateTime.UtcNow && t.Status != Models.TaskYagdaylar.Done)
            .OrderBy(t => t.DueDate)
            .Take(10)
            .ToList();

        return Ok(upcomingTasks.Select(t => new
        {
            t.Id,
            t.Title,
            t.DueDate,
            t.Priority,
            ProjectName = t.Project.Name,
            WorkspaceName = t.Project.Workspace.Name
        }));
    }
}