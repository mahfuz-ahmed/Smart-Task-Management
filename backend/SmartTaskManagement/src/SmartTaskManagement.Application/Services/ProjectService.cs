using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Projects;
using SmartTaskManagement.Application.Exceptions;
using SmartTaskManagement.Application.Interfaces.Services;
using SmartTaskManagement.Application.Mappings;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Infrastructure.Services;
using SmartTaskManagement.Domain.Interfaces;

namespace SmartTaskManagement.Application.Services;

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

        //var projectForAuth = await _uow.Projects.GetByIdWithDetailsAsync(id, ct)
        //    ?? throw new NotFoundException(nameof(Project), id);

        // Check if user can update this project
        AuthorizationHelper.EnsureCanUpdateProject(roles);

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
        AuthorizationHelper.EnsureCanDeleteProject(roles);

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

    // ── Private Helper Methods ────────────────────────────────────────────────

    private static bool IsAdmin(IEnumerable<string> roles)
    {
        return AuthorizationHelper.IsAdmin(roles);
    }
}