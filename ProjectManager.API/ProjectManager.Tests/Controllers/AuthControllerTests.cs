using Dinfo.Test.helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ProjectManager.API.Controllers;
using ProjectManager.API.DTOs;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Controllers;

public class AuthControllerTests : TestBase
{
    private readonly UserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _userRepository = new UserRepository(Context);
        _configuration = ConfigurationHelper.InitConfiguration();
        _sut = new AuthController(_userRepository, _configuration);
    }

    [Fact]
    public async Task Register_ShouldCreateUserAndReturnToken()
    {
        var unique = Guid.NewGuid().ToString("N");
        var registerDto = new RegisterDto
        {
            Username = $"testuser_{unique}",
            Email = $"{unique}@example.com",
            Password = "Test123!",
            FullName = "Test User"
        };

        var result = await _sut.Register(registerDto);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponseDto>(okResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
        Assert.Equal(registerDto.Username, response.User.Username);
    }

    [Fact]
    public async Task Login_ShouldReturnTokenForValidCredentials()
    {
        // Arrange
        var unique = Guid.NewGuid().ToString("N");
        var loginDto = new LoginDto
        {
            Username = $"testuser_{unique}",
            Password = "Test123!",
        };
        
        // Act
        var res  = await _sut.Login(loginDto);

        // Assert
        Assert.NotNull(res);
    }
}