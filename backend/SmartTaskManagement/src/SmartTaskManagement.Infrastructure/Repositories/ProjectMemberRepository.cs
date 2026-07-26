
using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;

namespace SmartTaskManagement.Infrastructure.Repositories;

public sealed class ProjectMemberRepository : BaseRepository<ProjectMember, Guid>, IProjectMemberRepository
{
    public ProjectMemberRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Gets a membership record including User and Inviter navigation details.
    /// Default tracked so it can be updated if necessary.
    /// </summary>
    public async Task<ProjectMember?> GetMembershipAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        return await _context.ProjectMembers
            .Include(m => m.User)
            .Include(m => m.InvitedByUser)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);
    }

    /// <summary>
    /// Lightweight Read-Only query to fetch active project members with user profiles.
    /// </summary>
    public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.ProjectMembers
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.InvitedByUser)
            .Where(m => m.ProjectId == projectId && m.IsActive)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Quick boolean check to verify active membership.
    /// </summary>
    public async Task<bool> IsMemberAsync(Guid projectId, Guid userId, CancellationToken ct = default)
    {
        return await _context.ProjectMembers
            .AsNoTracking()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive, ct);
    }

    /// <summary>
    /// Fast projection query returning only member User IDs without loading full entities.
    /// </summary>
    public async Task<IReadOnlyList<Guid>> GetUserIdsByProjectAsync(Guid projectId, CancellationToken ct = default)
    {
        return await _context.ProjectMembers
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId && m.IsActive)
            .Select(m => m.UserId)
            .ToListAsync(ct);
    }
}