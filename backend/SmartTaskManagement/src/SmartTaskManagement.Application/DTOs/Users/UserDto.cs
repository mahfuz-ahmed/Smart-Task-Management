namespace SmartTaskManagement.Application.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FullName,
    SmartTaskManagement.Domain.Enums.UserRole Role
);
