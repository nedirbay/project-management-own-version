using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManager.API.DTOs;
using ProjectManager.API.Models;
using ProjectManager.API.Repositories.Interfaces;
using AutoMapper;
using BCrypt.Net;
using System.Security.Claims;

namespace ProjectManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _userRepository.GetAllAsync();
        var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        // Users can get their own info or admins can get any user info
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserId != id.ToString() && currentUserRole != "Admin")
        {
            return Forbid();
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var userDto = _mapper.Map<UserDto>(user);
        return Ok(userDto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        // Check if username or email already exists
        if (await _userRepository.UsernameExistsAsync(createUserDto.Username))
        {
            return BadRequest(new { Message = "Username already exists" });
        }

        if (await _userRepository.EmailExistsAsync(createUserDto.Email))
        {
            return BadRequest(new { Message = "Email already exists" });
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        // Map DTO to entity
        var user = _mapper.Map<User>(createUserDto);
        user.PasswordHash = passwordHash;

        var createdUser = await _userRepository.CreateAsync(user);
        var userDto = _mapper.Map<UserDto>(createdUser);

        return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, userDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
    {
        // Users can update their own info or admins can update any user info
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserId != id.ToString() && currentUserRole != "Admin")
        {
            return Forbid();
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Update user properties
        _mapper.Map(updateUserDto, user);
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        var userDto = _mapper.Map<UserDto>(updatedUser);

        return Ok(userDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var userExists = await _userRepository.ExistsAsync(id);
        if (!userExists)
        {
            return NotFound();
        }

        var result = await _userRepository.DeleteAsync(id);
        if (!result)
        {
            return StatusCode(500, new { Message = "Error deleting user" });
        }

        return NoContent();
    }

    [HttpPatch("{id}/role")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> ChangeUserRole(Guid id, [FromBody] dynamic roleData)
    {
        var role = roleData.role?.ToString();
        if (string.IsNullOrEmpty(role))
        {
            return BadRequest(new { Message = "Role is required" });
        }

        // Validate role
        if (role != "Admin" && role != "WorkspaceAdmin" && role != "Member")
        {
            return BadRequest(new { Message = "Invalid role" });
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.Role = Enum.Parse<UserRole>(role);
        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);
        var userDto = _mapper.Map<UserDto>(updatedUser);

        return Ok(userDto);
    }

    [HttpPost("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto changePasswordDto)
    {
        // Users can change their own password or admins can change any user's password
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (currentUserId != id.ToString() && currentUserRole != "Admin")
        {
            return Forbid();
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { Message = "Current password is incorrect" });
        }

        // Hash new password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        return Ok(new { Message = "Password changed successfully" });
    }

    
}