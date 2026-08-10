using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Common.Constants;
using SmartTaskManagement.Application.DTOs.Comments;
using SmartTaskManagement.Application.Interfaces.Services;

namespace SmartTaskManagement.API.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks/{taskId:guid}/comments")]
[Authorize]
[Produces("application/json")]
public sealed class CommentsController : BaseController
{
    private readonly ITaskCommentService _commentService;
    private readonly IValidator<CreateCommentDto> _createCommentValidator;
    private readonly IValidator<UpdateCommentDto> _updateCommentValidator;

    public CommentsController(ITaskCommentService commentService, IValidator<CreateCommentDto> createCommentValidator, IValidator<UpdateCommentDto> updateCommentValidator)
    {
        _commentService = commentService;
        _createCommentValidator = createCommentValidator;
        _updateCommentValidator = updateCommentValidator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid projectId, Guid taskId, CancellationToken cancellationToken)
    {
        var result = await _commentService.GetByTaskAsync(taskId, cancellationToken);

        return Ok(ApiResponse<IEnumerable<TaskCommentDto>>.Ok(result, SuccessMessages.Retrieved));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, Guid taskId, [FromBody] CreateCommentDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _createCommentValidator, cancellationToken);

        if (validationResponse is not null) return validationResponse;

        var result = await _commentService.CreateAsync(taskId, dto, GetCurrentUserId(), cancellationToken);

        return Ok(ApiResponse<TaskCommentDto>.Ok(result, SuccessMessages.Created));
    }

    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> Update(Guid projectId, Guid taskId, Guid commentId, [FromBody] UpdateCommentDto dto, CancellationToken cancellationToken)
    {
        var validationResponse = await ValidateAsync(dto, _updateCommentValidator, cancellationToken);

        if (validationResponse is not null) return validationResponse;

        var result = await _commentService.UpdateAsync(commentId, dto, GetCurrentUserId(), cancellationToken);

        return Ok(ApiResponse<TaskCommentDto>.Ok(result, SuccessMessages.Updated));
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId, Guid commentId, CancellationToken cancellationToken)
    {
        await _commentService.DeleteAsync(commentId, GetCurrentUserId(), GetCurrentUserRoles(), cancellationToken);

        return Ok(ApiResponse.Ok("Comment deleted."));
    }
}