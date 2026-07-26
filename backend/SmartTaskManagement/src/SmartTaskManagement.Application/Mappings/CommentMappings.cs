using SmartTaskManagement.Application.DTOs.Comments;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Application.Mappings;

public static class CommentMappings
{
    public static TaskCommentDto ToDto(this TaskComment c) => new(
        c.Id,
        c.TaskId,
        c.UserId,
        c.User?.FullName ?? string.Empty,
        c.IsDeleted ? "[comment removed]" : c.Content,
        c.IsEdited,
        c.EditedAtUtc,
        c.ParentCommentId,
        c.Replies.Where(r => !r.IsDeleted).Select(r => r.ToDto()),
        c.CreatedAtUtc
    );
}
