using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Domain.Interfaces;

/// <summary>
/// Project-specific data access contract: paged listing and detail fetch.
/// </summary>
public interface IProjectRepository : IRepository<Project, Guid>
{
    Task<(IEnumerable<Project> Items, int TotalCount)> GetPagedAsync(
        string? search,
        ProjectStatus? status,
        Priority? priority,
        string? sortBy,
        bool sortDescending,
        int page,
        int pageSize,
        Guid? createdByUserId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Loads project with its Tasks (non-deleted) and CreatedByUser (AsNoTracking for read-only).</summary>
    Task<Project?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Loads project with all related entities for delete operation (WITH TRACKING for updates).</summary>
    Task<Project?> GetByIdForDeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
