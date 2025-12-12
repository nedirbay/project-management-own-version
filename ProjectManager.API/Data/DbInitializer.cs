using ProjectManager.API.Models;
using BCrypt.Net;

namespace ProjectManager.API.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if database is already seeded
        if (context.Users.Any())
        {
            return; // DB has been seeded
        }

        // Create admin user
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@projectmanager.com",
            FullName = "System Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);

        // Create workspace admin user
        var workspaceAdminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "workspaceadmin",
            Email = "workspaceadmin@projectmanager.com",
            FullName = "Workspace Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("WorkspaceAdmin123!"),
            Role = UserRole.WorkspaceAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(workspaceAdminUser);

        // Create regular user
        var regularUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "user",
            Email = "user@projectmanager.com",
            FullName = "Regular User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
            Role = UserRole.Member,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(regularUser);

        // Save users
        await context.SaveChangesAsync();

        // Create a workspace
        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Development Team",
            Description = "Main development workspace",
            Color = "#409eff",
            OwnerId = adminUser.Id,
            AdminId = workspaceAdminUser.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Workspaces.Add(workspace);

        // Add users to workspace
        var workspaceMember1 = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = adminUser.Id,
            JoinedAt = DateTime.UtcNow
        };

        context.WorkspaceMembers.Add(workspaceMember1);

        var workspaceMember2 = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = workspaceAdminUser.Id,
            JoinedAt = DateTime.UtcNow
        };

        context.WorkspaceMembers.Add(workspaceMember2);

        var workspaceMember3 = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = regularUser.Id,
            JoinedAt = DateTime.UtcNow
        };

        context.WorkspaceMembers.Add(workspaceMember3);

        // Save workspace and members
        await context.SaveChangesAsync();

        Console.WriteLine("Database seeded successfully!");
    }
}