using SmartTaskManagement.Domain.Enums;
using TaskStatus = SmartTaskManagement.Domain.Enums.TaskStatus;

namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Core work unit. Belongs to a Project, optionally assigned to a User.
/// RowVersion provides optimistic concurrency — prevents lost updates.
/// </summary>
public sealed class TaskItem : BaseEntity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }

    public Guid ProjectId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// SQL Server rowversion — EF Core uses this as optimistic concurrency token.
    /// Throws DbUpdateConcurrencyException if two users save simultaneously.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    // ── Navigation Properties ─────────────────────────────────────────────────

    public Project Project { get; set; } = null!;
    public User? AssignedToUser { get; set; }
    public ICollection<TaskActivityLog> ActivityLogs { get; set; } = new List<TaskActivityLog>();
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}