using SmartTaskManagement.Application.DTOs.Projects;

namespace SmartTaskManagement.Application.Interfaces.Services
{
    public interface IProjectMemberService
    {
        Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(Guid projectId, CancellationToken ct = default);

        Task<ProjectMemberDto> AddMemberAsync(Guid projectId, AddProjectMemberDto dto, Guid invitedByUserId, IEnumerable<string> roles, CancellationToken ct = default);

        Task RemoveMemberAsync(Guid projectId, Guid userId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default);
    }
}