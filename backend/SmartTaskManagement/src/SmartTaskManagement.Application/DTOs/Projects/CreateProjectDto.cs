using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Application.DTOs.Projects;

public sealed record CreateProjectDto(
    string Name,
    string Description,
    int Status,        // ProjectStatus: 0=Planning, 1=Active, 2=OnHold, 3=Completed, 4=Cancelled
    int Priority,      // Priority: 0=Low, 1=Medium, 2=High, 3=Critical
    DateTime? StartDate,
    DateTime? EndDate
);
