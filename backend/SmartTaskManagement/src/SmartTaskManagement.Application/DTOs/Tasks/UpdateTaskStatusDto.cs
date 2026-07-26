using SmartTaskManagement.Domain.Enums;
using TaskStatus = SmartTaskManagement.Domain.Enums.TaskStatus;

namespace SmartTaskManagement.Application.DTOs.Tasks;

public sealed record UpdateTaskStatusDto(
    TaskStatus Status
);
