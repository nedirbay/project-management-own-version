using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories.Interfaces;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthController(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        // Check if username or email already exists
        if (await _userRepository.UsernameExistsAsync(registerDto.Username))
        {
            return BadRequest(new { Message = "Username already exists" });
        }

        if (await _userRepository.EmailExistsAsync(registerDto.Email))
        {
            return BadRequest(new { Message = "Email already exists" });
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Create user
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            PasswordHash = passwordHash,
            Role = UserRole.Member,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user);

        // Generate JWT token
        var token = GenerateJwtToken(createdUser);

        var userDto = new UserDto
        {
            Id = createdUser.Id,
            Username = createdUser.Username,
            Email = createdUser.Email,
            FullName = createdUser.FullName,
            Role = createdUser.Role.ToString(),
            Avatar = createdUser.Avatar ?? string.Empty,
            Phone = createdUser.Phone ?? string.Empty,
            Bio = createdUser.Bio ?? string.Empty,
            CreatedAt = createdUser.CreatedAt
        };

        return Ok(new LoginResponseDto
        {
            Token = token,
            User = userDto
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        // Find user by username
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return Unauthorized(new { Message = "Invalid credentials" });
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Unauthorized(new { Message = "User account is deactivated" });
        }

        // Generate JWT token
        var token = GenerateJwtToken(user);

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Avatar = user.Avatar ?? string.Empty,
            Phone = user.Phone ?? string.Empty,
            Bio = user.Bio ?? string.Empty,
            CreatedAt = user.CreatedAt
        };

        return Ok(new LoginResponseDto
        {
            Token = token,
            User = userDto
        });
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured");
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "1440");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}