namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Persistent refresh token entity for secure JWT token rotation.
///
/// Security model:
///   - Access token (JWT): short-lived (15 min), stateless
///   - Refresh token: long-lived (7 days), stored in DB, single-use
///
/// WHY JwtId:
///   Links this refresh token to a specific JWT via the JWT's 'jti' claim.
///   When the client presents a refresh token, we verify that the JWT's jti
///   matches — preventing an attacker from using a refresh token with a
///   forged or unrelated access token.
///
/// WHY IsUsed + IsRevoked as separate flags:
///   IsUsed  → token was exchanged for a new one (normal rotation)
///   IsRevoked → token was explicitly invalidated (logout, suspicious activity)
///   Both make the token inactive but for different audit reasons.
/// </summary>
public sealed class RefreshToken : BaseEntity<Guid>
{
    // ── Ownership ─────────────────────────────────────────────────────────────

    /// <summary>FK to the User who owns this token.</summary>
    public Guid UserId { get; set; }

    // ── Token Data ────────────────────────────────────────────────────────────

    /// <summary>Cryptographically random token string (Base64, 64 bytes).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The 'jti' (JWT ID) claim of the access token this refresh token was issued alongside.
    /// Used to validate that the access token and refresh token are a matching pair.
    /// </summary>
    public string JwtId { get; set; } = string.Empty;

    /// <summary>UTC expiry time. After this point, the token is always inactive.</summary>
    public DateTime ExpiresAtUtc { get; set; }

    // ── State Flags ───────────────────────────────────────────────────────────

    /// <summary>True after the token has been successfully exchanged for a new pair.</summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>True if the token was explicitly invalidated (logout, security event).</summary>
    public bool IsRevoked { get; set; } = false;

    // ── Computed State ────────────────────────────────────────────────────────

    /// <summary>Token can only be used if not expired, not used, and not revoked.</summary>
    public bool IsActive => !IsUsed && !IsRevoked && DateTime.UtcNow < ExpiresAtUtc;

    // ── Navigation Properties ─────────────────────────────────────────────────

    public User User { get; set; } = null!;
}
