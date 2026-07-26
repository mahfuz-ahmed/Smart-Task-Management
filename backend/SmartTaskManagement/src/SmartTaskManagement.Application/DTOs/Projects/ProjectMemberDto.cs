namespace SmartTaskManagement.Application.DTOs.Projects;

public sealed record ProjectMemberDto(
    Guid Id,
    Guid ProjectId,
    Guid UserId,
    string UserFullName,
    string UserEmail,
    string ProjectRole,
    Guid InvitedByUserId,
    string InvitedByUserName,
    DateTime JoinedAtUtc,
    bool IsActive
);

public sealed record AddProjectMemberDto(
    Guid UserId,
    int ProjectRole   // 1=Manager, 2=Member
);

public sealed record UpdateProjectMemberRoleDto(
    int ProjectRole
);
