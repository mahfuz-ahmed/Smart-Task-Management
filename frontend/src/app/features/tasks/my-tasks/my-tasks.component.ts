import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';
import {
  TaskItem,
  TaskStatus,
  Priority,
  TASK_STATUS_LABELS,
  PRIORITY_LABELS,
} from '../../../core/models/app.models';

@Component({
  selector: 'app-my-tasks',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-tasks.component.html',
  styleUrl: './my-tasks.component.css',
})
export class MyTasksComponent implements OnInit {
  private taskService = inject(TaskService);

  tasks = signal<TaskItem[]>([]);
  loading = signal(true);
  searchTerm = signal('');
  statusFilter = signal('');
  priorityFilter = signal('');

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

  // Expose enums for template
  TaskStatus = TaskStatus;
  Priority = Priority;

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.loading.set(true);
    this.taskService.getMyTasks({ pageSize: 100 }).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          const items = (res.data.items || []).map((t) => this.normalizeTask(t));
          this.tasks.set(items);
        }
      },
      error: (err) => {
        this.loading.set(false);
        console.error('Failed to load my tasks:', err);
        this.tasks.set([]);
      },
    });
  }

  private normalizeTask(t: any): TaskItem {
    let statusNum: number;
    if (typeof t.status === 'number') {
      statusNum = t.status;
    } else if (typeof t.status === 'string') {
      const parsedStatus = parseInt(t.status, 10);
      statusNum = !isNaN(parsedStatus)
        ? parsedStatus
        : (TaskStatus[t.status as keyof typeof TaskStatus] ?? TaskStatus.ToDo);
    } else if (typeof t.statusName === 'string') {
      statusNum = TaskStatus[t.statusName as keyof typeof TaskStatus] ?? TaskStatus.ToDo;
    } else {
      statusNum = TaskStatus.ToDo;
    }

    let priorityNum: number;
    if (typeof t.priority === 'number') {
      priorityNum = t.priority;
    } else if (typeof t.priority === 'string') {
      const parsedPriority = parseInt(t.priority, 10);
      priorityNum = !isNaN(parsedPriority)
        ? parsedPriority
        : (Priority[t.priority as keyof typeof Priority] ?? Priority.Medium);
    } else if (typeof t.priorityName === 'string') {
      priorityNum = Priority[t.priorityName as keyof typeof Priority] ?? Priority.Medium;
    } else {
      priorityNum = Priority.Medium;
    }

    return {
      ...t,
      id: t.id,
      title: t.title,
      description: t.description || '',
      status: statusNum as TaskStatus,
      priority: priorityNum as Priority,
      dueDate: t.dueDate || null,
      projectId: t.projectId || '',
      projectName: t.projectName || '',
      assignedToId: t.assignedToUserId || t.assignedToId || null,
      assignedToName: t.assignedToUserName || t.assignedToName || null,
      createdById: t.createdById || '',
      createdByName: t.createdByName || '',
      createdAt: t.createdAtUtc || t.createdAt || new Date().toISOString(),
      updatedAt: t.lastModifiedAtUtc || t.updatedAt || new Date().toISOString(),
      estimatedHours: t.estimatedHours || null,
      tags: t.tags || [],
      comments: t.comments || [],
      activityLogs: t.activityLogs || [],
      attachments: t.attachments || [],
    };
  }

  filteredTasks() {
    return this.tasks().filter((t) => {
      const matchSearch =
        !this.searchTerm() || t.title.toLowerCase().includes(this.searchTerm().toLowerCase());
      const matchStatus = !this.statusFilter() || t.status === parseInt(this.statusFilter());
      const matchPriority =
        !this.priorityFilter() || t.priority === parseInt(this.priorityFilter());
      return matchSearch && matchStatus && matchPriority;
    });
  }

  taskStats() {
    const all = this.tasks();
    return [
      { status: '', label: 'All', count: all.length, color: '#6366f1' },
      {
        status: TaskStatus.ToDo.toString(),
        label: 'To Do',
        count: all.filter((t) => t.status === TaskStatus.ToDo).length,
        color: '#6b7280',
      },
      {
        status: TaskStatus.InProgress.toString(),
        label: 'In Progress',
        count: all.filter((t) => t.status === TaskStatus.InProgress).length,
        color: '#3b82f6',
      },
      {
        status: TaskStatus.Completed.toString(),
        label: 'Completed',
        count: all.filter((t) => t.status === TaskStatus.Completed).length,
        color: '#10b981',
      },
      {
        status: TaskStatus.Cancelled.toString(),
        label: 'Cancelled',
        count: all.filter((t) => t.status === TaskStatus.Cancelled).length,
        color: '#ef4444',
      },
    ];
  }

  setStatusFilter(status: number | string) {
    this.statusFilter.set(typeof status === 'number' ? status.toString() : status);
  }

  onSearch(e: Event) {
    this.searchTerm.set((e.target as HTMLInputElement).value);
  }
  onStatusFilter(e: Event) {
    this.statusFilter.set((e.target as HTMLSelectElement).value);
  }
  onPriorityFilter(e: Event) {
    this.priorityFilter.set((e.target as HTMLSelectElement).value);
  }

  getStatusLabel(status: number | TaskStatus): string {
    const statusNum = typeof status === 'number' ? status : status;
    return TASK_STATUS_LABELS[statusNum] || TASK_STATUS_LABELS[0];
  }

  getPriorityLabel(priority: number | Priority): string {
    const priorityNum = typeof priority === 'number' ? priority : priority;
    return PRIORITY_LABELS[priorityNum] || PRIORITY_LABELS[1];
  }

  getTaskStatusBadgeClass(status: number | TaskStatus): string {
    const statusNum = typeof status === 'number' ? status : status;
    const map: Record<number, string> = {
      1: 'badge-secondary', // ToDo
      2: 'badge-info', // InProgress
      3: 'badge-success', // Completed
      4: 'badge-danger', // Cancelled
      5: 'badge-warning', // InReview
      6: 'badge-dark', // Blocked
      7: 'badge-warning', // OnHold
    };
    return map[statusNum] || 'badge-secondary';
  }

  getPriorityBadgeClass(priority: number | Priority): string {
    const priorityNum = typeof priority === 'number' ? priority : priority;
    const map: Record<number, string> = {
      0: 'badge-success', // Low
      1: 'badge-info', // Medium
      2: 'badge-warning', // High
      3: 'badge-danger', // Critical
    };
    return map[priorityNum] || 'badge-info';
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

  navigateToTask(task: TaskItem) {
    // Navigate to project detail with task highlighted
    window.location.href = `/projects/${task.projectId}`;
  }
}
