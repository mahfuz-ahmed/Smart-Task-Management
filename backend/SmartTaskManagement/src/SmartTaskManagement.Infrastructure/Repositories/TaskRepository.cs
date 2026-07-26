using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;
using TaskStatus = SmartTaskManagement.Domain.Enums.TaskStatus;

namespace SmartTaskManagement.Infrastructure.Repositories;

public sealed class TaskRepository : BaseRepository<TaskItem, Guid>, ITaskRepository
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private const string StatusCountsCacheKey = "task_status_counts";
    private const string PriorityCountsCacheKey = "task_priority_counts";

    public TaskRepository(AppDbContext context, IMemoryCache cache) : base(context)
    {
        _cache = cache;
    }

    public async Task<TaskItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await BuildDetailsQuery(tracking: true)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
    }

    public async Task<TaskItem?> GetByIdWithDetailsNoTrackingAsync(Guid id, CancellationToken ct = default)
    {
        return await BuildDetailsQuery(tracking: false)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
    }

    public async Task<int> UpdateStatusAsync(Guid taskId, TaskStatus newStatus, CancellationToken ct = default)
    {
        _cache.Remove(StatusCountsCacheKey);
        _cache.Remove(PriorityCountsCacheKey);

        return await _context.Tasks
            .Where(t => t.Id == taskId && !t.IsDeleted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.Status, newStatus)
                .SetProperty(t => t.LastModifiedAtUtc, DateTime.UtcNow), ct);
    }

    public Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedByProjectAsync(
        Guid projectId, string? search, TaskStatus? status, TaskPriority? priority,
        Guid? assignedToUserId, string? sortBy, bool sortDescending,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId && !t.IsDeleted);

        return GetPagedAsync(query, search, status, priority, assignedToUserId,
            sortBy, sortDescending, page, pageSize, ct);
    }

    public Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedTasksAsync(
        string? search, TaskStatus? status, TaskPriority? priority,
        Guid? assignedToUserId, string? sortBy, bool sortDescending,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted);

        return GetPagedAsync(query, search, status, priority, assignedToUserId,
            sortBy, sortDescending, page, pageSize, ct);
    }

    public async Task<IEnumerable<TaskItem>> GetUpcomingDueTasksAsync(int daysAhead = 7, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(daysAhead);

        return await _context.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.AssignedToUser)
            .Where(t => !t.IsDeleted &&
                       t.DueDate.HasValue &&
                       t.DueDate.Value <= cutoff &&
                       t.DueDate.Value >= now &&
                       t.Status != TaskStatus.Completed &&
                       t.Status != TaskStatus.Cancelled)
            .OrderBy(t => t.DueDate)
            .Take(10)
            .ToListAsync(ct);
    }

    public async Task<IDictionary<TaskStatus, int>> GetCountByStatusAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(StatusCountsCacheKey, out IDictionary<TaskStatus, int>? cached))
            return cached!;

        var result = await _context.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var dict = result.ToDictionary(x => x.Status, x => x.Count);
        _cache.Set(StatusCountsCacheKey, dict, CacheDuration);

        return dict;
    }

    public async Task<IDictionary<TaskPriority, int>> GetCountByPriorityAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(PriorityCountsCacheKey, out IDictionary<TaskPriority, int>? cached))
            return cached!;

        var result = await _context.Tasks
            .AsNoTracking()
            .Where(t => !t.IsDeleted)
            .GroupBy(t => t.Priority)
            .Select(g => new { Priority = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var dict = result.ToDictionary(x => x.Priority, x => x.Count);
        _cache.Set(PriorityCountsCacheKey, dict, CacheDuration);

        return dict;
    }

    public async Task<IReadOnlyList<TaskItem>> GetByProjectAndAssigneeAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId &&
                       t.AssignedToUserId == userId &&
                       !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);
    }

    // ── Private Reusable Helpers ──────────────────────────────────────────────

    private IQueryable<TaskItem> BuildDetailsQuery(bool tracking)
    {
        var query = tracking ? _context.Tasks.AsQueryable() : _context.Tasks.AsNoTracking();

        return query
            .AsSplitQuery()
            .Include(t => t.Project)
            .Include(t => t.AssignedToUser)
            .Include(t => t.ActivityLogs.OrderByDescending(l => l.CreatedAtUtc))
                .ThenInclude(l => l.PerformedByUser)
            .Include(t => t.Comments.Where(c => !c.IsDeleted && c.ParentCommentId == null))
                .ThenInclude(c => c.Replies.Where(r => !r.IsDeleted));
    }

    private static IQueryable<TaskItem> ApplyFiltersAndSorting(
        IQueryable<TaskItem> query,
        string? search,
        TaskStatus? status,
        TaskPriority? priority,
        Guid? assignedToUserId,
        string? sortBy,
        bool sortDescending)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(t =>
                EF.Functions.Like(t.Title, $"%{trimmedSearch}%") ||
                (t.Description != null && EF.Functions.Like(t.Description, $"%{trimmedSearch}%")));
        }

        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        if (priority.HasValue) query = query.Where(t => t.Priority == priority.Value);
        if (assignedToUserId.HasValue) query = query.Where(t => t.AssignedToUserId == assignedToUserId.Value);

        return (sortBy?.ToLowerInvariant(), sortDescending) switch
        {
            ("title", false) => query.OrderBy(t => t.Title),
            ("title", true) => query.OrderByDescending(t => t.Title),
            ("priority", false) => query.OrderBy(t => t.Priority),
            ("priority", true) => query.OrderByDescending(t => t.Priority),
            ("status", false) => query.OrderBy(t => t.Status),
            ("status", true) => query.OrderByDescending(t => t.Status),
            ("duedate", false) => query.OrderBy(t => t.DueDate),
            ("duedate", true) => query.OrderByDescending(t => t.DueDate),
            ("createdat", false) => query.OrderBy(t => t.CreatedAtUtc),
            ("createdat", true) => query.OrderByDescending(t => t.CreatedAtUtc),
            _ => query.OrderByDescending(t => t.CreatedAtUtc)
        };
    }

    private async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedAsync(
        IQueryable<TaskItem> query,
        string? search,
        TaskStatus? status,
        TaskPriority? priority,
        Guid? assignedToUserId,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        // Safe Boundary checks for Pagination
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        query = ApplyFiltersAndSorting(query, search, status, priority, assignedToUserId, sortBy, sortDescending);

        // Count runs fast without JOINs
        var total = await query.CountAsync(ct);
        if (total == 0) return (Enumerable.Empty<TaskItem>(), 0);

        // Include relations only for the required page data
        var items = await query
            .Include(t => t.Project)
            .Include(t => t.AssignedToUser)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
    
    public Task InvalidateCacheAsync(CancellationToken ct = default)
    {
        _cache.Remove(StatusCountsCacheKey);
        _cache.Remove(PriorityCountsCacheKey);
        return Task.CompletedTask;
    }
}