namespace SmartTaskManagement.Application.DTOs.Tasks;

public sealed record UpdateTaskDto(
    string Title,
    string Description,
    int Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId = null,
    int? Status = null,
    byte[]? RowVersion = null
);
