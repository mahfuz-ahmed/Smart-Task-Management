using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// A Project groups related TaskItems and has explicit membership via ProjectMembers.
/// Only users who are members (or Admins) can access a project's tasks.
/// </summary>
public sealed class Project : BaseEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>FK to the User who created and owns this project.</summary>
    public Guid CreatedByUserId { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────

    public User CreatedByUser { get; set; } = null!;
    public ICollection<TaskItem> Tasks { get; set; } = new HashSet<TaskItem>();

    /// <summary>Explicit membership list — controls who can access this project.</summary>
    public ICollection<ProjectMember> Members { get; set; } = new HashSet<ProjectMember>();
}
