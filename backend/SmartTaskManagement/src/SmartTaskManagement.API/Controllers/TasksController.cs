using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Tasks;
using SmartTaskManagement.Application.Interfaces.Services;


namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks")]
[Authorize]
[Produces("application/json")]
public sealed class TasksController : BaseApiController
{
    private readonly ITaskService _tasks;
    private readonly IValidator<CreateTaskDto> _createVal;
    private readonly IValidator<UpdateTaskDto> _updateVal;



public TasksController(ITaskService tasks,
    IValidator<CreateTaskDto> createVal, IValidator<UpdateTaskDto> updateVal)
{
    _tasks = tasks;
    _createVal = createVal;
    _updateVal = updateVal;
}


    [HttpGet]
    public async Task<IActionResult> GetAll(Guid projectId, [FromQuery] TaskQueryDto query, CancellationToken ct)
    {
        var result = await _tasks.GetTasksAsync(projectId, query, GetUserId(), GetRoles(), ct);
        return Ok(ApiResponse<PagedResult<TaskDto>>.Ok(result));
    }

    [HttpGet("~/api/tasks/my-tasks")]
    public async Task<IActionResult> GetMyTasks([FromQuery] TaskQueryDto query, CancellationToken ct)
    {
        var result = await _tasks.GetMyTasksAsync(query, GetUserId(), ct);
        return Ok(ApiResponse<PagedResult<TaskDto>>.Ok(result));
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetById(Guid projectId, Guid taskId, CancellationToken ct)
    {
        var result = await _tasks.GetByIdAsync(projectId, taskId, GetUserId(), GetRoles(), ct);
        return Ok(ApiResponse<TaskDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateTaskDto dto, CancellationToken ct)
    {
        var (isValid, validationResult) = await ValidateRequestAsync(dto, _createVal, ct);
        if (!isValid) return validationResult!;

        var result = await _tasks.CreateAsync(projectId, dto, GetUserId(), GetRoles(), ct);
        return CreatedAtAction(nameof(GetById), new { projectId, taskId = result.Id },
            ApiResponse<TaskDto>.Ok(result, "Task created."));
    }

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update(Guid projectId, Guid taskId, [FromBody] UpdateTaskDto dto, CancellationToken ct)
    {
        var (isValid, validationResult) = await ValidateRequestAsync(dto, _updateVal, ct);
        if (!isValid) return validationResult!;

        var result = await _tasks.UpdateAsync(projectId, taskId, dto, GetUserId(), GetRoles(), ct);
        return Ok(ApiResponse<TaskDto>.Ok(result, "Task updated."));
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid projectId, Guid taskId,
        [FromBody] UpdateTaskStatusDto dto, CancellationToken ct)
    {
        var result = await _tasks.UpdateStatusAsync(projectId, taskId, dto, GetUserId(), GetRoles(), ct);
        return Ok(ApiResponse<TaskDto>.Ok(result, "Status updated."));
    }

    [HttpPatch("{taskId:guid}/assign")]
    public async Task<IActionResult> Assign(Guid projectId, Guid taskId,
        [FromBody] AssignTaskDto dto, CancellationToken ct)
    {
        var result = await _tasks.AssignAsync(projectId, taskId, dto, GetUserId(), GetRoles(), ct);
        return Ok(ApiResponse<TaskDto>.Ok(result, "Task assigned."));
    }

    [HttpDelete("{taskId:guid}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId, CancellationToken ct)
    {
        await _tasks.DeleteAsync(projectId, taskId, GetUserId(), GetRoles(), ct);
        return Ok(ApiResponse.Ok("Task deleted."));
    }

    [HttpGet("{taskId:guid}/activity")]
    public async Task<IActionResult> GetActivity(Guid projectId, Guid taskId, CancellationToken ct)
    {
        var result = await _tasks.GetActivityLogsAsync(taskId, ct);
        return Ok(ApiResponse<IEnumerable<TaskActivityLogDto>>.Ok(result));
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