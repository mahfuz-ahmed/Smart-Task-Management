namespace SmartTaskManagement.Domain.Interfaces;

/// <summary>
/// Coordinates all repositories in a single DB transaction.
/// One SaveChangesAsync = one atomic commit across all repos.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IProjectMemberRepository ProjectMembers { get; }
    ITaskCommentRepository TaskComments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
