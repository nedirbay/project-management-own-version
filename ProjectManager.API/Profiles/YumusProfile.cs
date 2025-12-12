using AutoMapper;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;

namespace ProjectManager.API.Profiles;

public class YumusProfile : Profile
{
    public YumusProfile()
    {
        CreateMap<Yumus, YumusDto>()
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
        
        CreateMap<CreateYumusDto, Yumus>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TaskYagdaylar>(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<Priority>(src.Priority)));
        
        CreateMap<UpdateYumusDto, Yumus>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<TaskYagdaylar>(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<Priority>(src.Priority)));
        
        CreateMap<SubTask, SubTaskDto>();
        CreateMap<CreateSubTaskDto, SubTask>();
        
        CreateMap<TaskComment, TaskCommentDto>();
        CreateMap<CreateTaskCommentDto, TaskComment>();
        
        CreateMap<TaskAttachment, TaskAttachmentDto>();
    }
}