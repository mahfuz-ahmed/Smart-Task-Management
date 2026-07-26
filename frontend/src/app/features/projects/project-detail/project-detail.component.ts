import { Component, inject, signal, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  FormControl,
} from '@angular/forms';
import { ProjectService } from '../../../core/services/project.service';
import { TaskService } from '../../../core/services/task.service';
import { AiService } from '../../../core/services/ai.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/user.service';
import {
  Project,
  TaskItem,
  TaskStatus,
  Priority,
  CreateTaskRequest,
  UpdateTaskRequest,
  AddCommentRequest,
  TASK_STATUS_LABELS,
  PRIORITY_LABELS,
  AddMemberRequest,
} from '../../../core/models/app.models';
import { UserProfile } from '../../../core/models/auth.models';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';
import { ConfirmationModalComponent } from '../../../shared/components/confirmation-modal/confirmation-modal.component';
import { debounceTime, distinctUntilChanged, filter, switchMap, map } from 'rxjs';

@Component({
  selector: 'app-project-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, MatDialogModule, ConfirmationModalComponent],
  templateUrl: './project-detail.component.html',
  styleUrl: './project-detail.component.css',
})
export class ProjectDetailComponent implements OnInit {
  @Input() id!: string; // from route param via withComponentInputBinding

  private projectService = inject(ProjectService);
  private taskService = inject(TaskService);
  private aiService = inject(AiService);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);
  private fb = inject(FormBuilder);
  private dialog = inject(MatDialog);
  private userService = inject(UserService);

  project = signal<Project | null>(null);
  tasks = signal<TaskItem[]>([]);
  loading = signal(true);
  activeTab = signal<'kanban' | 'list'>('kanban');

  showTaskModal = signal(false);
  editingTask = signal<TaskItem | null>(null);
  defaultStatus = signal<number>(TaskStatus.ToDo);
  savingTask = signal(false);
  viewingTask = signal<TaskItem | null>(null);
  deleteTaskTarget = signal<TaskItem | null>(null);

  showMembersModal = signal(false);
  savingMember = signal(false);

  // Remove member modal signals
  showRemoveMemberModal = signal(false);
  removeMemberTarget = signal<any>(null);
  removingMember = signal(false);

  isAddingMember = signal(false);
  searchControl = new FormControl('');
  projectRoleControl = new FormControl(2);
  selectedUser = signal<UserProfile | null>(null);

  filteredUsers$ = this.searchControl.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    filter((val: any) => typeof val === 'string' && val.trim().length >= 2),
    switchMap((term) => this.userService.search(term, this.id, 10)),
    map((res) => (res.success ? res.data : [])),
  );

  savingComment = signal(false);
  enhancing = signal(false);

  kanbanColumns = [
    { status: TaskStatus.ToDo, label: 'To Do', color: '#6b7280' },
    { status: TaskStatus.InProgress, label: 'In Progress', color: '#3b82f6' },
    { status: TaskStatus.Completed, label: 'Completed', color: '#10b981' },
    { status: TaskStatus.Cancelled, label: 'Cancelled', color: '#ef4444' },
  ];

  taskStatuses = [
    { value: TaskStatus.ToDo, label: 'To Do' },
    { value: TaskStatus.InProgress, label: 'In Progress' },
    { value: TaskStatus.Completed, label: 'Completed' },
    { value: TaskStatus.Cancelled, label: 'Cancelled' },
  ];

  priorities = [
    { value: Priority.Low, label: 'Low' },
    { value: Priority.Medium, label: 'Medium' },
    { value: Priority.High, label: 'High' },
    { value: Priority.Critical, label: 'Critical' },
  ];

  taskForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    priority: [Priority.Medium, Validators.required],
    status: [TaskStatus.ToDo],
    dueDate: [''],
    estimatedHours: [null],
    assignedToUserId: [''],
  });

  commentForm: FormGroup = this.fb.group({
    content: ['', Validators.required],
  });

  // Expose enums for template
  TaskStatus = TaskStatus;
  Priority = Priority;

  ngOnInit() {
    this.loadProject();
    this.loadTasks();
  }

  loadProject() {
    this.projectService.getProject(this.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.project.set(res.data);
          // Load members separately (backend ProjectDto doesn't embed them)
          this.projectService.getMembers(this.id).subscribe({
            next: (mRes) => {
              if (mRes.success) {
                this.project.update((p) => (p ? { ...p, members: mRes.data } : p));
              }
            },
          });
        }
      },
      error: () => this.toastService.error('Error', 'Failed to load project'),
    });
  }

  private normalizeTask(t: any): TaskItem {
    // Normalize status - handle both string and number formats
    let statusNum = TaskStatus.ToDo;
    if (typeof t.status === 'number') {
      statusNum = t.status;
      if (statusNum < TaskStatus.ToDo) statusNum = TaskStatus.ToDo;
      if (statusNum > TaskStatus.Cancelled) statusNum = TaskStatus.Cancelled;
    } else if (typeof t.status === 'string') {
      const statusMap: Record<string, number> = {
        ToDo: TaskStatus.ToDo,
        'To Do': TaskStatus.ToDo,
        InProgress: TaskStatus.InProgress,
        'In Progress': TaskStatus.InProgress,
        Completed: TaskStatus.Completed,
        Cancelled: TaskStatus.Cancelled,
        '0': TaskStatus.ToDo,
        '1': TaskStatus.InProgress,
        '2': TaskStatus.InProgress,
        '3': TaskStatus.Completed,
        '4': TaskStatus.Cancelled,
      };
      statusNum = statusMap[t.status] ?? TaskStatus.ToDo;
    }

    // Normalize priority - handle both string and number formats
    let priorityNum = Priority.Medium;
    if (typeof t.priority === 'number') {
      priorityNum = t.priority;
    } else if (typeof t.priority === 'string') {
      const priorityMap: Record<string, number> = {
        Low: Priority.Low,
        Medium: Priority.Medium,
        High: Priority.High,
        Critical: Priority.Critical,
        '0': Priority.Low,
        '1': Priority.Medium,
        '2': Priority.High,
        '3': Priority.Critical,
      };
      priorityNum = priorityMap[t.priority] ?? Priority.Medium;
    }

    return {
      ...t,
      id: t.id,
      title: t.title,
      description: t.description || '',
      status: statusNum as TaskStatus,
      priority: priorityNum as Priority,
      dueDate: t.dueDate || null,
      projectId: t.projectId || this.id,
      projectName: t.projectName || '',
      assignedToUserId: t.assignedToUserId || t.assignedToUserId || null,
      assignedToName: t.assignedToUserName || t.assignedToName || null,
      createdById: t.createdById || '',
      createdByName: t.createdByName || '',
      createdAt: t.createdAtUtc || t.createdAt || new Date().toISOString(),
      updatedAt: t.lastModifiedAtUtc || t.updatedAt || new Date().toISOString(),
      rowVersion: t.rowVersion || undefined,
      estimatedHours: t.estimatedHours || null,
      tags: t.tags || [],
      comments: t.comments || [],
      activityLogs: t.activityLogs || [],
      attachments: t.attachments || [],
    };
  }

  loadTasks() {
    this.loading.set(true);
    this.taskService.getTasks(this.id, { pageSize: 100 }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          const items = (res.data.items || []).map((t) => this.normalizeTask(t));
          this.tasks.set(items);
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error', 'Failed to load tasks');
      },
    });
  }

  getTasksForStatus(status: TaskStatus | number): TaskItem[] {
    const statusNum = typeof status === 'number' ? status : status;
    return this.tasks().filter((t) => (t.status ?? TaskStatus.ToDo) === statusNum);
  }

  openCreateTask(status: TaskStatus | number = TaskStatus.ToDo) {
    this.editingTask.set(null);
    this.defaultStatus.set(typeof status === 'number' ? status : status);
    this.taskForm.reset({
      title: '',
      description: '',
      priority: Priority.Medium,
      status: status,
      dueDate: '',
      estimatedHours: null,
      assignedToUserId: '',
    });
    this.showTaskModal.set(true);
  }

  openEditTask(task: TaskItem) {
    this.viewingTask.set(null);
    this.editingTask.set(task);
    this.taskForm.patchValue({
      title: task.title,
      description: task.description,
      priority: task.priority || Priority.Medium,
      status: task.status || TaskStatus.ToDo,
      dueDate: task.dueDate?.split('T')[0] || '',
      estimatedHours: task.estimatedHours,
      assignedToUserId: task.assignedToUserId || '',
    });
    this.showTaskModal.set(true);
  }

  closeTaskModal() {
    this.showTaskModal.set(false);
    this.editingTask.set(null);
  }

  openTaskDetail(task: TaskItem) {
    this.taskService.getTask(this.id, task.id).subscribe({
      next: (res) => {
        if (res.success) this.viewingTask.set(this.normalizeTask(res.data));
      },
      error: () => this.viewingTask.set(task),
    });
  }

  isTaskInvalid(field: string): boolean {
    const c = this.taskForm.get(field);
    return !!(c?.invalid && (c?.touched || c?.dirty));
  }

  onTaskSubmit() {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }
    this.savingTask.set(true);
    const raw = this.taskForm.value;

    const priorityMap: Record<string, number> = {
      Low: 1,
      Medium: 2,
      High: 3,
      Critical: 4,
      '1': 1,
      '2': 2,
      '3': 3,
      '4': 4,
    };
    const priorityInt = priorityMap[raw.priority] || 2;
    const desc = (raw.description || raw.title || '').trim();

    let formattedDueDate: string | null = null;
    if (raw.dueDate) {
      try {
        formattedDueDate = new Date(raw.dueDate).toISOString();
      } catch {
        formattedDueDate = null;
      }
    }

    const data: any = {
      title: raw.title.trim(),
      description: desc,
      priority: priorityInt,
      status: raw.status ?? TaskStatus.ToDo,
      dueDate: formattedDueDate,
      assignedToUserId: raw.assignedToUserId || null,
      estimatedHours: raw.estimatedHours ?? null,
    };

    const editing = this.editingTask();
    if (editing) {
      // Include rowVersion for concurrency control
      if (editing.rowVersion) {
        data.rowVersion = editing.rowVersion;
      }
      
      this.taskService.updateTask(this.id, editing.id, data).subscribe({
        next: (res) => {
          this.savingTask.set(false);
          if (res.success) {
            const updated = this.normalizeTask({
              ...res.data,
              status: raw.status || res.data.status,
            });
            this.tasks.update((ts) => ts.map((t) => (t.id === editing.id ? updated : t)));
            this.toastService.success('Task updated');
            this.closeTaskModal();
          }
        },
        error: (err) => {
          this.savingTask.set(false);
          
          // Check for concurrency conflict
          if (err.status === 409 || err?.error?.message?.includes('modified by another user')) {
            this.toastService.error(
              'Conflict Detected',
              'This task was modified by another user. Please refresh and try again.'
            );
            // Automatically reload the project
            this.loadProject();
          } else {
            this.toastService.error('Error', err?.error?.message || 'Failed to update task');
          }
        },
      });
    } else {
      this.taskService.createTask(this.id, data).subscribe({
        next: (res) => {
          this.savingTask.set(false);
          if (res.success) {
            const created = this.normalizeTask({
              ...res.data,
              status: raw.status || res.data.status,
            });
            this.tasks.update((ts) => [...ts, created]);
            this.toastService.success('Task created!', created.title);
            this.closeTaskModal();
          }
        },
        error: (err) => {
          this.savingTask.set(false);
          this.toastService.error('Error', err?.error?.message || 'Failed to create task');
        },
      });
    }
  }

  enhanceDescription() {
    const desc = this.taskForm.get('description')?.value || '';
    const title = this.taskForm.get('title')?.value || '';
    if (!desc && !title) {
      this.toastService.warning('Nothing to enhance', 'Add a description first');
      return;
    }
    this.enhancing.set(true);
    this.aiService.enhanceDescription({ description: desc, context: title }).subscribe({
      next: (enhanced) => {
        this.enhancing.set(false);
        this.taskForm.patchValue({ description: enhanced });
        this.toastService.success('AI Enhanced!', 'Description has been improved');
      },
      error: () => {
        this.enhancing.set(false);
        this.toastService.error('AI Error', 'Could not enhance description');
      },
    });
  }

  moveTask(task: TaskItem, newStatus: TaskStatus | number) {
    const statusNum = typeof newStatus === 'number' ? newStatus : newStatus;
    this.taskService.updateStatus(this.id, task.id, statusNum).subscribe({
      next: (res) => {
        if (res.success) {
          this.tasks.update((ts) =>
            ts.map((t) => (t.id === task.id ? { ...t, status: statusNum as TaskStatus } : t)),
          );
        }
      },
      error: () => this.toastService.error('Error', 'Failed to update status'),
    });
  }

  quickChangeStatus(task: TaskItem, newStatus: TaskStatus | number) {
    const statusNum = typeof newStatus === 'number' ? newStatus : newStatus;
    this.taskService.updateStatus(this.id, task.id, statusNum).subscribe({
      next: (res) => {
        if (res.success) {
          this.tasks.update((ts) =>
            ts.map((t) => (t.id === task.id ? { ...t, status: statusNum as TaskStatus } : t)),
          );
          this.viewingTask.update((t) => (t ? { ...t, status: statusNum as TaskStatus } : t));
        }
      },
      error: () => this.toastService.error('Error', 'Failed to update status'),
    });
  }

  confirmDeleteTask(task: TaskItem) {
    this.deleteTaskTarget.set(task);
  }

  onDeleteTask() {
    const target = this.deleteTaskTarget();
    if (!target) return;
    this.savingTask.set(true);
    this.taskService.deleteTask(this.id, target.id).subscribe({
      next: (res) => {
        this.savingTask.set(false);
        if (res.success) {
          this.tasks.update((ts) => ts.filter((t) => t.id !== target.id));
          this.toastService.success('Task deleted');
        }
        this.deleteTaskTarget.set(null);
      },
      error: (err) => {
        this.savingTask.set(false);
        this.toastService.error('Error', err?.error?.message || 'Failed to delete task');
      },
    });
  }

  onAddComment() {
    const v = this.commentForm.value.content?.trim();
    if (!v) return;
    const task = this.viewingTask();
    if (!task) return;

    this.savingComment.set(true);
    this.taskService.addComment(this.id, task.id, { content: v }).subscribe({
      next: (res) => {
        this.savingComment.set(false);
        this.commentForm.reset();
        // Reload task detail to get fresh comments
        this.openTaskDetail(task);
        this.toastService.success('Comment added');
      },
      error: (err) => {
        this.savingComment.set(false);
        this.toastService.error('Error', err?.error?.message || 'Failed to add comment');
      },
    });
  }

  closeMembersModal() {
    this.showMembersModal.set(false);
    this.cancelAddMember();
  }

  selectUser(user: UserProfile) {
    this.selectedUser.set(user);
    this.searchControl.setValue(user.fullName || user.firstName + ' ' + user.lastName, {
      emitEvent: false,
    });
  }

  clearSelection() {
    this.selectedUser.set(null);
    this.searchControl.setValue('');
  }

  cancelAddMember() {
    this.isAddingMember.set(false);
    this.clearSelection();
    this.projectRoleControl.setValue(2);
  }

  submitAddMember() {
    const user = this.selectedUser();
    const role = this.projectRoleControl.value;
    if (!user || !role) return;

    this.savingMember.set(true);
    this.projectService
      .addMember(this.id, {
        userId: user.id,
        projectRole: role,
      })
      .subscribe({
        next: (res) => {
          this.savingMember.set(false);
          if (res.success) {
            this.loadProject();
            const roleName = role === 1 ? 'Manager' : 'Member';
            this.toastService.success(`Member added as ${roleName}!`);
            this.cancelAddMember();
          }
        },
        error: (err) => {
          this.savingMember.set(false);
          this.toastService.error('Error', err?.error?.message || 'Failed to add member');
        },
      });
  }

  getInitials(user: UserProfile): string {
    if (user.fullName) {
      const parts = user.fullName.split(' ');
      return parts.length > 1
        ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
        : parts[0].substring(0, 2).toUpperCase();
    }
    return ((user.firstName?.[0] || '') + (user.lastName?.[0] || '')).toUpperCase();
  }

  removeMember(userId: string) {
    // Find the member object
    const member = this.project()?.members?.find(m => m.userId === userId);
    if (!member) return;
    
    // Set target and show modal
    this.removeMemberTarget.set(member);
    this.showRemoveMemberModal.set(true);
  }

  confirmRemoveMember() {
    const member = this.removeMemberTarget();
    if (!member) return;

    this.removingMember.set(true);
    this.projectService.removeMember(this.id, member.userId).subscribe({
      next: (res) => {
        if (res.success) {
          this.removingMember.set(false);
          this.showRemoveMemberModal.set(false);
          this.removeMemberTarget.set(null);
          this.loadProject();
          this.toastService.success('Member removed successfully');
        }
      },
      error: (err) => {
        this.removingMember.set(false);
        this.toastService.error('Error', err?.error?.message || 'Failed to remove member');
      },
    });
  }

  cancelRemoveMember() {
    this.showRemoveMemberModal.set(false);
    this.removeMemberTarget.set(null);
  }

  isProjectAdmin(): boolean {
    const user = this.authService.currentUserValue;
    if (!user) return false;
    if (user.role === 'Admin') return true;
    const proj = this.project();
    if (
      user.role === 'ProjectManager' &&
      (proj?.createdByUserId === user.id || proj?.createdById === user.id)
    ) {
      return true;
    }
    return false;
  }

  currentUserInitials(): string {
    const u = this.authService.currentUserValue;
    if (!u) return '?';
    return `${u.firstName.charAt(0)}${u.lastName.charAt(0)}`.toUpperCase();
  }

  getStatusLabel(status: number | TaskStatus): string {
    const statusNum = typeof status === 'number' ? status : status;
    return TASK_STATUS_LABELS[statusNum] || TASK_STATUS_LABELS[0];
  }

  getStatusBadge(status?: number): string {
    const statusNum = status ?? 1;
    const map: Record<number, string> = {
      0: 'badge badge-secondary', // Planning
      1: 'badge badge-success', // Active
      2: 'badge badge-warning', // OnHold
      3: 'badge badge-primary', // Completed
      4: 'badge badge-secondary', // Cancelled
    };
    return map[statusNum] || 'badge badge-secondary';
  }

  getTaskStatusBadgeClass(status?: number | TaskStatus): string {
    const statusNum = typeof status === 'number' ? status : (status ?? 0);
    const map: Record<number, string> = {
      0: 'badge-secondary', // ToDo
      1: 'badge-info', // InProgress
      2: 'badge-warning', // InReview
      3: 'badge-success', // Completed
      4: 'badge-danger', // Cancelled
      5: 'badge-dark', // Blocked
      6: 'badge-warning', // OnHold
    };
    return map[statusNum] || 'badge-secondary';
  }

  getPriorityBadgeClass(priority?: number | Priority): string {
    const priorityNum = typeof priority === 'number' ? priority : (priority ?? 1);
    const map: Record<number, string> = {
      0: 'badge-success', // Low
      1: 'badge-info', // Medium
      2: 'badge-warning', // High
      3: 'badge-danger', // Critical
    };
    return map[priorityNum] || 'badge-info';
  }

  getPriorityLabel(priority?: number | Priority): string {
    const priorityNum = typeof priority === 'number' ? priority : (priority ?? 1);
    return PRIORITY_LABELS[priorityNum] || PRIORITY_LABELS[1];
  }

  isOverdue(dueDate: string): boolean {
    return new Date(dueDate) < new Date();
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }

  formatTime(dateStr: string): string {
    const d = new Date(dateStr);
    const now = new Date();
    const diff = now.getTime() - d.getTime();
    const m = Math.floor(diff / 60000);
    if (m < 1) return 'Just now';
    if (m < 60) return `${m}m ago`;
    const h = Math.floor(m / 60);
    if (h < 24) return `${h}h ago`;
    return `${Math.floor(h / 24)}d ago`;
  }

  getPrevStatus(current: number | TaskStatus): TaskStatus {
    const currentNum = typeof current === 'number' ? current : current;
    // Order: ToDo(1) → InProgress(2) → Completed(3) → Cancelled(4)
    if (currentNum > TaskStatus.ToDo && currentNum <= TaskStatus.Cancelled) {
      return (currentNum - 1) as TaskStatus;
    }
    return currentNum as TaskStatus;
  }

  getNextStatus(current: number | TaskStatus): TaskStatus {
    const currentNum = typeof current === 'number' ? current : current;
    // Order: ToDo(1) → InProgress(2) → Completed(3) → Cancelled(4)
    if (currentNum >= TaskStatus.ToDo && currentNum < TaskStatus.Cancelled) {
      return (currentNum + 1) as TaskStatus;
    }
    return currentNum as TaskStatus;
  }
}
