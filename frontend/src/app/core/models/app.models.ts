// ============================================================
// ALL APPLICATION MODELS / DTOs
// ============================================================

// --- API Response Wrapper ---
export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data: T;
  errors?: string[];
}

// --- Pagination ---
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// --- Project Models ---
export interface Project {
  id: string;
  name: string;
  description: string;
  status?: ProjectStatus;
  priority?: Priority;
  startDate?: string | null;
  endDate?: string | null;
  createdAt?: string;
  updatedAt?: string;
  createdById?: string;
  createdByName?: string;
  members?: ProjectMember[];
  taskStats?: TaskStats;
  // Backend ProjectDto properties
  createdByUserId?: string;
  createdByUserName?: string;
  totalTasks?: number;
  completedTasks?: number;
  memberCount?: number;
  createdAtUtc?: string;
  lastModifiedAtUtc?: string | null;
}

export interface ProjectMember {
  id: string;
  projectId: string;
  userId: string;
  userFullName: string;
  email: string;
  role: string;
  invitedByUserId: string;
  invitedByUserName: string;
  joinedAt: string;
  isActive: boolean;
}

export interface TaskStats {
  total: number;
  todo: number;
  inProgress: number;
  completed: number;
  cancelled: number;
  completionPercentage: number;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
  status?: ProjectStatus;
  priority?: Priority;
  startDate?: string;
  endDate?: string;
}

export interface UpdateProjectRequest {
  name: string;
  description: string;
  status: ProjectStatus;
  priority: Priority;
  startDate?: string;
  endDate?: string;
}

export interface AddMemberRequest {
  userId: string;
  projectRole: number; // 1=Manager, 2=Member
}

// --- Task Models ---
export interface TaskItem {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  statusName?: string;           // Backend provides this
  priority: Priority;
  priorityName?: string;         // Backend provides this
  dueDate: string | null;
  isOverdue?: boolean;           // Backend provides this
  projectId: string;
  projectName: string;
  assignedToUserId: string | null;
  assignedToName: string | null;
  commentCount?: number;         // Backend provides this
  createdAt: string;
  updatedAt: string;
  rowVersion?: string;           // Base64 encoded byte array for concurrency control
  // Optional properties not in backend yet
  createdById?: string;
  createdByName?: string;
  estimatedHours?: number | null;
  tags?: string[];
  comments?: TaskComment[];
  activityLogs?: TaskActivityLog[];
  attachments?: TaskAttachment[];
}

export interface TaskComment {
  id: string;
  taskId?: string;               // Backend provides this
  content: string;
  authorId: string;
  authorName: string;
  createdAt: string;
  updatedAt?: string;            // Backend provides this
  parentCommentId?: string;      // Backend provides this for replies
  replyCount?: number;           // Backend provides this
}

export interface TaskActivityLog {
  id: string;
  action: string;
  description: string;
  createdAt: string;
  performedByName: string;
}

export interface TaskAttachment {
  id: string;
  fileName: string;
  fileUrl: string;
  uploadedAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  priority: Priority;
  status?: TaskStatus;
  dueDate?: string;
  assignedToUserId?: string | null;
  estimatedHours?: number;
  tags?: string[];
}

export interface UpdateTaskRequest {
  title: string;
  description: string;
  priority: Priority;
  status?: TaskStatus;
  dueDate?: string;
  assignedToUserId?: string | null;
  estimatedHours?: number;
  tags?: string[];
  rowVersion?: string;           // For optimistic concurrency control
}

export interface AddCommentRequest {
  content: string;
}

export interface TaskQueryParams {
  projectId?: string;
  status?: TaskStatus;
  priority?: Priority;
  assignedToId?: string;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface ProjectQueryParams {
  search?: string;
  status?: ProjectStatus;
  priority?: Priority;
  page?: number;
  pageSize?: number;
}

// --- Dashboard Models ---
export interface DashboardStats {
  totalProjects: number;
  totalTasks: number;
  myTasks: number;
  completedTasks: number;
  pendingTasks?: number;         // Backend provides this
  overdueTasks: number;
  upcomingTasks: number;
  tasksByStatus: { [key: string]: number };
  tasksByPriority: { [key: string]: number };
  recentActivity: ActivityItem[];
  projectProgress: ProjectProgressItem[];
  upcomingDueTasks?: TaskItem[]; // Backend provides full task list
}

export interface ActivityItem {
  id: string;
  action: string;
  description: string;
  createdAt: string;
  performedByName: string;
  projectName: string;
  taskTitle: string;
}

export interface ProjectProgressItem {
  projectId: string;
  projectName: string;
  completionPercentage: number;
  totalTasks: number;
  completedTasks: number;
}

// --- AI Models ---
export interface EnhanceDescriptionRequest {
  description: string;
  context?: string;
}

export interface EnhanceDescriptionResponse {
  improvedDescription: string;
}

// --- Notification Models ---
export interface NotificationItem {
  id: string;
  type: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
  relatedProjectId?: string;
  relatedTaskId?: string;
}

// --- Enums ---
export enum ProjectStatus {
  Planning = 0,
  Active = 1,
  OnHold = 2,
  Completed = 3,
  Cancelled = 4,
}

export enum TaskStatus {
  ToDo = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
}

export enum Priority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4,
}

// --- Helpers ---
export const TASK_STATUS_LABELS: Record<number, string> = {
  1: 'To Do',
  2: 'In Progress',
  3: 'Completed',
  4: 'Cancelled',
};

export const PRIORITY_LABELS: Record<number, string> = {
  1: 'Low',
  2: 'Medium',
  3: 'High',
  4: 'Critical',
};

export const PROJECT_STATUS_LABELS: Record<number, string> = {
  0: 'Planning',
  1: 'Active',
  2: 'On Hold',
  3: 'Completed',
  4: 'Cancelled',
};
