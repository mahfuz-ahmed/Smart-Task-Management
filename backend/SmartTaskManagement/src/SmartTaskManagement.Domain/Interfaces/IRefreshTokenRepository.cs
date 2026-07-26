using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Domain.Interfaces;

/// <summary>
/// Refresh token data access — focused solely on token lookup and management.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    /// <summary>Finds an active (non-expired, non-revoked, non-used) token by its value.</summary>
    Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Revokes all active tokens for a user — used on logout or security events.</summary>
    Task RevokeAllForUserAsync(Guid userId, string revokedBy, CancellationToken cancellationToken = default);
}
