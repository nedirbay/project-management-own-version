using AutoMapper;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;

namespace ProjectManager.API.Profiles;

public class WorkspaceProfile : Profile
{
    public WorkspaceProfile()
    {
        CreateMap<Workspace, WorkspaceDto>()
            .ForMember(dest => dest.AdminName, opt => opt.MapFrom(src => src.Admin.FullName));
        
        CreateMap<CreateWorkspaceDto, Workspace>();
        
        CreateMap<UpdateWorkspaceDto, Workspace>();
    }
}