using AutoMapper;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;

namespace ProjectManager.API.Profiles;

public class DailyReportProfile : Profile
{
    public DailyReportProfile()
    {
        CreateMap<DailyReport, DailyReportDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.WorkspaceName, opt => opt.MapFrom(src => src.Workspace.Name))
            .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty));
        
        CreateMap<CreateDailyReportDto, DailyReport>();
        
        CreateMap<UpdateDailyReportDto, DailyReport>();
    }
}