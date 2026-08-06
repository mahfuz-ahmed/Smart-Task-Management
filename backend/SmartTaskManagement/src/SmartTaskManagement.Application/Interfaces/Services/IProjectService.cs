using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Projects;

namespace SmartTaskManagement.Application.Interfaces.Services;

public interface IProjectService
{
    Task<PagedResult<ProjectDto>> GetProjectsAsync(ProjectQueryDto query, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<ProjectDto> GetByIdAsync(Guid id, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<ProjectDto> CreateAsync(CreateProjectDto dto, Guid createdByUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task DeleteAsync(Guid id, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);

    // Members
    Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(Guid projectId, CancellationToken ct = default);

    Task<ProjectMemberDto> AddMemberAsync(Guid projectId, AddProjectMemberDto dto, Guid invitedByUserId, IEnumerable<string> roles, CancellationToken ct = default);

    Task RemoveMemberAsync(Guid projectId, Guid userId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);
}