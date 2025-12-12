using System;
using AutoMapper;

namespace Dinfo.Test.helpers;

public static class MapperHelper
{
    private static readonly Lazy<IMapper> LazyMapper = new(() =>
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(ProjectManager.API.Profiles.WorkspaceProfile).Assembly);
        });
        return config.CreateMapper();
    });

    public static IMapper CreateMapper() => LazyMapper.Value;
}

