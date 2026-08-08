using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.DTOs.Comments;
using SmartTaskManagement.Application.Interfaces.Services;

namespace SmartTaskManagement.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/tasks/{taskId:guid}/comments")]
    [Authorize]
    [Produces("application/json")]
    public sealed class CommentsController : ControllerBase
    {
        private readonly ITaskCommentService _comments;
        private readonly IValidator<CreateCommentDto> _createVal;
        

        public CommentsController(ITaskCommentService comments, IValidator<CreateCommentDto> createVal)
        {
            _comments = comments;
            _createVal = createVal;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid projectId, Guid taskId, CancellationToken ct)
        {
            var result = await _comments.GetByTaskAsync(taskId, ct);
            return Ok(ApiResponse<IEnumerable<TaskCommentDto>>.Ok(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid projectId, Guid taskId, [FromBody] CreateCommentDto dto, CancellationToken ct)
        {
            var (isValid, errorResponse) = await ValidateRequestAsync(dto, _createVal, ct);
            if (!isValid) return errorResponse!;

            var result = await _comments.CreateAsync(taskId, dto, GetUserId(), ct);
            return Ok(ApiResponse<TaskCommentDto>.Ok(result, "Comment posted."));
        }

        [HttpPut("{commentId:guid}")]
        public async Task<IActionResult> Update(Guid taskId, Guid commentId, [FromBody] UpdateCommentDto dto, CancellationToken ct)
        {
            var result = await _comments.UpdateAsync(commentId, dto, GetUserId(), ct);
            return Ok(ApiResponse<TaskCommentDto>.Ok(result, "Comment updated."));
        }

        [HttpDelete("{commentId:guid}")]
        public async Task<IActionResult> Delete(Guid taskId, Guid commentId, CancellationToken ct)
        {
            await _comments.DeleteAsync(commentId, GetUserId(), GetRoles(), ct);
            return Ok(ApiResponse.Ok("Comment deleted."));
        }

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private IEnumerable<string> GetRoles() => User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        private async Task<(bool IsValid, IActionResult? Response)> ValidateRequestAsync<T>(
            T dto, IValidator<T> validator, CancellationToken ct)
        {
            var result = await validator.ValidateAsync(dto, ct);
            if (!result.IsValid)
                return (false, BadRequest(ApiResponse<object>.Fail(
                    result.Errors.Select(e => e.ErrorMessage))));
            return (true, null);
        }
    }
}