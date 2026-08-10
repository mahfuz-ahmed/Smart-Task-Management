using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.DTOs.Projects;
using SmartTaskManagement.Application.Exceptions;
using SmartTaskManagement.Application.Interfaces.Services;
using SmartTaskManagement.Application.Mappings;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Services;

namespace SmartTaskManagement.Application.Services
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<ProjectService> _logger;

        public ProjectMemberService(IUnitOfWork uow, ILogger<ProjectService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<ProjectMemberDto> AddMemberAsync(Guid projectId, AddProjectMemberDto dto, Guid invitedByUserId, IEnumerable<string> roles, CancellationToken ct = default)
        {
            var project = await _uow.Projects.GetByIdWithDetailsAsync(projectId, ct)
                ?? throw new NotFoundException(nameof(Project), projectId);

            // Check if user can manage members
            AuthorizationHelper.EnsureCanManageMembers(roles, project, invitedByUserId);

            var targetUserExists = await _uow.Users.ExistsAsync(u => u.Id == dto.UserId, ct);
            if (!targetUserExists)
                throw new NotFoundException(nameof(User), dto.UserId);

            var existingMember = await _uow.ProjectMembers.GetMembershipAsync(projectId, dto.UserId, ct);

            if (existingMember != null)
            {
                if (existingMember.IsActive)
                    throw new BusinessException("User is already an active member of this project.");

                existingMember.IsActive = true;
                existingMember.ProjectRole = (ProjectRole)dto.ProjectRole;
                existingMember.InvitedByUserId = invitedByUserId;
                existingMember.JoinedAtUtc = DateTime.UtcNow;

                _uow.ProjectMembers.Update(existingMember);
                await _uow.SaveChangesAsync(ct);

                _logger.LogInformation("Member reactivated: {UserId} to project {ProjectId}", dto.UserId, projectId);
                return existingMember.ToDto();
            }

            var member = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                UserId = dto.UserId,
                ProjectRole = (ProjectRole)dto.ProjectRole,
                InvitedByUserId = invitedByUserId,
                JoinedAtUtc = DateTime.UtcNow,
                IsActive = true
            };

            await _uow.ProjectMembers.AddAsync(member, ct);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Member added: {UserId} to project {ProjectId}", dto.UserId, projectId);

            var saved = await _uow.ProjectMembers.GetMembershipAsync(projectId, dto.UserId, ct);
            return saved!.ToDto();
        }

        public async Task RemoveMemberAsync(Guid projectId, Guid userId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
        {
            var project = await _uow.Projects.GetByIdWithDetailsAsync(projectId, ct)
                ?? throw new NotFoundException(nameof(Project), projectId);

            // Check if user can manage members
            AuthorizationHelper.EnsureCanManageMembers(roles, project, requestingUserId);

            var member = await _uow.ProjectMembers.GetMembershipAsync(projectId, userId, ct)
                ?? throw new NotFoundException("ProjectMember", userId);

            member.IsActive = false;
            _uow.ProjectMembers.Update(member);
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("Member removed: {UserId} from project {ProjectId}", userId, projectId);
        }

        public async Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(Guid projectId, CancellationToken ct = default)
        {
            if (!await _uow.Projects.ExistsAsync(p => p.Id == projectId, ct))
                throw new NotFoundException(nameof(Project), projectId);

            var members = await _uow.ProjectMembers.GetProjectMembersAsync(projectId, ct);
            return members.Select(m => m.ToDto());
        }
    }
}