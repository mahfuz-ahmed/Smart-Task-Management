namespace SmartTaskManagement.Domain.Entities;

/// <summary>
/// Immutable audit record capturing every significant change to a TaskItem.
///
/// WHY this exists:
///   "Who changed what, when, and from what value to what value" is a standard
///   enterprise requirement. Without it, you cannot answer support tickets,
///   compliance audits, or debugging questions about task history.
///
/// IMMUTABILITY INTENT:
///   Once written, an activity log entry must never be modified.
///   - No UpdateAsync is ever called on this entity
///   - IsDeleted from BaseEntity is present but should never be set true
///   - Service layer always uses AddAsync, never UpdateAsync
///
/// EXAMPLES of Action values:
///   "StatusChanged"      → PropertyName="Status", OldValue="ToDo", NewValue="InProgress"
///   "PriorityChanged"    → PropertyName="Priority", OldValue="Low", NewValue="Critical"
///   "AssigneeChanged"    → PropertyName="AssignedToUserId", OldValue=null, NewValue="<guid>"
///   "TaskCreated"        → no OldValue
///   "DescriptionUpdated" → OldValue=original text, NewValue=updated text
/// </summary>
public sealed class TaskActivityLog : BaseEntity<Guid>
{
    // ── Event Context ─────────────────────────────────────────────────────────

    /// <summary>FK to the TaskItem this log entry belongs to.</summary>
    public Guid TaskId { get; set; }

    /// <summary>FK to the User who performed the action.</summary>
    public Guid PerformedByUserId { get; set; }

    // ── Change Data ───────────────────────────────────────────────────────────

    /// <summary>
    /// Verb describing what happened (e.g., "StatusChanged", "TaskCreated", "Assigned").
    /// Stored as a plain string so it remains readable without enum mapping.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Which field/property was changed (e.g., "Status", "Priority", "AssignedToUserId").
    /// Null for actions like "TaskCreated" where no single property applies.
    /// </summary>
    public string? PropertyName { get; set; }

    /// <summary>String representation of the value before the change. Null for creation events.</summary>
    public string? OldValue { get; set; }

    /// <summary>String representation of the value after the change. Null for deletion events.</summary>
    public string? NewValue { get; set; }

    // ── Navigation Properties ─────────────────────────────────────────────────

    /// <summary>The task this log entry tracks.</summary>
    public TaskItem Task { get; set; } = null!;

    /// <summary>The user who caused this change.</summary>
    public User PerformedByUser { get; set; } = null!;
}
