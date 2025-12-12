using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.DTOs;

// Yumus Response
public class YumusDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public List<string> Tags { get; set; } = new();
    public int Order { get; set; }
    public List<UserDto> Assignees { get; set; } = new();
    public int SubTaskCount { get; set; }
    public int CompletedSubTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Yumus Detail Response
public class YumusDetailDto : YumusDto
{
    public List<SubTaskDto> SubTasks { get; set; } = new();
    public List<TaskCommentDto> Comments { get; set; } = new();
    public List<TaskAttachmentDto> Attachments { get; set; } = new();
}

// Create Yumus Request
public class CreateYumusDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public Guid ProjectId { get; set; }
    
    public string Status { get; set; } = string.Empty;
    
    public string Priority { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
    
    public decimal? EstimatedHours { get; set; }
    
    public List<string> Tags { get; set; } = new();
    
    public List<Guid> AssigneeIds { get; set; } = new();
}

// Update Yumus Request
public class UpdateYumusDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public List<string> Tags { get; set; } = new();
}

// Update Yumus Status Request
public class UpdateYumusStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

// Update Yumus Order (Kanban)
public class UpdateYumusOrderDto
{
    public int Order { get; set; }
    public string Status { get; set; } = string.Empty;
}

// SubTask DTO
public class SubTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int Order { get; set; }
}

// Create SubTask Request
public class CreateSubTaskDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
}

// Task Comment DTO
public class TaskCommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

// Create Comment Request
public class CreateTaskCommentDto
{
    [Required]
    public string Text { get; set; } = string.Empty;
}

// Task Attachment DTO
public class TaskAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}