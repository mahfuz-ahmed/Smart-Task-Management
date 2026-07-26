
using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;

namespace SmartTaskManagement.Infrastructure.Repositories;

public sealed class ProjectRepository : BaseRepository<Project, Guid>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Single Project Load with all navigations (AsNoTracking since it's typically for display/DTO mapping)
    /// </summary>
    public async Task<Project?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects
            .AsNoTracking()
            .Include(p => p.CreatedByUser)
            .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                .ThenInclude(t => t.AssignedToUser)
            .Include(p => p.Members.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <summary>
    /// Load project WITH TRACKING for delete operations (includes Tasks with Comments and ActivityLogs)
    /// </summary>
    public async Task<Project?> GetByIdForDeleteAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                .ThenInclude(t => t.Comments.Where(c => !c.IsDeleted))
            .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                .ThenInclude(t => t.ActivityLogs.Where(l => !l.IsDeleted))
            .Include(p => p.Members.Where(m => m.IsActive))
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <summary>
    /// Lightweight Paged Query optimized for performance & high volume traffic
    /// </summary>
    public async Task<(IEnumerable<Project> Items, int TotalCount)> GetPagedAsync(
        string? search,
        ProjectStatus? status,
        Priority? priority,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        Guid? createdByUserId = null,
        CancellationToken ct = default)
    {
        // 1. AsNoTracking for read-only optimization
        var query = _context.Projects.AsNoTracking();

        // 2. Filter by User Access / Owner
        if (createdByUserId.HasValue)
        {
            var userId = createdByUserId.Value;
            query = query.Where(p => p.CreatedByUserId == userId
                                 || p.Members.Any(m => m.UserId == userId && m.IsActive));
        }

        // 3. Apply Filters
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(p => p.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = $"%{search.Trim()}%";
            query = query.Where(p => EF.Functions.Like(p.Name, searchTerm)
                                 || (p.Description != null && EF.Functions.Like(p.Description, searchTerm)));
        }

        // 4. Count total BEFORE applying heavy Includes
        var total = await query.CountAsync(ct);

        if (total == 0)
            return (Enumerable.Empty<Project>(), 0);

        // 5. Apply Sorting
        query = (sortBy?.ToLowerInvariant(), sortDescending) switch
        {
            ("name", false) => query.OrderBy(p => p.Name),
            ("name", true) => query.OrderByDescending(p => p.Name),
            ("createdat", false) => query.OrderBy(p => p.CreatedAtUtc),
            ("createdat", true) => query.OrderByDescending(p => p.CreatedAtUtc),
            _ => query.OrderByDescending(p => p.CreatedAtUtc)
        };

        // 6. Include essential navigation properties + lightweight task/member counts
        //    Use Select projection to avoid loading full Tasks/Members collections
        var items = await query
            .Include(p => p.CreatedByUser)
            .Include(p => p.Tasks.Where(t => !t.IsDeleted))  // Include Tasks for counting
            .Include(p => p.Members.Where(m => m.IsActive))  // Include Members for counting
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}