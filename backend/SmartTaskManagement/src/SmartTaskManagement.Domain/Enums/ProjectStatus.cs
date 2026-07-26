
namespace SmartTaskManagement.Domain.Enums
{
    /// <summary>
    /// Represents the lifecycle stage of a Project.
    /// Default value on creation is Planning.
    /// </summary>
    public enum ProjectStatus
    {
        Planning = 0,
        Active = 1,
        OnHold = 2,
        Completed = 3,
        Cancelled = 4
    }
}
