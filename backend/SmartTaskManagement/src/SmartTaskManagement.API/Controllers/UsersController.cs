using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Common.Constants;
using SmartTaskManagement.Application.DTOs.Users;
using SmartTaskManagement.Application.Interfaces.Services;

namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] Guid? excludeProjectId = null, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var users = await _userService.SearchAsync(term, excludeProjectId, limit, cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(users));
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken = default)
    {
        await _userService.DeleteAsync(userId, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse.Ok(SuccessMessages.Deleted));
    }
}