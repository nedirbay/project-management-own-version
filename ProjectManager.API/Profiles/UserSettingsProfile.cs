using AutoMapper;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;

namespace ProjectManager.API.Profiles;

public class UserSettingsProfile : Profile
{
    public UserSettingsProfile()
    {
        CreateMap<UserSettings, UserSettingsDto>()
            .ForMember(dest => dest.Notifications, opt => opt.MapFrom(src => new NotificationSettingsDto
            {
                Email = src.EmailNotifications,
                Push = src.PushNotifications,
                TaskAssigned = src.TaskAssignedNotification,
                TaskCompleted = src.TaskCompletedNotification,
                ProjectUpdated = src.ProjectUpdatedNotification,
                ReportReminder = src.ReportReminderNotification
            }));
        
        CreateMap<UpdateUserSettingsDto, UserSettings>()
            .ForMember(dest => dest.EmailNotifications, opt => opt.MapFrom(src => src.Notifications.Email))
            .ForMember(dest => dest.PushNotifications, opt => opt.MapFrom(src => src.Notifications.Push))
            .ForMember(dest => dest.TaskAssignedNotification, opt => opt.MapFrom(src => src.Notifications.TaskAssigned))
            .ForMember(dest => dest.TaskCompletedNotification, opt => opt.MapFrom(src => src.Notifications.TaskCompleted))
            .ForMember(dest => dest.ProjectUpdatedNotification, opt => opt.MapFrom(src => src.Notifications.ProjectUpdated))
            .ForMember(dest => dest.ReportReminderNotification, opt => opt.MapFrom(src => src.Notifications.ReportReminder));
    }
}