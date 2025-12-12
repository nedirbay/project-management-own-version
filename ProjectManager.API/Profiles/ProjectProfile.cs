using AutoMapper;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;

namespace ProjectManager.API.Profiles;

public class ProjectProfile : Profile
{
    public ProjectProfile()
    {
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.WorkspaceName, opt => opt.MapFrom(src => src.Workspace.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));
        
        CreateMap<CreateProjectDto, Project>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ProjectStatus>(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<Priority>(src.Priority)));
        
        CreateMap<UpdateProjectDto, Project>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => Enum.Parse<ProjectStatus>(src.Status)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => Enum.Parse<Priority>(src.Priority)));
    }
}