namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Generic base entity providing:
///   - Strongly-typed primary key (avoids Guid vs int mismatch at compile time)
///   - Full UTC-based audit trail (Created, LastModified, Deleted)
///   - Soft delete support with complete audit (who + when)
///
/// WHY UTC: All timestamps stored as UTC to prevent timezone-related bugs
/// when the application or database runs in different regions.
///
/// WHY string? for By fields: The actor may be a system process ("SYSTEM"),
/// a user email, or a Guid string — keeping it as string? gives flexibility
/// without coupling to identity infrastructure.
/// </summary>
public abstract class BaseEntity<TId>
{
    // ── Primary Key ───────────────────────────────────────────────────────────

    public TId Id { get; set; } = default!;

    // ── Create Audit ─────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when the entity was first persisted.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Identity of the actor who created this entity (userId, email, or "SYSTEM").</summary>
    public string? CreatedBy { get; set; }

    // ── Modify Audit ─────────────────────────────────────────────────────────

    /// <summary>UTC timestamp of the last update. Null if never modified after creation.</summary>
    public DateTime? LastModifiedAtUtc { get; set; }

    /// <summary>Identity of the actor who last modified this entity.</summary>
    public string? LastModifiedBy { get; set; }

    // ── Soft Delete ──────────────────────────────────────────────────────────

    /// <summary>
    /// True when the entity is logically deleted.
    /// A global EF query filter excludes deleted entities from normal queries.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>UTC timestamp when the entity was soft-deleted.</summary>
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>Identity of the actor who performed the deletion.</summary>
    public string? DeletedBy { get; set; }
}
