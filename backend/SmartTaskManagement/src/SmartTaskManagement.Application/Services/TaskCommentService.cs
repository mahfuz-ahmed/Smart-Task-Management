using SmartTaskManagement.Application.DTOs.Comments;
using SmartTaskManagement.Application.Exceptions;
using SmartTaskManagement.Application.Interfaces.Services;
using SmartTaskManagement.Application.Mappings;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Domain.Interfaces;

namespace SmartTaskManagement.Infrastructure.Services;

public sealed class TaskCommentService : ITaskCommentService
{
    private readonly IUnitOfWork _uow;

    public TaskCommentService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<TaskCommentDto>> GetByTaskAsync(Guid taskId, CancellationToken ct = default)
    {
        // Ensure task exists first
        if (!await _uow.Tasks.ExistsAsync(t => t.Id == taskId, ct))
            throw new NotFoundException(nameof(TaskItem), taskId);

        var comments = await _uow.TaskComments.GetByTaskAsync(taskId, ct);
        return comments.Select(c => c.ToDto());
    }

    public async Task<TaskCommentDto> CreateAsync(
        Guid taskId,
        CreateCommentDto dto,
        Guid userId,
        CancellationToken ct = default)
    {
        // 1. Lightweight existence check for task
        if (!await _uow.Tasks.ExistsAsync(t => t.Id == taskId, ct))
            throw new NotFoundException(nameof(TaskItem), taskId);

        // 2. Validate parent comment if this is a reply
        if (dto.ParentCommentId.HasValue)
        {
            var parentExists = await _uow.TaskComments.ExistsAsync(
                c => c.Id == dto.ParentCommentId.Value && c.TaskId == taskId, ct);

            if (!parentExists)
                throw new NotFoundException("Parent TaskComment", dto.ParentCommentId.Value);
        }

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(),
            TaskId = taskId,
            UserId = userId,
            Content = dto.Content.Trim(),
            ParentCommentId = dto.ParentCommentId,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _uow.TaskComments.AddAsync(comment, ct);
        await _uow.SaveChangesAsync(ct);

        // 3. Load comment with details (User entity) for complete DTO mapping
        var savedComment = await _uow.TaskComments.GetByIdAsync(comment.Id, ct)
            ?? throw new NotFoundException(nameof(TaskComment), comment.Id);

        return savedComment.ToDto();
    }

    public async Task<TaskCommentDto> UpdateAsync(
        Guid commentId,
        UpdateCommentDto dto,
        Guid userId,
        CancellationToken ct = default)
    {
        var comment = await _uow.TaskComments.GetByIdAsync(commentId, ct)
            ?? throw new NotFoundException(nameof(TaskComment), commentId);

        if (comment.UserId != userId)
            throw new ForbiddenException("You can only edit your own comments.");

        comment.Content = dto.Content.Trim();
        comment.IsEdited = true;
        comment.EditedAtUtc = DateTime.UtcNow;
        _uow.TaskComments.Update(comment);
        await _uow.SaveChangesAsync(ct);

        return comment.ToDto();
    }

    public async Task DeleteAsync(
        Guid commentId,
        Guid userId,
        IEnumerable<string> roles,
        CancellationToken ct = default)
    {
        var comment = await _uow.TaskComments.GetByIdAsync(commentId, ct)
            ?? throw new NotFoundException(nameof(TaskComment), commentId);

        bool isAdmin = roles.Any(r => string.Equals(r, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase));

        if (comment.UserId != userId && !isAdmin)
            throw new ForbiddenException("You do not have permission to delete this comment.");

        _uow.TaskComments.SoftDelete(comment, userId.ToString());
        await _uow.SaveChangesAsync(ct);
    }
}