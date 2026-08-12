using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Tasks;
using SmartTaskManagement.Application.Exceptions;
using SmartTaskManagement.Application.Interfaces.Services;
using SmartTaskManagement.Application.Mappings;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;
using TaskStatus = SmartTaskManagement.Domain.Enums.TaskStatus;

namespace SmartTaskManagement.Infrastructure.Services;

public sealed class TaskService : ITaskService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<TaskService> _logger;
    private readonly AppDbContext _context;

    public TaskService(IUnitOfWork uow, ILogger<TaskService> logger, AppDbContext context)
    {
        _uow = uow;
        _logger = logger;
        _context = context;
    }

    public async Task<PagedResult<TaskDto>> GetTasksAsync(Guid projectId, TaskQueryDto query, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        await EnsureProjectAccessAsync(projectId, requestingUserId, roles, ct);

        var (items, total) = await _uow.Tasks.GetPagedByProjectAsync(
            projectId, query.Search,
            query.Status.HasValue ? (TaskStatus?)query.Status.Value : null,
            query.Priority.HasValue ? (TaskPriority?)query.Priority.Value : null,
            query.AssignedToUserId,
            query.SortBy, query.SortDescending,
            query.Page, query.PageSize, ct);

        return PagedResult<TaskDto>.Create(items.Select(t => t.ToDto()), total, query.Page, query.PageSize);
    }

    public async Task<PagedResult<TaskDto>> GetMyTasksAsync(TaskQueryDto query, Guid userId, CancellationToken ct = default)
    {
        var (items, total) = await _uow.Tasks.GetPagedTasksAsync(
            query.Search,
            query.Status.HasValue ? (TaskStatus?)query.Status.Value : null,
            query.Priority.HasValue ? (TaskPriority?)query.Priority.Value : null,
            userId,
            query.SortBy, query.SortDescending,
            query.Page, query.PageSize, ct);

        return PagedResult<TaskDto>.Create(items.Select(t => t.ToDto()), total, query.Page, query.PageSize);
    }

    public async Task<TaskDto> GetByIdAsync(Guid projectId, Guid taskId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        await EnsureProjectAccessAsync(projectId, requestingUserId, roles, ct);
        var task = await GetTaskOrThrowAsync(projectId, taskId, ct);
        return task.ToDto();
    }

    public async Task<TaskDto> CreateAsync(Guid projectId, CreateTaskDto dto, Guid createdByUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        await EnsureProjectAccessAsync(projectId, createdByUserId, roles, ct);

        // Load project WITH TRACKING for authorization check (ensures Members are loaded)
        var project = await _uow.Projects.GetByIdAsync(projectId, ct);
        if (project == null)
            throw new NotFoundException(nameof(Project), projectId);

        // Load members explicitly for authorization check
        await _context.Entry(project)
            .Collection(p => p.Members)
            .Query()
            .Where(m => m.IsActive)
            .LoadAsync(ct);

        // Check if user can create tasks
        AuthorizationHelper.EnsureCanManageTasks(roles, project, createdByUserId);

        var task = dto.ToEntity(projectId);
        task.Id = Guid.NewGuid();
        task.AssignedToUserId = dto.AssignedToUserId;
        if (dto.Status.HasValue && Enum.IsDefined(typeof(TaskStatus), dto.Status.Value))
            task.Status = (TaskStatus)dto.Status.Value;
        await _uow.Tasks.AddAsync(task, ct);

        // Activity log
        await LogActivityAsync(task, createdByUserId, "TaskCreated",
            null, null, task.Title, ct);

        await _uow.SaveChangesAsync(ct);

        // Invalidate dashboard cache after creating task
        await _uow.Tasks.InvalidateCacheAsync(ct);

        _logger.LogInformation("Task created: {Id} by user {UserId}", task.Id, createdByUserId);
        var created = await _uow.Tasks.GetByIdWithDetailsAsync(task.Id, ct);
        return created!.ToDto();
    }

    public async Task<TaskDto> UpdateAsync(Guid projectId, Guid taskId, UpdateTaskDto dto,
        Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        // Load task WITHOUT including collections to avoid tracking issues
        var task = await _uow.Tasks.GetByIdAsync(taskId, ct);
        if (task == null || task.ProjectId != projectId)
            throw new NotFoundException(nameof(TaskItem), taskId);

        // Load project for authorization check
        var project = await _uow.Projects.GetByIdWithDetailsAsync(projectId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        // Check if user can update this task
        AuthorizationHelper.EnsureCanUpdateTask(roles, task, project, requestingUserId);

        var oldTitle = task.Title;

        if (task.Title != dto.Title.Trim())
            await LogActivityAsync(task, requestingUserId, "TitleChanged",
                "Title", task.Title, dto.Title.Trim(), ct);

        if (task.Priority != (TaskPriority)dto.Priority)
            await LogActivityAsync(task, requestingUserId, "PriorityChanged",
                "Priority", task.Priority.ToString(), ((TaskPriority)dto.Priority).ToString(), ct);

        task.Title = dto.Title.Trim();
        task.Description = dto.Description.Trim();
        task.Priority = (TaskPriority)dto.Priority;
        task.DueDate = dto.DueDate;
        task.AssignedToUserId = dto.AssignedToUserId;
        if (dto.Status.HasValue && Enum.IsDefined(typeof(TaskStatus), dto.Status.Value))
            task.Status = (TaskStatus)dto.Status.Value;

        // Set RowVersion if provided for concurrency check
        if (dto.RowVersion != null && dto.RowVersion.Length > 0)
        {
            _context.Entry(task).Property(t => t.RowVersion).OriginalValue = dto.RowVersion;
        }

        try
        {
            await _uow.SaveChangesAsync(ct);

            // Invalidate dashboard cache after update
            await _uow.Tasks.InvalidateCacheAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating task {TaskId}.", taskId);
            throw new BusinessException("The task was modified by another user. Please refresh and try again.");
        }

        // Reload with details for DTO mapping
        var updated = await _uow.Tasks.GetByIdWithDetailsAsync(taskId, ct);
        return updated!.ToDto();
    }

    public async Task<TaskDto> UpdateStatusAsync(Guid projectId, Guid taskId,
        UpdateTaskStatusDto dto, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        if (!Enum.IsDefined(typeof(TaskStatus), dto.Status))
            throw new BusinessException($"Invalid status: {dto.Status}.");

        // Load task WITHOUT including collections to avoid tracking issues
        var task = await _uow.Tasks.GetByIdAsync(taskId, ct);
        if (task == null || task.ProjectId != projectId)
            throw new NotFoundException(nameof(TaskItem), taskId);

        var roleList = roles.ToList();
        var isAdmin = roleList.Contains(UserRole.Admin.ToString());

        var projectMember = await _uow.ProjectMembers.GetMembershipAsync(projectId, requestingUserId, ct);

        if (!isAdmin)
        {
            if (projectMember == null)
                throw new NotFoundException(nameof(ProjectMember), projectId);
        }

        var isProjectManager = projectMember?.ProjectRole == ProjectRole.Manager;
        var isAssignedUser = task.AssignedToUserId == requestingUserId;

        //if (!roleList.Contains(UserRole.Admin.ToString()) && task.AssignedToUserId != requestingUserId)
        //    throw new ForbiddenException("You can only update tasks assigned to you.");

        if (!(isAdmin || isProjectManager || isAssignedUser))
        {
            throw new ForbiddenException(
                "Only the Admin, Project Manager, or assigned user can update this task.");
        }

        var oldStatus = task.Status;
        var newStatus = dto.Status;

        if (oldStatus == newStatus)
        {
            // Reload with details for DTO mapping
            var unchanged = await _uow.Tasks.GetByIdWithDetailsAsync(taskId, ct);
            return unchanged!.ToDto();
        }

        // Update status with a direct database update to avoid stale tracked state.
        var affectedRows = await _uow.Tasks.UpdateStatusAsync(taskId, newStatus, ct);
        if (affectedRows == 0)
            throw new BusinessException("The task was modified or removed before the update completed. Please refresh and try again.");

        // Reload with details for return
        var updated = await _uow.Tasks.GetByIdWithDetailsAsync(taskId, ct);
        return updated!.ToDto();
    }

    public async Task<TaskDto> AssignAsync(Guid projectId, Guid taskId,
        AssignTaskDto dto, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var task = await EnsureTaskProjectAccessAsync(projectId, taskId, requestingUserId, roles, ct);
        var oldAssignee = task.AssignedToUserId;

        if (dto.AssignedToUserId.HasValue)
        {
            var user = await _uow.Users.GetByIdAsync(dto.AssignedToUserId.Value, ct)
                ?? throw new NotFoundException(nameof(User), dto.AssignedToUserId.Value);
        }

        await LogActivityAsync(task, requestingUserId, "AssigneeChanged",
            "AssignedToUserId", oldAssignee?.ToString(), dto.AssignedToUserId?.ToString(), ct);

        task.AssignedToUserId = dto.AssignedToUserId;
        // No explicit Update() needed — entity is already tracked by EF Core
        try
        {
            await _uow.SaveChangesAsync(ct);

            // Invalidate dashboard cache after assignment change
            await _uow.Tasks.InvalidateCacheAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict assigning task {TaskId}.", taskId);
            throw new BusinessException("The task was modified by another user. Please refresh and try again.");
        }

        // Notify new assignee
        //if (dto.AssignedToUserId.HasValue)
        //    await _notifications.SendAsync(
        //        dto.AssignedToUserId.Value,
        //        NotificationType.TaskAssigned,
        //        "Task Assigned to You",
        //        $"Task '{task.Title}' has been assigned to you.",
        //        "Task", task.Id, ct);

        return task.ToDto();
    }

    public async Task DeleteAsync(Guid projectId, Guid taskId,
        Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct = default)
    {
        var task = await EnsureTaskProjectAccessAsync(projectId, taskId, requestingUserId, roles, ct);

        // Load project for authorization check
        var project = await _uow.Projects.GetByIdWithDetailsAsync(projectId, ct)
            ?? throw new NotFoundException(nameof(Project), projectId);

        // Check if user can delete this task
        AuthorizationHelper.EnsureCanManageTasks(roles, project, requestingUserId);

        _uow.Tasks.SoftDelete(task, requestingUserId.ToString());
        await _uow.SaveChangesAsync(ct);

        // Invalidate dashboard cache after deletion
        await _uow.Tasks.InvalidateCacheAsync(ct);

        _logger.LogInformation("Task deleted: {Id} by user {UserId}", taskId, requestingUserId);
    }

    public async Task<IEnumerable<TaskActivityLogDto>> GetActivityLogsAsync(Guid taskId, CancellationToken ct = default)
    {
        var task = await _uow.Tasks.GetByIdWithDetailsAsync(taskId, ct)
            ?? throw new NotFoundException(nameof(TaskItem), taskId);
        return task.ActivityLogs.Select(l => l.ToDto());
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<TaskItem> GetTaskOrThrowAsync(Guid projectId, Guid taskId, CancellationToken ct)
    {
        var task = await _uow.Tasks.GetByIdWithDetailsAsync(taskId, ct)
            ?? throw new NotFoundException(nameof(TaskItem), taskId);

        if (task.ProjectId != projectId)
            throw new NotFoundException(nameof(TaskItem), taskId);

        return task;
    }

    private async Task EnsureProjectExistsAsync(Guid projectId, CancellationToken ct)
    {
        if (!await _uow.Projects.ExistsAsync(p => p.Id == projectId, ct))
            throw new NotFoundException(nameof(Project), projectId);
    }

    private async Task EnsureProjectAccessAsync(Guid projectId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct)
    {
        var roleList = roles.ToList();
        if (roleList.Contains(UserRole.Admin.ToString()))
            return;

        var hasAccess = await _uow.Projects.ExistsAsync(p => p.Id == projectId &&
            (p.CreatedByUserId == requestingUserId || p.Members.Any(m => m.UserId == requestingUserId && m.IsActive)), ct);

        if (!hasAccess)
            throw new ForbiddenException();
    }

    private async Task<TaskItem> EnsureTaskProjectAccessAsync(Guid projectId, Guid taskId, Guid requestingUserId, IEnumerable<string> roles, CancellationToken ct)
    {
        await EnsureProjectAccessAsync(projectId, requestingUserId, roles, ct);

        var task = await _uow.Tasks.GetByIdAsync(taskId, ct)
            ?? throw new NotFoundException(nameof(TaskItem), taskId);

        if (task.ProjectId != projectId)
            throw new NotFoundException(nameof(TaskItem), taskId);

        var roleList = roles.ToList();
        if (roleList.Contains(UserRole.Admin.ToString()) || task.AssignedToUserId == requestingUserId)
            return task;

        throw new ForbiddenException("You can only access tasks assigned to you.");
    }

    private Task LogActivityAsync(TaskItem task, Guid userId, string action,
        string? property, string? oldValue, string? newValue, CancellationToken ct)
    {
        var log = new TaskActivityLog
        {
            Id = Guid.NewGuid(),
            TaskId = task.Id,
            PerformedByUserId = userId,
            Action = action,
            PropertyName = property,
            OldValue = oldValue,
            NewValue = newValue
        };
        task.ActivityLogs.Add(log);
        return Task.CompletedTask;
    }
}