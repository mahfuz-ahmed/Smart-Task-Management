using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Tasks;
using SmartTaskManagement.Application.Interfaces.Services;
using SmartTaskManagement.Application.Common.Constants;

namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks")]
[Authorize]
[Produces("application/json")]
public sealed class TasksController : BaseController
{
    private readonly ITaskService _taskService;
    private readonly IValidator<CreateTaskDto> _createTaskValidator;
    private readonly IValidator<UpdateTaskDto> _updateTaskValidator;
    private readonly IValidator<UpdateTaskStatusDto> _updateTaskStatusValidator;
    //private readonly IValidator<AssignTaskDto> _assignTaskValidator;

    public TasksController(ITaskService taskService, IValidator<CreateTaskDto> createTaskValidator, IValidator<UpdateTaskDto> updateTaskValidator, IValidator<UpdateTaskStatusDto> updateTaskStatusValidator)
    {
        _taskService = taskService;
        _createTaskValidator = createTaskValidator;
        _updateTaskValidator = updateTaskValidator;
        _updateTaskStatusValidator = updateTaskStatusValidator;
        //_assignTaskValidator = assignTaskValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid projectId, [FromQuery] TaskQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetTasksAsync(projectId, query, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<PagedResult<TaskDto>>.Ok(result, SuccessMessages.Retrieved));
    }

    [HttpGet("~/api/tasks/my-tasks")]
    public async Task<IActionResult> GetMyTasks([FromQuery] TaskQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetMyTasksAsync(query, GetCurrentUserId(), cancellationToken);

        return Ok(ApiResponse<PagedResult<TaskDto>>.Ok(result, SuccessMessages.Retrieved));
    }

    [HttpGet("{taskId:guid}")]
    public async Task<IActionResult> GetById(Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetByIdAsync(projectId, taskId, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<TaskDto>.Ok(result, SuccessMessages.Retrieved));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _createTaskValidator, cancellationToken);

        if (validationResponse is not null) return validationResponse;

        var result = await _taskService.CreateAsync(projectId, dto, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return CreatedAtAction(nameof(GetById), new { projectId, taskId = result.Id }, ApiResponse<TaskDto>.Ok(result, SuccessMessages.Created));
    }

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update(Guid projectId, Guid taskId, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _updateTaskValidator, cancellationToken);

        if (validationResponse is not null) return validationResponse;

        var result = await _taskService.UpdateAsync(projectId, taskId, dto, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<TaskDto>.Ok(result, SuccessMessages.Updated));
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid projectId, Guid taskId, [FromBody] UpdateTaskStatusDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _updateTaskStatusValidator, cancellationToken);

        if (validationResponse is not null) return validationResponse;

        var result = await _taskService.UpdateStatusAsync(projectId, taskId, dto, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<TaskDto>.Ok(result, SuccessMessages.Updated));
    }

    [HttpPatch("{taskId:guid}/assign")]
    public async Task<IActionResult> Assign(Guid projectId, Guid taskId, [FromBody] AssignTaskDto dto, CancellationToken cancellationToken)
    {
        //var validationResponse = await ValidateAsync(dto, _assignTaskValidator, cancellationToken);

        //if (validationResponse is not null) return validationResponse;

        var result = await _taskService.AssignAsync(projectId, taskId, dto, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse<TaskDto>.Ok(result, SuccessMessages.Assigned));
    }

    [HttpDelete("{taskId:guid}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        await _taskService.DeleteAsync(projectId, taskId, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse.Ok(SuccessMessages.Deleted));
    }

    [HttpGet("{taskId:guid}/activity")]
    public async Task<IActionResult> GetActivity(Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        var result = await _taskService.GetActivityLogsAsync(taskId, cancellationToken);

        return Ok(ApiResponse<IEnumerable<TaskActivityLogDto>>.Ok(result, SuccessMessages.Retrieved));
    }
}