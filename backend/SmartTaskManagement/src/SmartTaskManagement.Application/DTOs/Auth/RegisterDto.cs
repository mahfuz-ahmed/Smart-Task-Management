using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.DTOs.Auth;

/// <summary>
/// Registration payload from frontend.
/// Role is required - users must choose ProjectManager or TeamMember.
/// Admin role can only be assigned through database seeding, not self-registration.
/// </summary>
public sealed record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    UserRole Role
);
