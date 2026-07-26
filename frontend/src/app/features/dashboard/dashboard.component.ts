import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { DashboardStats, ActivityItem, ProjectProgressItem } from '../../core/models/app.models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private dashService = inject(DashboardService);
  private authService = inject(AuthService);

  loading = signal(true);
  stats = signal<DashboardStats | null>(null);

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.loading.set(true);
    this.dashService.getStats().subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) this.stats.set(res.data);
        else this.stats.set(this.mockStats());
      },
      error: () => {
        this.loading.set(false);
        this.stats.set(this.mockStats());
      }
    });
  }

  greeting(): string {
    const h = new Date().getHours();
    if (h < 12) return 'morning';
    if (h < 17) return 'afternoon';
    return 'evening';
  }

  firstName(): string {
    return this.authService.currentUserValue?.firstName || 'there';
  }

  completionRate(): number {
    const s = this.stats();
    if (!s || s.totalTasks === 0) return 0;
    return Math.round((s.completedTasks / s.totalTasks) * 100);
  }

  statusItems() {
    const s = this.stats();
    if (!s) return [];
    const total = s.totalTasks || 1;
    return [
      { key: 'ToDo', label: 'To Do', count: s.tasksByStatus['ToDo'] ?? 0, color: '#6b7280', pct: ((s.tasksByStatus['ToDo'] ?? 0) / total) * 100 },
      { key: 'InProgress', label: 'In Progress', count: s.tasksByStatus['InProgress'] ?? 0, color: '#3b82f6', pct: ((s.tasksByStatus['InProgress'] ?? 0) / total) * 100 },
      { key: 'Completed', label: 'Completed', count: s.tasksByStatus['Completed'] ?? 0, color: '#10b981', pct: ((s.tasksByStatus['Completed'] ?? 0) / total) * 100 },
      { key: 'Cancelled', label: 'Cancelled', count: s.tasksByStatus['Cancelled'] ?? 0, color: '#ef4444', pct: ((s.tasksByStatus['Cancelled'] ?? 0) / total) * 100 },
    ];
  }

  priorityItems() {
    const s = this.stats();
    if (!s) return [];
    return [
      { key: 'Low', label: 'Low', count: s.tasksByPriority['Low'] ?? 0, color: '#10b981' },
      { key: 'Medium', label: 'Medium', count: s.tasksByPriority['Medium'] ?? 0, color: '#f59e0b' },
      { key: 'High', label: 'High', count: s.tasksByPriority['High'] ?? 0, color: '#ef4444' },
      { key: 'Critical', label: 'Critical', count: s.tasksByPriority['Critical'] ?? 0, color: '#dc2626' },
    ];
  }

  formatTime(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutes = Math.floor(diff / 60000);
    if (minutes < 1) return 'Just now';
    if (minutes < 60) return `${minutes}m ago`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h ago`;
    const days = Math.floor(hours / 24);
    return `${days}d ago`;
  }

  private mockStats(): DashboardStats {
    return {
      totalProjects: 0,
      totalTasks: 0,
      myTasks: 0,
      completedTasks: 0,
      overdueTasks: 0,
      upcomingTasks: 0,
      tasksByStatus: { ToDo: 0, InProgress: 0, Completed: 0, Cancelled: 0 },
      tasksByPriority: { Low: 0, Medium: 0, High: 0, Critical: 0 },
      recentActivity: [],
      projectProgress: []
    };
  }
}
