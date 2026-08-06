using SmartTaskManagement.Application.Exceptions;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;

namespace SmartTaskManagement.Infrastructure.Services;

/// <summary>
/// Centralized authorization logic for role-based access control.
/// Implements permission rules for Admin, ProjectManager, and TeamMember roles.
/// </summary>

public static class AuthorizationHelper
{
    #region Role Checks

    public static bool IsAdmin(IEnumerable<string> roles)
        => roles.Contains(UserRole.Admin.ToString());

    public static bool IsProjectManager(IEnumerable<string> roles)
        => roles.Contains(UserRole.ProjectManager.ToString());

    public static bool IsTeamMember(IEnumerable<string> roles)
        => roles.Contains(UserRole.TeamMember.ToString());

    #endregion Role Checks

    #region Project Authorization

    public static void EnsureCanCreateProject(IEnumerable<string> roles)
    {
        if (!IsAdmin(roles))
            throw new ForbiddenException("Only administrators can create projects.");
    }

    public static void EnsureCanUpdateProject(IEnumerable<string> roles)
    {
        if (!IsAdmin(roles))
            throw new ForbiddenException("Only administrators can update projects.");
    }

    public static void EnsureCanDeleteProject(IEnumerable<string> roles)
    {
        if (!IsAdmin(roles))
            throw new ForbiddenException("Only administrators can delete projects.");
    }

    #endregion Project Authorization

    #region Project Task & Member Authorization

    /// <summary>
    /// Used for:
    /// - Create Task
    /// - Delete Task
    /// - Assign Task
    /// - Add/Remove Project Members
    /// </summary>
    public static void EnsureCanManageTasks(
        IEnumerable<string> roles,
        Project project,
        Guid userId)
    {
        // Global Admin
        if (IsAdmin(roles))
            return;

        var membership = GetMembership(project, userId);

        if (membership.ProjectRole == ProjectRole.Manager)
            return;

        throw new ForbiddenException(
            "Only Project Managers can perform this action.");
    }

    #endregion Project Task & Member Authorization

    #region Task Authorization

    /// <summary>
    /// Admin -> Any task
    /// Project Manager -> Any task in own project
    /// Project Member -> Only assigned task
    /// </summary>
    public static void EnsureCanUpdateTask(
        IEnumerable<string> roles,
        TaskItem task,
        Project project,
        Guid userId)
    {
        if (IsAdmin(roles))
            return;

        var membership = GetMembership(project, userId);

        if (membership.ProjectRole == ProjectRole.Manager)
            return;

        if (task.AssignedToUserId == userId)
            return;

        throw new ForbiddenException(
            "You can only update tasks assigned to you.");
    }

    /// <summary>
    /// Admin -> Any task status
    /// Project Manager -> Any task status in own project
    /// Project Member -> Only assigned task status
    /// </summary>
    public static void EnsureCanUpdateTaskStatus(
        IEnumerable<string> roles,
        TaskItem task,
        Project project,
        Guid userId)
    {
        if (IsAdmin(roles))
            return;

        var membership = GetMembership(project, userId);

        if (membership.ProjectRole == ProjectRole.Manager)
            return;

        if (task.AssignedToUserId == userId)
            return;

        throw new ForbiddenException(
            "You can only update the status of tasks assigned to you.");
    }

    #endregion Task Authorization

    #region Private Helpers

    private static ProjectMember GetMembership(
        Project project,
        Guid userId)
    {
        var membership = project.Members.FirstOrDefault(m =>
            m.UserId == userId &&
            m.IsActive);

        if (membership == null)
        {
            throw new ForbiddenException(
                "You are not a member of this project.");
        }

        return membership;
    }

    #endregion Private Helpers

    public static void EnsureCanManageMembers(
        IEnumerable<string> roles,
        Project project,
        Guid userId)
    {
        var roleList = roles.ToList();

        // Level 1: Global Admin bypass
        if (IsAdmin(roleList))
            return; // Admin can manage members anywhere

        // Level 2: Team Members cannot manage members
        if (IsTeamMember(roleList))
            throw new ForbiddenException("Team Members cannot manage project members.");

        // Level 3: ProjectManager - Check project membership and project-level role
        if (IsProjectManager(roleList))
        {
            var membership = project.Members?
                .FirstOrDefault(m => m.UserId == userId && m.IsActive);

            if (membership == null)
                throw new ForbiddenException(
                    "You are not a member of this project.");

            // Level 4: Check project-specific role
            if (membership.ProjectRole == ProjectRole.Manager)
                return; // Manager can manage members

            // ProjectManager (system) but Member (project) = cannot manage members
            throw new ForbiddenException(
                "You are assigned as a Member in this project. Only Project Managers can manage members.");
        }

        throw new ForbiddenException("You do not have permission to manage project members.");
    }
}