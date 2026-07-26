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
    /// <summary>
    /// Check if user has Admin role.
    /// </summary>
    public static bool IsAdmin(IEnumerable<string> roles)
    {
        return roles.Contains(UserRole.Admin.ToString());
    }

    /// <summary>
    /// Check if user has ProjectManager role.
    /// </summary>
    public static bool IsProjectManager(IEnumerable<string> roles)
    {
        return roles.Contains(UserRole.ProjectManager.ToString());
    }

    /// <summary>
    /// Check if user has TeamMember role.
    /// </summary>
    public static bool IsTeamMember(IEnumerable<string> roles)
    {
        return roles.Contains(UserRole.TeamMember.ToString());
    }

    /// <summary>
    /// Check if user can create projects (Admin only).
    /// </summary>
    public static void EnsureCanCreateProject(IEnumerable<string> roles)
    {
        if (!IsAdmin(roles))
            throw new ForbiddenException("Only Admins can create projects.");
    }

    /// <summary>
    /// Check if user can update a project.
    /// Admin: Can update any project
    /// ProjectManager: Cannot update projects (Admin only)
    /// TeamMember: Cannot update projects
    /// </summary>
    public static void EnsureCanUpdateProject(
        IEnumerable<string> roles,
        Project project,
        Guid userId)
    {
        if (IsAdmin(roles))
            return; // Only Admin can update projects

        throw new ForbiddenException("Only Admins can update projects.");
    }

    /// <summary>
    /// Check if user can delete a project.
    /// Admin: Can delete any project
    /// ProjectManager: Cannot delete projects (Admin only)
    /// TeamMember: Cannot delete projects
    /// </summary>
    public static void EnsureCanDeleteProject(
        IEnumerable<string> roles,
        Guid projectCreatorId,
        Guid userId)
    {
        if (IsAdmin(roles))
            return; // Only Admin can delete projects

        throw new ForbiddenException("Only Admins can delete projects.");
    }

    /// <summary>
    /// Check if user can create tasks.
    /// Admin: Can create tasks in any project
    /// ProjectManager: Can create tasks ONLY if assigned as Manager in project
    /// TeamMember: Cannot create tasks
    /// </summary>
    public static void EnsureCanCreateTask(
        IEnumerable<string> roles,
        Project project,
        Guid userId)
    {
        var roleList = roles.ToList();

        // Level 1: Global Admin bypass
        if (IsAdmin(roleList))
            return; // Admin can create tasks anywhere

        // Level 2: Team Members cannot create tasks
        if (IsTeamMember(roleList))
            throw new ForbiddenException("Team Members cannot create tasks. Please ask your Project Manager.");

        // Level 3: ProjectManager - Check project membership and project-level role
        if (IsProjectManager(roleList))
        {
            var membership = project.Members?
                .FirstOrDefault(m => m.UserId == userId && m.IsActive);

            if (membership == null)
                throw new ForbiddenException(
                    "You are not a member of this project. Please ask a Project Manager to add you.");

            // Level 4: Check project-specific role (Industry Standard)
            if (membership.ProjectRole == ProjectRole.Manager)
                return; //  ProjectManager (system) + Manager (project) = allowed

            // ProjectManager (system) but Member (project) = restricted
            throw new ForbiddenException(
                "You are assigned as a Member in this project. Only Project Managers can create tasks. " +
                "Contact the project owner to upgrade your role.");
        }

        throw new ForbiddenException("You do not have permission to create tasks.");
    }

    /// <summary>
    /// Check if user can update a task.
    /// Admin: Can update any task
    /// ProjectManager: Can update tasks ONLY if assigned as Manager in project
    /// TeamMember: Can update ONLY tasks assigned to them
    /// </summary>
    public static void EnsureCanUpdateTask(
        IEnumerable<string> roles,
        TaskItem task,
        Project project,
        Guid userId)
    {
        var roleList = roles.ToList();

        // Level 1: Global Admin bypass
        if (IsAdmin(roleList))
            return; // Admin can update any task

        // Level 2: TeamMember can ONLY update tasks assigned to them
        if (IsTeamMember(roleList))
        {
            if (task.AssignedToUserId != userId)
                throw new ForbiddenException("You can only update tasks assigned to you.");

            return; // TeamMember updating their own task
        }

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
                return; // Manager can update tasks in their project

            // ProjectManager (system) but Member (project) = can only update assigned tasks
            if (task.AssignedToUserId != userId)
                throw new ForbiddenException(
                    "You are assigned as a Member in this project. You can only update tasks assigned to you.");

            return; // Member updating their assigned task
        }

        throw new ForbiddenException("You do not have permission to update this task.");
    }

    /// <summary>
    /// Check if user can delete a task.
    /// Admin: Can delete any task
    /// ProjectManager: Can delete tasks ONLY if assigned as Manager in project
    /// TeamMember: Cannot delete tasks
    /// </summary>
    public static void EnsureCanDeleteTask(
        IEnumerable<string> roles,
        Project project,
        Guid userId)
    {
        var roleList = roles.ToList();

        // Level 1: Global Admin bypass
        if (IsAdmin(roleList))
            return; // Admin can delete any task

        // Level 2: Team Members cannot delete tasks
        if (IsTeamMember(roleList))
            throw new ForbiddenException("Team Members cannot delete tasks.");

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
                return; //  Manager can delete tasks

            // ProjectManager (system) but Member (project) = cannot delete
            throw new ForbiddenException(
                "You are assigned as a Member in this project. Only Project Managers can delete tasks.");
        }

        throw new ForbiddenException("You do not have permission to delete this task.");
    }

    /// <summary>
    /// Check if user can manage project members (add/remove).
    /// Admin: Can manage members in any project
    /// ProjectManager: Can manage members ONLY if assigned as Manager in project
    /// TeamMember: Cannot manage members
    /// </summary>
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
