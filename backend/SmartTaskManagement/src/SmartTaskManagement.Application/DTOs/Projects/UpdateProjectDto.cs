namespace SmartTaskManagement.Application.DTOs.Projects;

public sealed record UpdateProjectDto(
    string Name,
    string Description,
    int Status,        // ProjectStatus enum value
    int Priority,      // Priority enum value
    DateTime? StartDate,
    DateTime? EndDate
);
