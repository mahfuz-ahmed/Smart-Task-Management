namespace SmartTaskManagement.Application.DTOs.Tasks;

public sealed record AssignTaskDto(
    Guid? AssignedToUserId
);
