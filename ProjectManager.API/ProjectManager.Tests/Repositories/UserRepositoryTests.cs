using Dinfo.Test.helpers;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories;

namespace ProjectManager.Tests.Repositories;

public class UserRepositoryTests : TestBase
{
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        _sut = new UserRepository(Context);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnSeededAdmin()
    {
        var admin = await _sut.GetByUsernameAsync("admin");

        Assert.NotNull(admin);
        Assert.Equal("admin", admin!.Username);
        Assert.Equal(UserRole.Admin, admin.Role);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistUser()
    {
        var unique = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Username = $"integration_{unique}",
            Email = $"{unique}@example.com",
            FullName = "Integration Test User",
            PasswordHash = "hashed-password",
            Role = UserRole.Member,
            IsActive = true
        };

        var created = await _sut.CreateAsync(user);
        var fetched = await _sut.GetByIdAsync(created.Id);

        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Username, fetched!.Username);
    }
}

