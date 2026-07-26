using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;

namespace SmartTaskManagement.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly AppDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    // Lazy initialization using DI service provider
    public IProjectRepository Projects => _serviceProvider.GetRequiredService<IProjectRepository>();
    public ITaskRepository Tasks => _serviceProvider.GetRequiredService<ITaskRepository>();
    public IUserRepository Users => _serviceProvider.GetRequiredService<IUserRepository>();
    public IRefreshTokenRepository RefreshTokens => _serviceProvider.GetRequiredService<IRefreshTokenRepository>();
    public IProjectMemberRepository ProjectMembers => _serviceProvider.GetRequiredService<IProjectMemberRepository>();
    public ITaskCommentRepository TaskComments => _serviceProvider.GetRequiredService<ITaskCommentRepository>();

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null) return;
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            await _context.SaveChangesAsync(ct);
            if (_transaction != null)
            {
                await _transaction.CommitAsync(ct);
            }
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(ct);
            }
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        // AppDbContext will be automatically disposed by ASP.NET Core DI container.
    }
}
