using SmartTaskManagement.Application.DTOs.Comments;

namespace SmartTaskManagement.Application.Interfaces.Services;

public interface ITaskCommentService
{
    Task<IEnumerable<TaskCommentDto>> GetByTaskAsync(Guid taskId, CancellationToken ct = default);

    Task<TaskCommentDto> CreateAsync(Guid taskId, CreateCommentDto dto, Guid userId, CancellationToken ct = default);

    Task<TaskCommentDto> UpdateAsync(Guid commentId, UpdateCommentDto dto, Guid userId, CancellationToken ct = default);

    Task DeleteAsync(Guid commentId, Guid userId, IEnumerable<string> roles, CancellationToken ct = default);
}