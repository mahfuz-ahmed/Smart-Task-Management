namespace SmartTaskManagement.Application.DTOs.Comments;

public sealed record TaskCommentDto(
    Guid Id,
    Guid TaskId,
    Guid UserId,
    string UserFullName,
    string Content,
    bool IsEdited,
    DateTime? EditedAtUtc,
    Guid? ParentCommentId,
    IEnumerable<TaskCommentDto> Replies,
    DateTime CreatedAtUtc
);

public sealed record CreateCommentDto(
    string Content,
    Guid? ParentCommentId
);

public sealed record UpdateCommentDto(
    string Content
);
