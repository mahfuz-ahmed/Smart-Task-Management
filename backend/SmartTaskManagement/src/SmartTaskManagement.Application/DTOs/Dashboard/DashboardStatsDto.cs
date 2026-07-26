using SmartTaskManagement.Application.DTOs.Tasks;

namespace SmartTaskManagement.Application.DTOs.Dashboard;

public sealed record ProjectProgressItemDto(
    Guid ProjectId,
    string ProjectName,
    int CompletionPercentage,
    int TotalTasks,
    int CompletedTasks
);

public sealed record DashboardActivityItemDto(
    Guid Id,
    string Action,
    string Description,
    DateTime CreatedAt,
    string PerformedByName,
    string ProjectName,
    string TaskTitle
);

public sealed record DashboardStatsDto(
    int TotalProjects,
    int TotalTasks,
    int MyTasks,
    int CompletedTasks,
    int PendingTasks,
    int OverdueTasks,
    int UpcomingTasks,
    IDictionary<string, int> TasksByStatus,
    IDictionary<string, int> TasksByPriority,
    IEnumerable<DashboardActivityItemDto> RecentActivity,
    IEnumerable<ProjectProgressItemDto> ProjectProgress,
    IEnumerable<TaskDto> UpcomingDueTasks
);
