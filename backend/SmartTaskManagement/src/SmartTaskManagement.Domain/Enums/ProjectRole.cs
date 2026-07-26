namespace SmartTaskManagement.Domain.Enums;

/// <summary>
/// Project-level role — separate from system-wide UserRole.
///
/// WHY two role layers:
///   UserRole    = global system role (Admin, ProjectManager, TeamMember)
///   ProjectRole = local project role (a TeamMember can be project Manager)
///
/// This mirrors how Jira/Linear handle project-level permissions.
/// </summary>
public enum ProjectRole
{
    Manager = 1,
    Member  = 2
}
