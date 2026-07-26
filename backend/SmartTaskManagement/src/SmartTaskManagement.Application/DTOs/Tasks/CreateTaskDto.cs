namespace SmartTaskManagement.Application.DTOs.Tasks;

public sealed record CreateTaskDto(
    string Title,
    string Description,
    int Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId = null,
    int? Status = null
);
