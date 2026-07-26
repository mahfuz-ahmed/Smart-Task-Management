using SmartTaskManagement.Application.DTOs.Dashboard;

namespace SmartTaskManagement.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(Guid currentUserId, CancellationToken ct = default);
}
