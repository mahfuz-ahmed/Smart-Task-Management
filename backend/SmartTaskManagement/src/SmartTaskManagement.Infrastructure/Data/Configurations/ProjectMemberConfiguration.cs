using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Data.Configurations;

public sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> b)
    {
        b.ToTable("ProjectMembers");

        // ── Primary Key ───────────────────────────────────────────────────
        b.HasKey(m => m.Id);

        // ── Properties ────────────────────────────────────────────────────
        b.Property(m => m.ProjectRole)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        b.Property(x => x.InvitedByUserId).IsRequired();

        b.Property(m => m.JoinedAtUtc).IsRequired();

        b.Property(m => m.IsActive).IsRequired();

        // ── Query Filter ──────────────────────────────────────────────────
        // Active members check automatically in normal queries
        b.HasQueryFilter(m => m.IsActive);

        // ── Relationships ─────────────────────────────────────────────────
        b.HasOne(m => m.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(m => m.User)
            .WithMany(u => u.ProjectMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.InvitedByUser)
            .WithMany()
            .HasForeignKey(x => x.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Indexes ───────────────────────────────────────────────────────
        // 1. Prevent duplicate active memberships for the same project
        b.HasIndex(m => new { m.ProjectId, m.UserId })
            .IsUnique();

        // 2. Fast lookup for user's assigned projects
        b.HasIndex(m => m.UserId);

        // 3. Fast filtering for active status
        b.HasIndex(m => m.IsActive);
    }
}