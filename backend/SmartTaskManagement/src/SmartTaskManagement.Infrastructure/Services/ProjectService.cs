using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Projects;
using SmartTaskManagement.Application.Exceptions;
using SmartTaskManagement.Application.Interfaces;
using SmartTaskManagement.Application.Mappings;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Domain.Interfaces;

namespace SmartTaskManagement.Infrastructure.Services;

public sealed class ProjectService : IProjectService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(IUnitOfWork uow, ILogger<ProjectService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsAsync(
        ProjectQueryDto query,
        Guid requestingUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        Guid? filterUserId = IsAdmin(roles) ? null : requestingUserId;

        var (items, total) = await _uow.Projects.GetPagedAsync(
            query.Search, query.Status, query.Priority, query.SortBy, query.SortDescending,
            query.Page, query.PageSize, filterUserId, ct);

        return PagedResult<ProjectDto>.Create(items.Select(p => p.ToDto()), total, query.Page, query.PageSize);
    }

    public async Task<ProjectDto> GetByIdAsync(
        Guid id,
        Guid requestingUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        var project = await _uow.Projects.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException(nameof(Project), id);

        if (!IsAdmin(roles) &&
            project.CreatedByUserId != requestingUserId &&
            !await _uow.ProjectMembers.IsMemberAsync(id, requestingUserId, ct))
        {
            throw new ForbiddenException("You do not have access to this project.");
        }

        return project.ToDto();
    }

    public async Task<ProjectDto> CreateAsync(
        CreateProjectDto dto,
        Guid createdByUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        // Only Admins can create projects
        AuthorizationHelper.EnsureCanCreateProject(roles);

        var project = dto.ToEntity(createdByUserId);
        project.Id = Guid.NewGuid();

        await _uow.Projects.AddAsync(project, ct);

        // Creator automatically becomes a Manager member
        var membership = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = createdByUserId,
            ProjectRole = ProjectRole.Manager,
            InvitedByUserId = createdByUserId,
            JoinedAtUtc = DateTime.UtcNow,
            IsActive = true
        };

        await _uow.ProjectMembers.AddAsync(membership, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Project created: {ProjectId} by user {UserId}", project.Id, createdByUserId);

        var created = await _uow.Projects.GetByIdWithDetailsAsync(project.Id, ct);
        return created!.ToDto();
    }

    public async Task<ProjectDto> UpdateAsync(
        Guid id,
        UpdateProjectDto dto,
        Guid requestingUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        // First, load with details for authorization check
        var projectForAuth = await _uow.Projects.GetByIdWithDetailsAsync(id, ct)
            ?? throw new NotFoundException(nameof(Project), id);

        // Check if user can update this project
        AuthorizationHelper.EnsureCanUpdateProject(roles, projectForAuth, requestingUserId);

        // Now load the project WITHOUT navigation properties for clean update
        var project = await _uow.Projects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Project), id);

        // Update all properties
        project.Name = dto.Name.Trim();
        project.Description = dto.Description.Trim();
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;

        if (Enum.IsDefined(typeof(ProjectStatus), dto.Status))
            project.Status = (ProjectStatus)dto.Status;

        if (Enum.IsDefined(typeof(Priority), dto.Priority))
            project.Priority = (Priority)dto.Priority;

        // Now Update() is safe because this entity has no tracked navigation properties
        _uow.Projects.Update(project);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Project updated: {ProjectId} by user {UserId}", id, requestingUserId);

        // Return fresh data with all details
        var updated = await _uow.Projects.GetByIdWithDetailsAsync(id, ct);
        return updated!.ToDto();
    }

    public async Task DeleteAsync(
        Guid id,
        Guid requestingUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        var project = await _uow.Projects.GetByIdForDeleteAsync(id, ct)
            ?? throw new NotFoundException(nameof(Project), id);

        // Check if user can delete this project
        AuthorizationHelper.EnsureCanDeleteProject(roles, project.CreatedByUserId, requestingUserId);

        // Soft delete the project itself
        _uow.Projects.SoftDelete(project, requestingUserId.ToString());

        // Cascade soft delete all related entities
        foreach (var task in project.Tasks.Where(t => !t.IsDeleted))
        {
            _uow.Tasks.SoftDelete(task, requestingUserId.ToString());
            
            // Soft delete task comments
            foreach (var comment in task.Comments.Where(c => !c.IsDeleted))
            {
                comment.IsDeleted = true;
                comment.DeletedAtUtc = DateTime.UtcNow;
                comment.DeletedBy = requestingUserId.ToString();
            }
            
            // Soft delete task activity logs
            foreach (var log in task.ActivityLogs.Where(l => !l.IsDeleted))
            {
                log.IsDeleted = true;
                log.DeletedAtUtc = DateTime.UtcNow;
                log.DeletedBy = requestingUserId.ToString();
            }
        }
        
        // Deactivate project members
        foreach (var member in project.Members.Where(m => m.IsActive))
        {
            member.IsActive = false;
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Project soft-deleted with cascade: {ProjectId}", id);
    }

    public async Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(Guid projectId, CancellationToken ct = default)
    {
        if (!await _uow.Projects.ExistsAsync(p => p.Id == projectId, ct))
            throw new NotFoundException(nameof(Project), projectId);

        var members = await _uow.ProjectMembers.GetProjectMembersAsync(projectId, ct);
        return members.Select(m => m.ToDto());
    }

    public async Task<ProjectMemberDto> AddMemberAsync(
        Guid projectId,
        AddProjectMemberDto dto,
        Guid invitedByUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
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

    public async Task RemoveMemberAsync(
        Guid projectId,
        Guid userId,
        Guid requestingUserId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
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
    

    // ── Private Helper Methods ────────────────────────────────────────────────

    private static bool IsAdmin(IEnumerable<string> roles)
    {
        return AuthorizationHelper.IsAdmin(roles);
    }
}
