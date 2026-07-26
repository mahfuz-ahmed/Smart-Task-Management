using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;

namespace SmartTaskManagement.Infrastructure.Repositories;

public sealed class TaskCommentRepository : BaseRepository<TaskComment, Guid>, ITaskCommentRepository
{
    public TaskCommentRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Fetches root task comments along with active non-deleted replies and user profiles.
    /// Uses AsSplitQuery to avoid Cartesian Explosion.
    /// </summary>
    public async Task<IEnumerable<TaskComment>> GetByTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        return await _context.TaskComments
            .AsNoTracking()
            .AsSplitQuery() // Split multiple collection includes into optimized separate SQL queries
            .Include(c => c.User)
            .Include(c => c.Replies.Where(r => !r.IsDeleted))
                .ThenInclude(r => r.User)
            .Where(c => c.TaskId == taskId && c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(ct);
    }
}