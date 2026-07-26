namespace SmartTaskManagement.Application.DTOs.AI;

public sealed record ImproveDescriptionDto(
    string Description,
    string? TaskTitle = null
);

public sealed record ImproveDescriptionResponseDto(
    string OriginalDescription,
    string ImprovedDescription
);
