using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;

namespace SmartTaskManagement.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : BaseRepository<RefreshToken, Guid>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Gets a refresh token only if it is active, not revoked, not used, AND not expired.
    /// </summary>
    public async Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r =>
                r.Token == token &&
                !r.IsUsed &&
                !r.IsRevoked &&
                r.ExpiresAtUtc > DateTime.UtcNow, ct);
    }

    /// <summary>
    /// Instantly revokes all active refresh tokens for a specific user via bulk SQL execution.
    /// </summary>
    public async Task RevokeAllForUserAsync(Guid userId, string revokedBy, CancellationToken ct = default)
    {
        // EF Core 7+ Bulk Update: Single SQL statement without loading entities into memory
        await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.IsRevoked, true)
                .SetProperty(r => r.LastModifiedAtUtc, DateTime.UtcNow)
                .SetProperty(r => r.LastModifiedBy, revokedBy), ct);
    }
}
