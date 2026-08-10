using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Projects;
using SmartTaskManagement.Application.Interfaces.Services;
using SmartTaskManagement.Application.Services;
using SmartTaskManagement.Application.Common.Constants;

namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
[Produces("application/json")]
public sealed class ProjectsController : BaseController
{
    private readonly IProjectService _projectService;
    private readonly IProjectMemberService _projectMemberService;
    private readonly IValidator<CreateProjectDto> _createProjectValidator;
    private readonly IValidator<UpdateProjectDto> _updateProjectValidator;
    private readonly IValidator<AddProjectMemberDto> _addProjectMemberValidator;

    public ProjectsController(IProjectService projectService, IProjectMemberService projectMemberService, IValidator<CreateProjectDto> createProjectValidator, IValidator<UpdateProjectDto> updateProjectValidator, IValidator<AddProjectMemberDto> addProjectMemberValidator)
    {
        _projectService = projectService;
        _projectMemberService = projectMemberService;
        _createProjectValidator = createProjectValidator;
        _updateProjectValidator = updateProjectValidator;
        _addProjectMemberValidator = addProjectMemberValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProjectQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(query, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<PagedResult<ProjectDto>>.Ok(result, SuccessMessages.Retrieved));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetByIdAsync(id, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<ProjectDto>.Ok(result, SuccessMessages.Retrieved));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _createProjectValidator, cancellationToken);

        if (validationResponse is not null)
            return validationResponse;

        var result = await _projectService.CreateAsync(dto, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ProjectDto>.Ok(result, SuccessMessages.Created));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _updateProjectValidator, cancellationToken);

        if (validationResponse is not null)
            return validationResponse;

        var result = await _projectService.UpdateAsync(id, dto, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<ProjectDto>.Ok(result, SuccessMessages.Updated));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _projectService.DeleteAsync(id, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse.Ok("Project deleted successfully."));
    }

    [HttpGet("{id:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid id, CancellationToken cancellationToken)
    {
        var result = await _projectMemberService.GetMembersAsync(id, cancellationToken);

        return Ok(ApiResponse<IEnumerable<ProjectMemberDto>>.Ok(result, SuccessMessages.Retrieved));
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddProjectMemberDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _addProjectMemberValidator, cancellationToken);

        if (validationResponse is not null)
            return validationResponse;

        var result = await _projectMemberService.AddMemberAsync(id, dto, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<ProjectMemberDto>.Ok(result, SuccessMessages.Created));
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _projectMemberService.RemoveMemberAsync(id, userId, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse.Ok(SuccessMessages.Deleted));
    }
}