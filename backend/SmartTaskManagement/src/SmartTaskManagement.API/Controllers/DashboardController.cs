using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Common.Constants;
using SmartTaskManagement.Application.DTOs.Dashboard;
using SmartTaskManagement.Application.Interfaces.ExternalServices;

namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
[Produces("application/json")]
public sealed class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetStatsAsync(GetCurrentUserId(), cancellationToken);

        return Ok(ApiResponse<DashboardStatsDto>.Ok(result, SuccessMessages.Retrieved));
    }
}