using SmartTaskManagement.Application.DTOs.Tasks;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using TaskStatus = SmartTaskManagement.Domain.Enums.TaskStatus;

namespace SmartTaskManagement.Application.Mappings;

public static class TaskMappings
{
    public static TaskDto ToDto(this TaskItem t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.Status,
        t.Status.ToString(),
        t.Priority,
        t.Priority.ToString(),
        t.DueDate,
        t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow
            && t.Status != TaskStatus.Completed
            && t.Status != TaskStatus.Cancelled,
        t.ProjectId,
        t.Project?.Name ?? string.Empty,
        t.AssignedToUserId,
        t.AssignedToUser?.FullName,
        t.Comments.Count(c => !c.IsDeleted),
        t.CreatedAtUtc,
        t.LastModifiedAtUtc,
        t.RowVersion
    );

    public static TaskItem ToEntity(this CreateTaskDto dto, Guid projectId) => new()
    {
        Title       = dto.Title.Trim(),
        Description = dto.Description.Trim(),
        Priority    = (TaskPriority)dto.Priority,
        DueDate     = dto.DueDate,
        ProjectId   = projectId,
        Status      = TaskStatus.ToDo
    };

    public static TaskActivityLogDto ToDto(this TaskActivityLog log) => new(
        log.Id,
        log.TaskId,
        log.PerformedByUserId,
        log.PerformedByUser?.FullName ?? string.Empty,
        log.Action,
        log.PropertyName,
        log.OldValue,
        log.NewValue,
        log.CreatedAtUtc
    );
}
