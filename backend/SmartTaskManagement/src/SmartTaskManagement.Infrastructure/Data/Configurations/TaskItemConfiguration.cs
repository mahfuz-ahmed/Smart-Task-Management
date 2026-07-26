using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Data.Configurations;

public sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> b)
    {
        b.ToTable("Tasks");

        // ── Primary Key ───────────────────────────────────────────────────
        b.HasKey(t => t.Id);

        // ── Properties ────────────────────────────────────────────────────
        b.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        b.Property(t => t.Description)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false); // Task description can be optional & long

        b.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        b.Property(t => t.Priority)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        b.Property(t => t.DueDate)
            .IsRequired(false);

        // ── Optimistic Concurrency ────────────────────────────────────────
        b.Property(t => t.RowVersion)
            .IsRowVersion();

        // ── Query Filter ──────────────────────────────────────────────────
        b.HasQueryFilter(t => !t.IsDeleted);

        // ── Relationships ─────────────────────────────────────────────────
        b.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(t => t.AssignedToUser)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Database Indexes (Performance Optimization) ───────────────────

        // 1. Composite Index: Optimized for Kanban / Scrum Board Status Filtering
        // Example: WHERE ProjectId = '...' AND Status = 'InProgress'
        b.HasIndex(t => new { t.ProjectId, t.Status });

        // 2. Fast lookup for My Assigned Tasks / User Workload Dashboard
        // Example: WHERE AssignedToUserId = '...' AND IsDeleted = 0
        b.HasIndex(t => t.AssignedToUserId);

        // 3. Fast filtering for upcoming or overdue tasks
        // Example: WHERE DueDate <= @today
        b.HasIndex(t => t.DueDate);
    }
}
