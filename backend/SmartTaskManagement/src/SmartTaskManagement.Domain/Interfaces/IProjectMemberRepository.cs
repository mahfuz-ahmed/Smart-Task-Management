using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Domain.Interfaces;

public interface IProjectMemberRepository : IRepository<ProjectMember, Guid>
{
    Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Guid>> GetUserIdsByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
