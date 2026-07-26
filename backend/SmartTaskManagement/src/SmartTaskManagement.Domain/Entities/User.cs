using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Pure domain User — no IdentityUser dependency.
/// Infrastructure layer handles password hashing and JWT generation.
/// </summary>
public sealed class User : BaseEntity<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.TeamMember;
    public bool IsActive { get; set; } = true;

    /// <summary>Computed domain property — ignored by EF Core mapping.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    // ── Navigation Properties ─────────────────────────────────────────────────

    public ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new HashSet<ProjectMember>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new HashSet<TaskItem>();
    public ICollection<TaskComment> Comments { get; set; } = new HashSet<TaskComment>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new HashSet<RefreshToken>();
}