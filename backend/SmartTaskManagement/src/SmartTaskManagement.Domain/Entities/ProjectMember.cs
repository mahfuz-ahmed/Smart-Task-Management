using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Join entity representing a User's membership in a Project.
///
/// WHY this exists:
///   Without this table any authenticated user can see/interact with any project.
///   ProjectMember enforces that only explicitly added users have access.
///
/// UNIQUE CONSTRAINT: (ProjectId, UserId) — enforced at DB level via EF config.
///   Prevents duplicate membership records.
///
/// InvitedByUserId: Full audit — who added this member.
///   Required for accountability in multi-user environments.
///
/// IsActive: Soft membership removal without losing history.
///   Set IsActive=false instead of deleting — preserves "who was member when".
/// </summary>
public sealed class ProjectMember : BaseEntity<Guid>
{
    // ── Membership ────────────────────────────────────────────────────────────

    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>
    /// Project-level role — independent of the user's system-wide UserRole.
    /// A TeamMember system-role user can be a project Manager here.
    /// </summary>
    public ProjectRole ProjectRole { get; set; } = ProjectRole.Member;

    /// <summary>FK to the User who added this member to the project.</summary>
    public Guid InvitedByUserId { get; set; }

    /// <summary>UTC timestamp when the user joined/was added to the project.</summary>
    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// False = removed from project without deleting history.
    /// Inactive members cannot access the project.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ── Navigation Properties ─────────────────────────────────────────────────

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
    public User InvitedByUser { get; set; } = null!;
}
