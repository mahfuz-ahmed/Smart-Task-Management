using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.DTOs.Projects;

public sealed class ProjectQueryDto : QueryParameters
{
    public Guid? CreatedByUserId { get; set; }
    public ProjectStatus? Status { get; set; }
    public Priority? Priority { get; set; }
}
