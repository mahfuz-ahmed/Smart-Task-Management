using System.Linq.Expressions;

namespace SmartTaskManagement.Domain.Interfaces;

/// <summary>
/// Generic repository abstraction over BaseEntity&lt;TId&gt;.
/// Defined in Domain so Application services can depend on it without
/// referencing EF Core or any infrastructure concern.
/// </summary>
public interface IRepository<TEntity, TId>
    where TEntity : Entities.BaseEntity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    void Update(TEntity entity);

    /// <summary>
    /// Soft delete — sets IsDeleted = true, DeletedAtUtc, DeletedBy.
    /// Never issues a physical DELETE.
    /// </summary>
    void SoftDelete(TEntity entity, string? deletedBy = null);

    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
