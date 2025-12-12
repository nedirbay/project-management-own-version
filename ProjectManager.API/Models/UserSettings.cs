using System.ComponentModel.DataAnnotations;

namespace ProjectManager.API.Models;

public class UserSettings
{
    public Guid Id { get; set; }
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public string Theme { get; set; } = "system"; // "light", "dark", "system"
    
    [Required]
    public string Language { get; set; } = "en"; // "tr", "en"
    
    [Required]
    public string Timezone { get; set; } = "UTC"; // "Europe/Istanbul"
    
    [Required]
    public string DateFormat { get; set; } = "DD/MM/YYYY"; // "DD/MM/YYYY"
    
    [Required]
    public string TimeFormat { get; set; } = "24h"; // "12h", "24h"

    // Notification Settings
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool TaskAssignedNotification { get; set; } = true;
    public bool TaskCompletedNotification { get; set; } = true;
    public bool ProjectUpdatedNotification { get; set; } = true;
    public bool ReportReminderNotification { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
}