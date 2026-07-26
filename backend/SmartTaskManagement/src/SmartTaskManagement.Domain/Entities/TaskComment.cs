namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// A comment posted by a user on a TaskItem.
///
/// THREADING: ParentCommentId enables one-level reply threads.
///   NULL  → top-level comment
///   Set   → reply to the referenced comment
///   Self-referencing FK — standard pattern for threaded comments.
///   Deep nesting (replies-of-replies) is deliberately excluded to keep
///   the data model and UI simple.
///
/// EDIT TRACKING: IsEdited + EditedAtUtc are separate from BaseEntity's
///   LastModifiedAtUtc because LastModified is a system concern whereas
///   IsEdited is a user-visible UI concern ("edited" badge on comment).
///
/// SOFT DELETE: Deleted comments show as "[comment removed]" in UI
///   rather than disappearing — preserves thread context.
/// </summary>
public sealed class TaskComment : BaseEntity<Guid>
{
    // ── Content ───────────────────────────────────────────────────────────────

    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }

    /// <summary>The comment body. Max 1000 chars enforced at DB level.</summary>
    public string Content { get; set; } = string.Empty;

    // ── Edit Tracking ─────────────────────────────────────────────────────────

    /// <summary>True if the user has edited this comment after initial posting.</summary>
    public bool IsEdited { get; set; } = false;

    /// <summary>UTC timestamp of the last user edit. Null if never edited.</summary>
    public DateTime? EditedAtUtc { get; set; }

    // ── Threading ─────────────────────────────────────────────────────────────

    /// <summary>
    /// If set, this comment is a reply to the referenced comment.
    /// Null = top-level comment.
    /// Self-referencing FK: TaskComments.ParentCommentId → TaskComments.Id
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    // ── Navigation Properties ─────────────────────────────────────────────────

    public TaskItem Task { get; set; } = null!;
    public User User { get; set; } = null!;

    /// <summary>The parent comment if this is a reply.</summary>
    public TaskComment? ParentComment { get; set; }

    /// <summary>Direct replies to this comment.</summary>
    public ICollection<TaskComment> Replies { get; set; } = new List<TaskComment>();
}
