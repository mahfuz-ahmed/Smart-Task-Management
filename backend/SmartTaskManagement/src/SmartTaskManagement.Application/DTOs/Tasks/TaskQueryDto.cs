using SmartTaskManagement.Application.Common;

namespace SmartTaskManagement.Application.DTOs.Tasks;

public sealed class TaskQueryDto : QueryParameters
{
    public int? Status { get; set; }
    public int? Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
}
