namespace SmartTaskManagement.Domain.Enums;

/// <summary>
/// Defines the access roles within the system.
/// Values start at 1 (not 0) so that default(UserRole) is never a valid role —
/// any uninitialized field immediately signals a bug.
/// </summary>
public enum UserRole
{
    Admin          = 1,
    ProjectManager = 2,
    TeamMember     = 3
}
