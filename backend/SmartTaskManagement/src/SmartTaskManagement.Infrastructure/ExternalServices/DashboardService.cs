using Microsoft.EntityFrameworkCore;
using SmartTaskManagement.Application.DTOs.Dashboard;
using SmartTaskManagement.Application.Interfaces.ExternalServices;
using SmartTaskManagement.Application.Mappings;
using SmartTaskManagement.Domain.Interfaces;
using SmartTaskManagement.Infrastructure.Data;
using TaskStatus = SmartTaskManagement.Domain.Enums.TaskStatus;

namespace SmartTaskManagement.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ITaskRepository _tasks;

    public DashboardService(AppDbContext context, ITaskRepository tasks)
    {
        _context = context;
        _tasks = tasks;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(Guid currentUserId, CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var upcomingCutoff = nowUtc.AddDays(7);

        // 1. Core Metrics (Read-Only queries with AsNoTracking)
        var totalProjects = await _context.Projects.AsNoTracking().Where(p => !p.IsDeleted).CountAsync(ct);
        var totalTasks = await _context.Tasks.AsNoTracking().Where(t => !t.IsDeleted).CountAsync(ct);
        var completedTasks = await _context.Tasks.AsNoTracking().Where(t => !t.IsDeleted).CountAsync(t => t.Status == TaskStatus.Completed, ct);

        var pendingTasks = await _context.Tasks.AsNoTracking().Where(t => !t.IsDeleted).CountAsync(
            t => t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled, ct);

        var overdueTasks = await _context.Tasks.AsNoTracking().Where(t => !t.IsDeleted).CountAsync(
            t => t.AssignedToUserId == currentUserId  // Filter by current user
              && t.DueDate.HasValue && t.DueDate < nowUtc
              && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled, ct);

        // User-scoped metrics
        var myTasks = await _context.Tasks.AsNoTracking().Where(t => !t.IsDeleted).CountAsync(
            t => t.AssignedToUserId == currentUserId
              && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled, ct);

        var upcomingTasks = await _context.Tasks.AsNoTracking().Where(t => !t.IsDeleted).CountAsync(
            t => t.DueDate.HasValue && t.DueDate.Value <= upcomingCutoff && t.DueDate.Value >= nowUtc
              && t.Status != TaskStatus.Completed && t.Status != TaskStatus.Cancelled, ct);

        // 2. Repository Aggregations
        var byStatus = await _tasks.GetCountByStatusAsync(ct);
        var byPriority = await _tasks.GetCountByPriorityAsync(ct);
        var upcoming = await _tasks.GetUpcomingDueTasksAsync(7, ct);

        // 3. Recent Activity Feed (Projected directly to DTO at DB level)
        var recentActivity = await _context.TaskActivityLogs
            .AsNoTracking()
            .OrderByDescending(l => l.CreatedAtUtc)
            .Take(10)
            .Select(l => new DashboardActivityItemDto(
                l.Id,
                l.Action == "TaskCreated" ? "created task" :
                l.Action == "StatusChanged" ? $"changed status of task to {l.NewValue}" :
                l.Action == "PriorityChanged" ? $"changed priority of task to {l.NewValue}" :
                l.Action == "AssigneeChanged" ? "assigned task" :
                l.Action == "TitleChanged" ? "changed title of task" :
                l.Action.ToLower(),
                l.PropertyName ?? string.Empty,
                l.CreatedAtUtc,
                l.PerformedByUser != null ? l.PerformedByUser.FullName : string.Empty,
                l.Task != null && l.Task.Project != null ? l.Task.Project.Name : string.Empty,
                l.Task != null ? l.Task.Title : string.Empty
            ))
            .ToListAsync(ct);

        // 4. Project Progress (Calculated directly in SQL Server via Projection)
        var projectProgress = await _context.Projects
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                p.Name,
                TotalTasks = p.Tasks.Count(t => !t.IsDeleted),
                CompletedTasks = p.Tasks.Count(t => !t.IsDeleted && t.Status == TaskStatus.Completed)
            })
            .Select(p => new ProjectProgressItemDto(
                p.Id,
                p.Name,
                p.TotalTasks > 0 ? (int)Math.Round((double)p.CompletedTasks / p.TotalTasks * 100) : 0,
                p.TotalTasks,
                p.CompletedTasks
            ))
            .ToListAsync(ct);

        return new DashboardStatsDto(
            totalProjects,
            totalTasks,
            myTasks,
            completedTasks,
            pendingTasks,
            overdueTasks,
            upcomingTasks,
            byStatus.ToDictionary(k => k.Key.ToString(), v => v.Value),
            byPriority.ToDictionary(k => k.Key.ToString(), v => v.Value),
            recentActivity,
            projectProgress,
            upcoming.Select(t => t.ToDto())
        );
    }
}