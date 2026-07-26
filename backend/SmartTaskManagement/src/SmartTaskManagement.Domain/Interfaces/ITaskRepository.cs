using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using TaskStatus = SmartTaskManagement.Domain.Enums.TaskStatus;

namespace SmartTaskManagement.Domain.Interfaces;

/// <summary>
/// Task-specific data access: paged/filtered listing, dashboard aggregates.
/// </summary>
public interface ITaskRepository : IRepository<TaskItem, Guid>
{
    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedByProjectAsync(
        Guid projectId,
        string? search,
        TaskStatus? status,
        TaskPriority? priority,
        Guid? assignedToUserId,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Loads task with Project, AssignedToUser, and ActivityLogs.</summary>
    Task<TaskItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetByIdWithDetailsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> UpdateStatusAsync(Guid taskId, TaskStatus newStatus, CancellationToken cancellationToken = default);

    /// <summary>Tasks due within the next <paramref name="daysAhead"/> days, excluding terminal states.</summary>
    Task<IEnumerable<TaskItem>> GetUpcomingDueTasksAsync(
        int daysAhead = 7,
        CancellationToken cancellationToken = default);

    /// <summary>Count per status — used by Dashboard aggregate query.</summary>
    Task<IDictionary<TaskStatus, int>> GetCountByStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Count per priority — used by Dashboard aggregate query.</summary>
    Task<IDictionary<TaskPriority, int>> GetCountByPriorityAsync(CancellationToken cancellationToken = default);

    Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedTasksAsync(
        string? search,
        TaskStatus? status,
        TaskPriority? priority,
        Guid? assignedToUserId,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetByProjectAndAssigneeAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>Invalidates dashboard cache for status and priority counts.</summary>
    Task InvalidateCacheAsync(CancellationToken cancellationToken = default);
}
