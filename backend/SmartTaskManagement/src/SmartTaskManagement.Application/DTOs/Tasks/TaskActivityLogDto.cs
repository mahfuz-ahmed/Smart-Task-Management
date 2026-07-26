namespace SmartTaskManagement.Application.DTOs.Tasks;

public sealed record TaskActivityLogDto(
    Guid Id,
    Guid TaskId,
    Guid PerformedByUserId,
    string PerformedByUserName,
    string Action,
    string? PropertyName,
    string? OldValue,
    string? NewValue,
    DateTime CreatedAtUtc
);
