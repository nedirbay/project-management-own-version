using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories.Interfaces;
using AutoMapper;
using System.Security.Claims;

namespace ProjectManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DailyReportsController : ControllerBase
{
    private readonly IDailyReportRepository _dailyReportRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public DailyReportsController(
        IDailyReportRepository dailyReportRepository,
        IWorkspaceRepository workspaceRepository,
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _dailyReportRepository = dailyReportRepository;
        _workspaceRepository = workspaceRepository;
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DailyReportDto>>> GetAllReports()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        IEnumerable<DailyReport> reports;

        if (currentUserRole == "Admin")
        {
            // Admins can see all reports
            reports = await _dailyReportRepository.GetAllAsync();
        }
        else if (currentUserRole == "WorkspaceAdmin")
        {
            // Workspace admins can see reports from their workspaces
            var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
            var workspaceIds = workspaces.Select(w => w.Id).ToList();
            reports = new List<DailyReport>();

            foreach (var workspaceId in workspaceIds)
            {
                var workspaceReports = await _dailyReportRepository.GetByWorkspaceIdAsync(workspaceId);
                reports = reports.Concat(workspaceReports);
            }
        }
        else
        {
            // Regular users can only see their own reports
            reports = await _dailyReportRepository.GetByUserIdAsync(currentUserId);
        }

        var reportDtos = reports.Select(r => new DailyReportDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User.FullName,
            Date = r.Date,
            WorkspaceId = r.WorkspaceId,
            WorkspaceName = r.Workspace.Name,
            ProjectId = r.ProjectId,
            ProjectName = r.Project?.Name ?? string.Empty,
            WorkDescription = r.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(r.YumusesCompleted) ? new List<Guid>() : r.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = r.Notes ?? string.Empty,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Ok(reportDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DailyReportDto>> GetReport(Guid id)
    {
        var report = await _dailyReportRepository.GetByIdAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var hasAccess = report.UserId == currentUserId || currentUserRole == "Admin" || currentUserRole == "WorkspaceAdmin";
        
        // If user is workspace admin, check if report is from their workspace
        if (currentUserRole == "WorkspaceAdmin" && !hasAccess)
        {
            var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
            hasAccess = workspaces.Any(w => w.Id == report.WorkspaceId);
        }

        if (!hasAccess)
        {
            return Forbid();
        }

        var reportDto = new DailyReportDto
        {
            Id = report.Id,
            UserId = report.UserId,
            UserName = report.User.FullName,
            Date = report.Date,
            WorkspaceId = report.WorkspaceId,
            WorkspaceName = report.Workspace.Name,
            ProjectId = report.ProjectId,
            ProjectName = report.Project?.Name ?? string.Empty,
            WorkDescription = report.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(report.YumusesCompleted) ? new List<Guid>() : report.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = report.Notes ?? string.Empty,
            CreatedAt = report.CreatedAt
        };

        return Ok(reportDto);
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<DailyReportDto>>> GetMyReports()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var reports = await _dailyReportRepository.GetByUserIdAsync(currentUserId);

        var reportDtos = reports.Select(r => new DailyReportDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User.FullName,
            Date = r.Date,
            WorkspaceId = r.WorkspaceId,
            WorkspaceName = r.Workspace.Name,
            ProjectId = r.ProjectId,
            ProjectName = r.Project?.Name ?? string.Empty,
            WorkDescription = r.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(r.YumusesCompleted) ? new List<Guid>() : r.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = r.Notes ?? string.Empty,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Ok(reportDtos);
    }

    [HttpGet("today")]
    public async Task<ActionResult<DailyReportDto>> GetTodaysReport()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var today = DateTime.UtcNow.Date;
        var report = await _dailyReportRepository.GetByUserAndDateAsync(currentUserId, today);

        if (report == null)
        {
            return NotFound(new { Message = "No report found for today" });
        }

        var reportDto = new DailyReportDto
        {
            Id = report.Id,
            UserId = report.UserId,
            UserName = report.User.FullName,
            Date = report.Date,
            WorkspaceId = report.WorkspaceId,
            WorkspaceName = report.Workspace.Name,
            ProjectId = report.ProjectId,
            ProjectName = report.Project?.Name ?? string.Empty,
            WorkDescription = report.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(report.YumusesCompleted) ? new List<Guid>() : report.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = report.Notes ?? string.Empty,
            CreatedAt = report.CreatedAt
        };

        return Ok(reportDto);
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<IEnumerable<DailyReportDto>>> GetReportsByDate(DateTime date)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        IEnumerable<DailyReport> reports;

        if (currentUserRole == "Admin")
        {
            // Admins can see all reports for the date
            reports = await _dailyReportRepository.GetAllAsync();
            reports = reports.Where(r => r.Date.Date == date.Date);
        }
        else if (currentUserRole == "WorkspaceAdmin")
        {
            // Workspace admins can see reports from their workspaces for the date
            var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
            var workspaceIds = workspaces.Select(w => w.Id).ToList();
            reports = new List<DailyReport>();

            foreach (var workspaceId in workspaceIds)
            {
                var workspaceReports = await _dailyReportRepository.GetByWorkspaceIdAsync(workspaceId);
                reports = reports.Concat(workspaceReports.Where(r => r.Date.Date == date.Date));
            }
        }
        else
        {
            // Regular users can only see their own reports for the date
            reports = await _dailyReportRepository.GetByUserIdAsync(currentUserId);
            reports = reports.Where(r => r.Date.Date == date.Date);
        }

        var reportDtos = reports.Select(r => new DailyReportDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User.FullName,
            Date = r.Date,
            WorkspaceId = r.WorkspaceId,
            WorkspaceName = r.Workspace.Name,
            ProjectId = r.ProjectId,
            ProjectName = r.Project?.Name ?? string.Empty,
            WorkDescription = r.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(r.YumusesCompleted) ? new List<Guid>() : r.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = r.Notes ?? string.Empty,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Ok(reportDtos);
    }

    [HttpPost]
    public async Task<ActionResult<DailyReportDto>> CreateReport([FromBody] CreateDailyReportDto createReportDto)
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        // Validate date (cannot be in future)
        if (createReportDto.Date.Date > DateTime.UtcNow.Date)
        {
            return BadRequest(new { Message = "Cannot create report for future dates" });
        }

        // Validate date (cannot be more than 30 days in past)
        if (createReportDto.Date.Date < DateTime.UtcNow.Date.AddDays(-30))
        {
            return BadRequest(new { Message = "Cannot create report for dates older than 30 days" });
        }

        // Check if workspace exists
        var workspaceExists = await _workspaceRepository.ExistsAsync(createReportDto.WorkspaceId);
        if (!workspaceExists)
        {
            return BadRequest(new { Message = "Workspace not found" });
        }

        // Check if project exists (if provided)
        if (createReportDto.ProjectId.HasValue)
        {
            var projectExists = await _projectRepository.ExistsAsync(createReportDto.ProjectId.Value);
            if (!projectExists)
            {
                return BadRequest(new { Message = "Project not found" });
            }

            // Verify project belongs to workspace
            var project = await _projectRepository.GetByIdAsync(createReportDto.ProjectId.Value);
            if (project?.WorkspaceId != createReportDto.WorkspaceId)
            {
                return BadRequest(new { Message = "Project does not belong to the specified workspace" });
            }
        }

        // Check if report already exists for this user and date
        var reportExists = await _dailyReportRepository.ReportExistsForDateAsync(currentUserId, createReportDto.Date);
        if (reportExists)
        {
            return BadRequest(new { Message = "Report already exists for this date" });
        }

        var report = new DailyReport
        {
            UserId = currentUserId,
            Date = createReportDto.Date,
            WorkspaceId = createReportDto.WorkspaceId,
            ProjectId = createReportDto.ProjectId,
            WorkDescription = createReportDto.WorkDescription,
            YumusesCompleted = string.Join(",", createReportDto.YumusesCompleted),
            Notes = createReportDto.Notes
        };

        var createdReport = await _dailyReportRepository.CreateAsync(report);

        var reportDto = new DailyReportDto
        {
            Id = createdReport.Id,
            UserId = createdReport.UserId,
            UserName = (await _userRepository.GetByIdAsync(currentUserId))?.FullName ?? string.Empty,
            Date = createdReport.Date,
            WorkspaceId = createdReport.WorkspaceId,
            WorkspaceName = (await _workspaceRepository.GetByIdAsync(createdReport.WorkspaceId))?.Name ?? string.Empty,
            ProjectId = createdReport.ProjectId,
            ProjectName = createdReport.ProjectId.HasValue ? (await _projectRepository.GetByIdAsync(createdReport.ProjectId.Value))?.Name ?? string.Empty : string.Empty,
            WorkDescription = createdReport.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(createdReport.YumusesCompleted) ? new List<Guid>() : createdReport.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = createdReport.Notes ?? string.Empty,
            CreatedAt = createdReport.CreatedAt
        };

        return CreatedAtAction(nameof(GetReport), new { id = createdReport.Id }, reportDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DailyReportDto>> UpdateReport(Guid id, [FromBody] UpdateDailyReportDto updateReportDto)
    {
        var report = await _dailyReportRepository.GetByIdAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var hasPermission = report.UserId == currentUserId || currentUserRole == "Admin";
        if (!hasPermission)
        {
            return Forbid();
        }

        // Check if report is older than 7 days (non-admins cannot edit old reports)
        if (currentUserRole != "Admin" && report.CreatedAt < DateTime.UtcNow.AddDays(-7))
        {
            return BadRequest(new { Message = "Cannot modify reports older than 7 days" });
        }

        // Cannot change date after creation
        // Update other fields
        report.WorkDescription = updateReportDto.WorkDescription;
        report.YumusesCompleted = string.Join(",", updateReportDto.YumusesCompleted);
        report.Notes = updateReportDto.Notes;
        report.UpdatedAt = DateTime.UtcNow;

        var updatedReport = await _dailyReportRepository.UpdateAsync(report);

        var reportDto = new DailyReportDto
        {
            Id = updatedReport.Id,
            UserId = updatedReport.UserId,
            UserName = updatedReport.User.FullName,
            Date = updatedReport.Date,
            WorkspaceId = updatedReport.WorkspaceId,
            WorkspaceName = updatedReport.Workspace.Name,
            ProjectId = updatedReport.ProjectId,
            ProjectName = updatedReport.Project?.Name ?? string.Empty,
            WorkDescription = updatedReport.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(updatedReport.YumusesCompleted) ? new List<Guid>() : updatedReport.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = updatedReport.Notes ?? string.Empty,
            CreatedAt = updatedReport.CreatedAt
        };

        return Ok(reportDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReport(Guid id)
    {
        var report = await _dailyReportRepository.GetByIdAsync(id);
        if (report == null)
        {
            return NotFound();
        }

        // Check permissions
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var hasPermission = report.UserId == currentUserId || currentUserRole == "Admin";
        if (!hasPermission)
        {
            return Forbid();
        }

        var result = await _dailyReportRepository.DeleteAsync(id);
        if (!result)
        {
            return StatusCode(500, new { Message = "Error deleting report" });
        }

        return NoContent();
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<DailyReportDto>>> GetReportsByUser(Guid userId)
    {
        // Check if user exists
        var userExists = await _userRepository.ExistsAsync(userId);
        if (!userExists)
        {
            return NotFound(new { Message = "User not found" });
        }

        var reports = await _dailyReportRepository.GetByUserIdAsync(userId);

        var reportDtos = reports.Select(r => new DailyReportDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User.FullName,
            Date = r.Date,
            WorkspaceId = r.WorkspaceId,
            WorkspaceName = r.Workspace.Name,
            ProjectId = r.ProjectId,
            ProjectName = r.Project?.Name ?? string.Empty,
            WorkDescription = r.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(r.YumusesCompleted) ? new List<Guid>() : r.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = r.Notes ?? string.Empty,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Ok(reportDtos);
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<ActionResult<IEnumerable<DailyReportDto>>> GetReportsByWorkspace(Guid workspaceId)
    {
        // Check if workspace exists
        var workspaceExists = await _workspaceRepository.ExistsAsync(workspaceId);
        if (!workspaceExists)
        {
            return NotFound(new { Message = "Workspace not found" });
        }

        // Check if user has access to this workspace
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserRole != "Admin")
        {
            var workspace = await _workspaceRepository.GetByIdAsync(workspaceId);
            var isMember = workspace?.Members.Any(m => m.UserId == currentUserId) ?? false;
            var hasAccess = isMember || workspace?.OwnerId == currentUserId || workspace?.AdminId == currentUserId;
            
            if (!hasAccess)
            {
                return Forbid();
            }
        }

        var reports = await _dailyReportRepository.GetByWorkspaceIdAsync(workspaceId);

        var reportDtos = reports.Select(r => new DailyReportDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = r.User.FullName,
            Date = r.Date,
            WorkspaceId = r.WorkspaceId,
            WorkspaceName = r.Workspace.Name,
            ProjectId = r.ProjectId,
            ProjectName = r.Project?.Name ?? string.Empty,
            WorkDescription = r.WorkDescription,
            YumusesCompleted = string.IsNullOrEmpty(r.YumusesCompleted) ? new List<Guid>() : r.YumusesCompleted.Split(',').Select(Guid.Parse).ToList(),
            Notes = r.Notes ?? string.Empty,
            CreatedAt = r.CreatedAt
        }).ToList();

        return Ok(reportDtos);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetReportStats()
    {
        var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        IEnumerable<DailyReport> reports;

        if (currentUserRole == "Admin")
        {
            // Admins can see all reports
            reports = await _dailyReportRepository.GetAllAsync();
        }
        else if (currentUserRole == "WorkspaceAdmin")
        {
            // Workspace admins can see reports from their workspaces
            var workspaces = await _workspaceRepository.GetByUserIdAsync(currentUserId);
            var workspaceIds = workspaces.Select(w => w.Id).ToList();
            reports = new List<DailyReport>();

            foreach (var workspaceId in workspaceIds)
            {
                var workspaceReports = await _dailyReportRepository.GetByWorkspaceIdAsync(workspaceId);
                reports = reports.Concat(workspaceReports);
            }
        }
        else
        {
            // Regular users can only see their own reports
            reports = await _dailyReportRepository.GetByUserIdAsync(currentUserId);
        }

        var totalReports = reports.Count();
        var reportsLast7Days = reports.Count(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-7));
        var reportsLast30Days = reports.Count(r => r.CreatedAt >= DateTime.UtcNow.AddDays(-30));

        return Ok(new
        {
            TotalReports = totalReports,
            ReportsLast7Days = reportsLast7Days,
            ReportsLast30Days = reportsLast30Days
        });
    }
}