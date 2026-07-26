namespace SmartTaskManagement.Application.DTOs.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    int Status,              // ProjectStatus enum value
    int Priority,            // Priority enum value
    DateTime? StartDate,
    DateTime? EndDate,
    Guid CreatedByUserId,
    string CreatedByUserName,
    int TotalTasks,
    int CompletedTasks,
    int MemberCount,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc
);
