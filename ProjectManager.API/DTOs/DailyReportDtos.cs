using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.DTOs;

// Daily Report Response
public class DailyReportDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string WorkDescription { get; set; } = string.Empty;
    public List<Guid> YumusesCompleted { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// Create Daily Report Request
public class CreateDailyReportDto
{
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public Guid WorkspaceId { get; set; }
    
    public Guid? ProjectId { get; set; }
    
    [Required]
    [MinLength(10)]
    public string WorkDescription { get; set; } = string.Empty;
    
    public List<Guid> YumusesCompleted { get; set; } = new();
    
    public string Notes { get; set; } = string.Empty;
}

// Update Daily Report Request
public class UpdateDailyReportDto
{
    public string WorkDescription { get; set; } = string.Empty;
    public List<Guid> YumusesCompleted { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}