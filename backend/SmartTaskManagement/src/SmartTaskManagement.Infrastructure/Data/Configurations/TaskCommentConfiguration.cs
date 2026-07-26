
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Data.Configurations;

public sealed class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> b)
    {
        b.ToTable("TaskComments");

        // ── Primary Key ───────────────────────────────────────────────────
        b.HasKey(c => c.Id);

        // ── Properties ────────────────────────────────────────────────────
        b.Property(c => c.Content)
            .IsRequired()
            .HasMaxLength(1000);

        b.Property(c => c.IsEdited)
            .IsRequired();

        b.Property(c => c.EditedAtUtc)
            .IsRequired(false);

        b.Property(c => c.ParentCommentId)
            .IsRequired(false);

        // ── Relationships ─────────────────────────────────────────────────
        b.HasOne(c => c.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing for reply threads
        b.HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Database Indexes (Performance Optimization) ───────────────────

        // 1. Optimized for fetching comments of a task ordered chronologically
        // Example: WHERE TaskId = '...' ORDER BY CreatedAtUtc ASC
        b.HasIndex(c => new { c.TaskId, c.CreatedAtUtc });

        // 2. Fast retrieval of direct replies to a parent comment
        // Example: WHERE ParentCommentId = '...'
        b.HasIndex(c => c.ParentCommentId);

        // 3. Fast lookup for comments written by a specific user
        b.HasIndex(c => c.UserId);

        // 4. Index on IsDeleted for efficient contextual rendering/filtering
        b.HasIndex(c => c.IsDeleted);
    }
}