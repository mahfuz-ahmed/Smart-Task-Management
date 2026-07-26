
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Projects;
using SmartTaskManagement.Application.Interfaces;

namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
[Produces("application/json")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;
    private readonly IValidator<CreateProjectDto> _createVal;
    private readonly IValidator<UpdateProjectDto> _updateVal;

    public ProjectsController(
        IProjectService projects,
        IValidator<CreateProjectDto> createVal,
        IValidator<UpdateProjectDto> updateVal)
    {
        _projects = projects;
        _createVal = createVal;
        _updateVal = updateVal;
    }

    // Get all projects, only accessible by Admin
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] ProjectQueryDto query, CancellationToken ct)
    {
        var result = await _projects.GetProjectsAsync(query,GetUserId(),GetRoles(),ct);
        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id,CancellationToken ct)
    {
        var result = await _projects.GetByIdAsync(id,GetUserId(),GetRoles(),ct);

        return Ok(ApiResponse<ProjectDto>.Ok(result));
    }

    // Create a new project
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto,CancellationToken ct)
    {
        var v = await _createVal.ValidateAsync(dto, ct);

        if (!v.IsValid)return BadRequest(ApiResponse<object>.Fail(v.Errors.Select(e => e.ErrorMessage)));

        var result = await _projects.CreateAsync(dto,GetUserId(),GetRoles(),ct);

        return CreatedAtAction(nameof(GetById),new { id = result.Id },
            ApiResponse<ProjectDto>.Ok(result,"Project created."));
    }

    // Update a project
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id,[FromBody] UpdateProjectDto dto,CancellationToken ct)
    {
        var v = await _updateVal.ValidateAsync(dto, ct);

        if (!v.IsValid)return BadRequest(ApiResponse<object>.Fail(v.Errors.Select(e => e.ErrorMessage)));

        var result = await _projects.UpdateAsync(id,dto,GetUserId(),GetRoles(),ct);

        return Ok(ApiResponse<ProjectDto>.Ok(
            result,
            "Project updated."));
    }

    // Delete a project
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id,CancellationToken ct)
    {
        await _projects.DeleteAsync(id,GetUserId(),GetRoles(),ct);
        return Ok(ApiResponse.Ok("Project deleted."));
    }


    // ── Members ───────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id,CancellationToken ct)
    {
        var result = await _projects.GetMembersAsync(id, ct);

        return Ok(ApiResponse<IEnumerable<ProjectMemberDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> AddMember(Guid id,[FromBody] AddProjectMemberDto dto,CancellationToken ct)
    {
        if (dto.UserId == Guid.Empty)
        {
            return BadRequest(ApiResponse<object>.Fail("UserId is required"));
        }

        var result = await _projects.AddMemberAsync(id,dto,GetUserId(),GetRoles(),ct);

        return Ok(ApiResponse<ProjectMemberDto>.Ok(result,"Member added."));
    }

    // Remove a member from a project
    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> RemoveMember(Guid id,Guid userId,CancellationToken ct)
    {
        await _projects.RemoveMemberAsync(id,userId,GetUserId(),GetRoles(),ct);

        return Ok(ApiResponse.Ok("Member removed."));
    }

    // get the current user's ID from the claims
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not authenticated");

        return Guid.Parse(userIdClaim); // Will throw if invalid format
    }

    // get the current user's roles from the claims
    private IEnumerable<string> GetRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(x => x.Value);
    }
}
