namespace SmartTaskManagement.Domain.Enums;

/// <summary>
/// Lifecycle states of a TaskItem.
/// Values start at 1 so that default(TaskStatus) = 0 is always invalid and detectable.
/// </summary>
public enum TaskStatus
{
    ToDo       = 1,
    InProgress = 2,
    Completed  = 3,
    Cancelled  = 4
}
