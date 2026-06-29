using Microsoft.EntityFrameworkCore;
using ArtisanalVCS.Server.Models;

namespace ArtisanalVCS.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Commit> Commits => Set<Commit>();
    public DbSet<Snapshot> Snapshots => Set<Snapshot>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Username).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.HasOne(p => p.Owner)
                .WithMany(u => u.Projects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Branch>(e =>
        {
            e.HasOne(b => b.Project)
                .WithMany(p => p.Branches)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(b => new { b.ProjectId, b.Name }).IsUnique();
        });

        modelBuilder.Entity<Commit>(e =>
        {
            e.HasOne(c => c.Author)
                .WithMany(u => u.Commits)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(c => c.Branch)
                .WithMany(b => b.Commits)
                .HasForeignKey(c => c.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.Project)
                .WithMany(p => p.Commits)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Snapshot>(e =>
        {
            e.HasOne(s => s.Commit)
                .WithMany(c => c.Snapshots)
                .HasForeignKey(s => s.CommitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectMember>(e =>
        {
            e.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(pm => new { pm.ProjectId, pm.UserId }).IsUnique();
        });
    }
}
