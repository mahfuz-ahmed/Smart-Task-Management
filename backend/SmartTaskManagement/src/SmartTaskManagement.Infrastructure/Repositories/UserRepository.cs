using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;

namespace SmartTaskManagement.Infrastructure.Repositories;

public sealed class UserRepository : BaseRepository<User, Guid>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var normalizedEmail = email.Trim();

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Email == normalizedEmail, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var normalizedEmail = email.Trim();

        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => !u.IsDeleted && u.Email == normalizedEmail, ct);
    }

    public async Task<IReadOnlyList<User>> SearchUsersAsync(
        string keyword,
        Guid? excludeProjectId = null,
        int limit = 10,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(keyword) || keyword.Trim().Length < 2)
            return Array.Empty<User>();

        var term = keyword.Trim();

        var query = _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        // Filter out existing project members if project ID is provided
        if (excludeProjectId.HasValue)
        {
            var memberUserIds = _context.ProjectMembers
                .AsNoTracking()
                .Where(pm => pm.ProjectId == excludeProjectId.Value && !pm.IsDeleted)
                .Select(pm => pm.UserId);

            query = query.Where(u => !memberUserIds.Contains(u.Id));
        }

        // High performance SQL LIKE query using EF.Functions
        query = query.Where(u =>
            EF.Functions.Like(u.Email, $"%{term}%") ||
            EF.Functions.Like(u.FirstName, $"%{term}%") ||
            EF.Functions.Like(u.LastName, $"%{term}%"));

        return await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Take(Math.Clamp(limit, 1, 50))
            .ToListAsync(ct);
    }
}