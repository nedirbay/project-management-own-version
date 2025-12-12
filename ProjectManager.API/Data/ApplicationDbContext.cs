using Microsoft.EntityFrameworkCore;
using ProjectManager.API.Models;

namespace ProjectManager.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceMember> WorkspaceMembers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Yumus> Yumuses { get; set; }
        public DbSet<TaskAssignee> TaskAssignees { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<DailyReport> DailyReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure many-to-many relationships

            // WorkspaceMember composite key
            modelBuilder.Entity<WorkspaceMember>()
                .HasKey(wm => new { wm.WorkspaceId, wm.UserId });

            // ProjectMember composite key
            modelBuilder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.ProjectId, pm.UserId });

            // TaskAssignee composite key
            modelBuilder.Entity<TaskAssignee>()
                .HasKey(ta => new { ta.TaskId, ta.UserId });

            // Configure relationships
            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.Workspace)
                .WithMany(w => w.Members)
                .HasForeignKey(wm => wm.WorkspaceId);

            modelBuilder.Entity<WorkspaceMember>()
                .HasOne(wm => wm.User)
                .WithMany(u => u.WorkspaceMemberships)
                .HasForeignKey(wm => wm.UserId);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId);

            modelBuilder.Entity<TaskAssignee>()
                .HasOne(ta => ta.Task)
                .WithMany(y => y.Assignees)
                .HasForeignKey(ta => ta.TaskId);

            modelBuilder.Entity<TaskAssignee>()
                .HasOne(ta => ta.User)
                .WithMany(u => u.AssignedYumuses)
                .HasForeignKey(ta => ta.UserId);

            modelBuilder.Entity<SubTask>()
                .HasOne(st => st.Task)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(st => st.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAttachment>()
                .HasOne(ta => ta.Task)
                .WithMany(t => t.Attachments)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAttachment>()
                .HasOne(ta => ta.Uploader)
                .WithMany()
                .HasForeignKey(ta => ta.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.User)
                .WithMany()
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Workspace relationships
            modelBuilder.Entity<Workspace>()
                .HasOne(w => w.Owner)
                .WithMany(u => u.OwnedWorkspaces)
                .HasForeignKey(w => w.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Workspace>()
                .HasOne(w => w.Admin)
                .WithMany()
                .HasForeignKey(w => w.AdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Project relationships
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Workspace)
                .WithMany(w => w.Projects)
                .HasForeignKey(p => p.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Yumus relationships
            modelBuilder.Entity<Yumus>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Yumuses)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Yumus>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.CreatedYumuses)
                .HasForeignKey(t => t.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // Daily report relationships
            modelBuilder.Entity<DailyReport>()
                .HasOne(r => r.User)
                .WithMany(u => u.DailyReports)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyReport>()
                .HasOne(r => r.Workspace)
                .WithMany(w => w.DailyReports)
                .HasForeignKey(r => r.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DailyReport>()
                .HasOne(r => r.Project)
                .WithMany(p => p.DailyReports)
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
