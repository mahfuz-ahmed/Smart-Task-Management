namespace SmartTaskManagement.Domain.Enums;

/// <summary>
/// Business priority levels for a TaskItem.
/// Values start at 1 so that default(TaskPriority) = 0 is always invalid and detectable.
/// </summary>
public enum TaskPriority
{
    Low      = 1,
    Medium   = 2,
    High     = 3,
    Critical = 4
}
