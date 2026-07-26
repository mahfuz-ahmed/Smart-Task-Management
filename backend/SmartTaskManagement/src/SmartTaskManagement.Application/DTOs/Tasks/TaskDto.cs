using SmartTaskManagement.Domain.Enums;
using TaskStatus = SmartTaskManagement.Domain.Enums.TaskStatus;

namespace SmartTaskManagement.Application.DTOs.Tasks;

public sealed record TaskDto(
    Guid Id,
    string Title,
    string Description,
    TaskStatus Status,
    string StatusName,
    TaskPriority Priority,
    string PriorityName,
    DateTime? DueDate,
    bool IsOverdue,
    Guid ProjectId,
    string ProjectName,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    int CommentCount,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc,
    byte[] RowVersion
);
