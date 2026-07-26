import { Injectable } from '@angular/core';
import {
  Project,
  ProjectMember,
  TaskItem,
  TaskComment,
  DashboardStats,
} from '../models/app.models';

/**
 * Service to map backend DTOs to frontend models
 * Handles property name differences and data transformations
 */
@Injectable({
  providedIn: 'root',
})
export class ModelMapperService {
  /**
   * Map backend ProjectMemberDto to frontend ProjectMember
   */
  mapProjectMember(dto: any): ProjectMember {
    return {
      id: dto.id || dto.Id,
      projectId: dto.projectId || dto.ProjectId,
      userId: dto.userId || dto.UserId,
      userFullName: dto.userFullName || dto.UserFullName,
      email: dto.userEmail || dto.UserEmail,
      role: dto.projectRole || dto.ProjectRole,
      invitedByUserId: dto.invitedByUserId || dto.InvitedByUserId,
      invitedByUserName: dto.invitedByUserName || dto.InvitedByUserName,
      joinedAt: dto.joinedAtUtc || dto.JoinedAtUtc || dto.joinedAt,
      isActive: dto.isActive !== undefined ? dto.isActive : dto.IsActive !== undefined ? dto.IsActive : true,
    };
  }

  /**
   * Map backend TaskDto to frontend TaskItem
   */
  mapTask(dto: any): TaskItem {
    return {
      id: dto.id || dto.Id,
      title: dto.title || dto.Title,
      description: dto.description || dto.Description || '',
      status: dto.status || dto.Status,
      statusName: dto.statusName || dto.StatusName,
      priority: dto.priority || dto.Priority,
      priorityName: dto.priorityName || dto.PriorityName,
      dueDate: dto.dueDate || dto.DueDate || null,
      isOverdue: dto.isOverdue || dto.IsOverdue || false,
      projectId: dto.projectId || dto.ProjectId,
      projectName: dto.projectName || dto.ProjectName || '',
      assignedToUserId: dto.assignedToUserId || dto.AssignedToUserId || null,
      assignedToName: dto.assignedToUserName || dto.AssignedToUserName || null,
      commentCount: dto.commentCount || dto.CommentCount || 0,
      createdAt: dto.createdAtUtc || dto.CreatedAtUtc || dto.createdAt,
      updatedAt: dto.lastModifiedAtUtc || dto.LastModifiedAtUtc || dto.updatedAt,
      // Optional properties
      createdById: dto.createdById || dto.CreatedById,
      createdByName: dto.createdByName || dto.CreatedByName,
      estimatedHours: dto.estimatedHours || dto.EstimatedHours,
      tags: dto.tags || dto.Tags || [],
      comments: dto.comments || dto.Comments || [],
      activityLogs: dto.activityLogs || dto.ActivityLogs || [],
      attachments: dto.attachments || dto.Attachments || [],
    };
  }

  /**
   * Map backend TaskCommentDto to frontend TaskComment
   */
  mapTaskComment(dto: any): TaskComment {
    return {
      id: dto.id || dto.Id,
      taskId: dto.taskId || dto.TaskId,
      content: dto.content || dto.Content,
      authorId: dto.authorUserId || dto.AuthorUserId,
      authorName: dto.authorUserName || dto.AuthorUserName,
      createdAt: dto.createdAtUtc || dto.CreatedAtUtc || dto.createdAt,
      updatedAt: dto.updatedAtUtc || dto.UpdatedAtUtc,
      parentCommentId: dto.parentCommentId || dto.ParentCommentId,
      replyCount: dto.replyCount || dto.ReplyCount || 0,
    };
  }

  /**
   * Map backend ProjectDto to frontend Project
   */
  mapProject(dto: any): Project {
    return {
      id: dto.id || dto.Id,
      name: dto.name || dto.Name,
      description: dto.description || dto.Description || '',
      status: dto.status || dto.Status,
      priority: dto.priority || dto.Priority,
      startDate: dto.startDate || dto.StartDate || null,
      endDate: dto.endDate || dto.EndDate || null,
      createdById: dto.createdByUserId || dto.CreatedByUserId,
      createdByName: dto.createdByUserName || dto.CreatedByUserName,
      totalTasks: dto.totalTasks || dto.TotalTasks || 0,
      completedTasks: dto.completedTasks || dto.CompletedTasks || 0,
      memberCount: dto.memberCount || dto.MemberCount || 0,
      createdAt: dto.createdAtUtc || dto.CreatedAtUtc || dto.createdAt,
      updatedAt: dto.lastModifiedAtUtc || dto.LastModifiedAtUtc || dto.updatedAt,
      // Map members if provided
      members: dto.members ? dto.members.map((m: any) => this.mapProjectMember(m)) : [],
      // Calculate task stats
      taskStats: {
        total: dto.totalTasks || dto.TotalTasks || 0,
        completed: dto.completedTasks || dto.CompletedTasks || 0,
        todo: 0,
        inProgress: 0,
        cancelled: 0,
        completionPercentage:
          dto.totalTasks && dto.totalTasks > 0
            ? Math.round((dto.completedTasks / dto.totalTasks) * 100)
            : 0,
      },
    };
  }

  /**
   * Map backend DashboardStatsDto to frontend DashboardStats
   */
  mapDashboardStats(dto: any): DashboardStats {
    return {
      totalProjects: dto.totalProjects || dto.TotalProjects || 0,
      totalTasks: dto.totalTasks || dto.TotalTasks || 0,
      myTasks: dto.myTasks || dto.MyTasks || 0,
      completedTasks: dto.completedTasks || dto.CompletedTasks || 0,
      pendingTasks: dto.pendingTasks || dto.PendingTasks,
      overdueTasks: dto.overdueTasks || dto.OverdueTasks || 0,
      upcomingTasks: dto.upcomingTasks || dto.UpcomingTasks || 0,
      tasksByStatus: dto.tasksByStatus || dto.TasksByStatus || {},
      tasksByPriority: dto.tasksByPriority || dto.TasksByPriority || {},
      recentActivity: dto.recentActivity || dto.RecentActivity || [],
      projectProgress: dto.projectProgress || dto.ProjectProgress || [],
      upcomingDueTasks: dto.upcomingDueTasks || dto.UpcomingDueTasks
        ? (dto.upcomingDueTasks || dto.UpcomingDueTasks).map((t: any) => this.mapTask(t))
        : undefined,
    };
  }

  /**
   * Helper to handle both PascalCase (C#) and camelCase (TypeScript) responses
   */
  normalizeResponse<T>(data: any, mapper?: (dto: any) => T): T {
    if (!data) return data;
    if (mapper) return mapper(data);
    return data;
  }
}
