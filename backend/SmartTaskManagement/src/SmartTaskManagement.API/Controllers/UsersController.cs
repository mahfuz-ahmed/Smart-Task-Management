using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Users;
using SmartTaskManagement.Application.Interfaces.Services;
using System.Security.Claims;

namespace SmartTaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> Search(
        [FromQuery] string term,
        [FromQuery] Guid? excludeProjectId = null,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var users = await _userService.SearchAsync(term, excludeProjectId, limit, ct);
        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(users));
    }

    [HttpDelete("{userId:guid}")]
    public async Task<ActionResult<ApiResponse>> Delete(
        Guid userId,
        CancellationToken ct = default)
    {
        await _userService.DeleteAsync(userId, GetUserId(), GetRoles(), ct);
        return Ok(ApiResponse.Ok("User deleted successfully."));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");
        return Guid.Parse(userIdClaim);
    }

    private IEnumerable<string> GetRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(x => x.Value);
    }
}