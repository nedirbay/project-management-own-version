using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.Models;

namespace Dinfo.Test.helpers;

public abstract class ControllerTestBase : TestBase
{
    protected IMapper Mapper { get; }

    protected ControllerTestBase()
    {
        Mapper = MapperHelper.CreateMapper();
    }

    protected void SetUser(ControllerBase controller, User user)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = BuildPrincipal(user)
            }
        };
    }

    private static ClaimsPrincipal BuildPrincipal(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }
}

