using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Data.Configurations;

public sealed class TaskActivityLogConfiguration : IEntityTypeConfiguration<TaskActivityLog>
{
    public void Configure(EntityTypeBuilder<TaskActivityLog> b)
    {
        b.ToTable("TaskActivityLogs");

        // ── Primary Key ───────────────────────────────────────────────────
        b.HasKey(l => l.Id);

        // ── Properties ────────────────────────────────────────────────────
        b.Property(l => l.Action)
            .IsRequired()
            .HasMaxLength(100);

        b.Property(l => l.PropertyName)
            .HasMaxLength(100)
            .IsRequired(false);

        // nvarchar(max) for rich text / long description changes to prevent truncation exceptions
        b.Property(l => l.OldValue)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        b.Property(l => l.NewValue)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // ── Relationships ─────────────────────────────────────────────────
        b.HasOne(l => l.Task)
            .WithMany(t => t.ActivityLogs)
            .HasForeignKey(l => l.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(l => l.PerformedByUser)
            .WithMany()
            .HasForeignKey(l => l.PerformedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Database Indexes (Performance Optimization) ───────────────────

        // 1. Composite Index: Optimized for Task History Timeline queries
        // Example: WHERE TaskId = '...' ORDER BY CreatedAtUtc DESC
        b.HasIndex(l => new { l.TaskId, l.CreatedAtUtc });

        // 2. Optimized for User Audit / Activity Tracking queries
        // Example: "Find all actions performed by User X"
        b.HasIndex(l => l.PerformedByUserId);
    }
}