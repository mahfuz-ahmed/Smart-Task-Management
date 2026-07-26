using SmartTaskManagement.Application.DTOs.Projects;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.Mappings;

public static class ProjectMappings
{
    public static ProjectDto ToDto(this Project p) => new(
        p.Id,
        p.Name,
        p.Description,
        (int)p.Status,          // Add Status
        (int)p.Priority,        // Add Priority
        p.StartDate,            // Add StartDate
        p.EndDate,              // Add EndDate
        p.CreatedByUserId,
        p.CreatedByUser?.FullName ?? string.Empty,
        p.Tasks.Count(t => !t.IsDeleted),
        p.Tasks.Count(t => !t.IsDeleted && t.Status == Domain.Enums.TaskStatus.Completed),
        p.Members.Count(m => m.IsActive),
        p.CreatedAtUtc,
        p.LastModifiedAtUtc
    );

    public static Project ToEntity(this CreateProjectDto dto, Guid createdByUserId) => new()
    {
        Name             = dto.Name.Trim(),
        Description      = dto.Description.Trim(),
        Status           = (ProjectStatus)dto.Status,     // Add Status
        Priority         = (Priority)dto.Priority,         // Add Priority
        StartDate        = dto.StartDate,                  // Add StartDate
        EndDate          = dto.EndDate,                    // Add EndDate
        CreatedByUserId  = createdByUserId
    };

    public static ProjectMemberDto ToDto(this ProjectMember m) => new(
        m.Id,
        m.ProjectId,
        m.UserId,
        m.User?.FullName ?? string.Empty,
        m.User?.Email ?? string.Empty,
        m.ProjectRole.ToString(),
        m.InvitedByUserId,
        m.InvitedByUser?.FullName ?? string.Empty,
        m.JoinedAtUtc,
        m.IsActive
    );
}
