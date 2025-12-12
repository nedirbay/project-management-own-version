namespace ProjectManager.API.DTOs;

// User Settings Response
public class UserSettingsDto
{
    public string Theme { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string DateFormat { get; set; } = string.Empty;
    public string TimeFormat { get; set; } = string.Empty;
    public NotificationSettingsDto Notifications { get; set; } = new();
}

// Notification Settings
public class NotificationSettingsDto
{
    public bool Email { get; set; }
    public bool Push { get; set; }
    public bool TaskAssigned { get; set; }
    public bool TaskCompleted { get; set; }
    public bool ProjectUpdated { get; set; }
    public bool ReportReminder { get; set; }
}

// Update Settings Request
public class UpdateUserSettingsDto
{
    public string Theme { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string DateFormat { get; set; } = string.Empty;
    public string TimeFormat { get; set; } = string.Empty;
    public NotificationSettingsDto Notifications { get; set; } = new();
}