import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ProjectService } from '../../core/services/project.service';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/services/auth.service';
import { AiService } from '../../core/services/ai.service';
import {
  Project,
  CreateProjectRequest,
  UpdateProjectRequest,
  ProjectStatus,
  Priority,
  PROJECT_STATUS_LABELS,
  PRIORITY_LABELS,
  PagedResult,
} from '../../core/models/app.models';

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.css',
})
export class ProjectsComponent implements OnInit, OnDestroy {
  private projectService = inject(ProjectService);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);
  private aiService = inject(AiService);
  private fb = inject(FormBuilder);

  projects = signal<Project[]>([]);
  pagination = signal<PagedResult<Project> | null>(null);
  loading = signal(true);
  saving = signal(false);
  enhancing = signal(false);
  showModal = signal(false);
  editingProject = signal<Project | null>(null);
  deleteTarget = signal<Project | null>(null);
  openMenuId = signal<string | null>(null);
  searchTerm = signal('');
  statusFilter = signal('');
  priorityFilter = signal('');
  currentPage = signal(1);
  pageSize = signal(10);

  projectStatuses = [
    { value: ProjectStatus.Planning, label: 'Planning' },
    { value: ProjectStatus.Active, label: 'Active' },
    { value: ProjectStatus.OnHold, label: 'On Hold' },
    { value: ProjectStatus.Completed, label: 'Completed' },
    { value: ProjectStatus.Cancelled, label: 'Cancelled' },
  ];

  priorities = [
    { value: Priority.Low, label: 'Low' },
    { value: Priority.Medium, label: 'Medium' },
    { value: Priority.High, label: 'High' },
    { value: Priority.Critical, label: 'Critical' },
  ];

  projectForm!: FormGroup;

  dateValidator(group: FormGroup): { [key: string]: any } | null {
    const start = group.get('startDate')?.value;
    const end = group.get('endDate')?.value;

    if (start && end && new Date(start) > new Date(end)) {
      return { dateInvalid: true };
    }
    return null;
  }

  ngOnInit() {
    this.initializeForm();
    this.loadProjects();
    document.addEventListener('click', this.closeMenuOnOutsideClick.bind(this));
  }

  initializeForm() {
    this.projectForm = this.fb.group(
      {
        name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
        description: ['', [Validators.required, Validators.maxLength(500)]],
        status: [ProjectStatus.Active, Validators.required],
        priority: [Priority.Medium, Validators.required],
        startDate: [''],
        endDate: [''],
      },
      { validators: this.dateValidator },
    );
  }

  ngOnDestroy() {
    document.removeEventListener('click', this.closeMenuOnOutsideClick.bind(this));
    // Clean up form
    if (this.projectForm) {
      this.projectForm.reset();
    }
  }

  closeMenuOnOutsideClick(e: MouseEvent) {
    const target = e.target as HTMLElement;
    if (!target.closest('.project-actions')) {
      this.openMenuId.set(null);
    }
  }

  filteredProjects() {
    return this.projects();
  }

  private normalizeProject(p: any, fallbackData?: any): Project {
    const user = this.authService.currentUserValue;
    return {
      ...p,
      id: p.id,
      name: p.name,
      description: p.description || '',
      status: fallbackData?.status || p.status || ProjectStatus.Active,
      priority: fallbackData?.priority || p.priority || Priority.Medium,
      startDate: fallbackData?.startDate ?? p.startDate ?? null,
      endDate: fallbackData?.endDate ?? p.endDate ?? null,
      createdById: p.createdByUserId || p.createdById || user?.id || '',
      createdByName: p.createdByUserName || p.createdByName || user?.fullName || 'Manager',
      totalTasks: p.totalTasks ?? p.taskStats?.total ?? 0,
      completedTasks: p.completedTasks ?? p.taskStats?.completed ?? 0,
      memberCount: p.memberCount ?? p.members?.length ?? 1,
      members: p.members || [],
      taskStats: p.taskStats || {
        total: p.totalTasks ?? 0,
        completed: p.completedTasks ?? 0,
        todo: 0,
        inProgress: 0,
        cancelled: 0,
        completionPercentage:
          p.totalTasks && p.totalTasks > 0
            ? Math.round(((p.completedTasks || 0) / p.totalTasks) * 100)
            : 0,
      },
    };
  }

  loadProjects() {
    this.loading.set(true);
    const params: any = {
      search: this.searchTerm() || undefined,
      status: this.statusFilter() ? parseInt(this.statusFilter() as any) : undefined,
      priority: this.priorityFilter() ? parseInt(this.priorityFilter() as any) : undefined,
      page: this.currentPage(),
      pageSize: this.pageSize(),
    };

    this.projectService.getProjects(params).subscribe({
      next: (res) => {
        this.loading.set(false);
        if (res.success) {
          // Map server PagedResult (PascalCase) to frontend PagedResult (camelCase)
          const server = res.data as any;
          const serverItems = server.items ?? server.Items ?? [];
          const items = (serverItems || []).map((p: any) => this.normalizeProject(p));
          const pageNumber = server.pageNumber ?? server.Page ?? server.page ?? 1;
          const pageSize = server.pageSize ?? server.PageSize ?? server.pageSize ?? this.pageSize();
          const totalCount = server.totalCount ?? server.TotalCount ?? 0;
          const totalPages = server.totalPages ?? server.TotalPages ?? Math.ceil(totalCount / (pageSize || 1));
          const hasNextPage = server.hasNextPage ?? server.HasNextPage ?? pageNumber < totalPages;
          const hasPreviousPage = server.hasPreviousPage ?? server.HasPreviousPage ?? pageNumber > 1;

          const mapped: PagedResult<Project> = {
            items: items,
            totalCount: totalCount,
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalPages: totalPages,
            hasPreviousPage: hasPreviousPage,
            hasNextPage: hasNextPage,
          };

          this.projects.set(items);
          this.pagination.set(mapped);
          this.currentPage.set(mapped.pageNumber);
          this.pageSize.set(mapped.pageSize);
        }
      },
      error: () => {
        this.loading.set(false);
        this.toastService.error('Error', 'Failed to load projects');
      },
    });
  }

  openCreateModal() {
    this.editingProject.set(null);
    this.projectForm.reset({
      name: '',
      description: '',
      status: ProjectStatus.Active,
      priority: Priority.Medium,
      startDate: '',
      endDate: '',
    });
    this.showModal.set(true);
  }

  openEditModal(project: Project) {
    this.openMenuId.set(null);
    this.editingProject.set(project);
    this.projectForm.patchValue({
      name: project.name,
      description: project.description,
      status: project.status || ProjectStatus.Active,
      priority: project.priority || Priority.Medium,
      startDate: project.startDate?.split('T')[0] || '',
      endDate: project.endDate?.split('T')[0] || '',
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
    this.editingProject.set(null);
  }

  isInvalid(field: string): boolean {
    const c = this.projectForm.get(field);
    return !!(c?.invalid && (c?.touched || c?.dirty));
  }

  getErrorMessage(field: string): string {
    const control = this.projectForm.get(field);
    if (!control || !control.errors) return '';

    if (control.errors['required']) return `${field} is required`;
    if (control.errors['minLength'])
      return `Minimum ${control.errors['minLength'].requiredLength} characters required`;
    if (control.errors['maxLength'])
      return `Maximum ${control.errors['maxLength'].requiredLength} characters allowed`;

    return 'Invalid value';
  }

  onSubmit() {
    // Check if form exists
    if (!this.projectForm) {
      console.error('Form not initialized!');
      this.toastService.error('Error', 'Form not initialized properly');
      return;
    }

    // Mark all fields as touched to show validation errors
    Object.keys(this.projectForm.controls).forEach((key) => {
      const control = this.projectForm.get(key);
      control?.markAsTouched();
      control?.markAsDirty();
    });

    if (this.projectForm.invalid) {
      console.log('Form invalid:', this.projectForm.errors);
      console.log('Form controls:', this.projectForm.controls);
      this.toastService.error('Validation Error', 'Please fill in all required fields correctly');
      return;
    }

    this.saving.set(true);
    const data = this.projectForm.value;

    console.log('Submitting project data:', data);

    const editing = this.editingProject();
    if (editing) {
      this.projectService.updateProject(editing.id, data).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            const updated = this.normalizeProject(res.data, data);
            this.projects.update((ps) => ps.map((p) => (p.id === editing.id ? updated : p)));
            this.toastService.success('Project updated', updated.name);
            this.closeModal();
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.toastService.error('Error', err?.error?.message || 'Failed to update project');
        },
      });
    } else {
      this.projectService.createProject(data).subscribe({
        next: (res) => {
          this.saving.set(false);
          if (res.success) {
            const created = this.normalizeProject(res.data, data);
            this.projects.update((ps) => [created, ...ps]);
            this.toastService.success('Project created!', created.name);
            this.closeModal();
          }
        },
        error: (err) => {
          this.saving.set(false);
          this.toastService.error('Error', err?.error?.message || 'Failed to create project');
        },
      });
    }
  }

  enhanceDescription() {
    const desc = this.projectForm.get('description')?.value || '';
    const name = this.projectForm.get('name')?.value || '';
    if (!desc && !name) {
      this.toastService.warning('Nothing to enhance', 'Add a description first');
      return;
    }
    this.enhancing.set(true);
    this.aiService.enhanceDescription({ description: desc, context: name }).subscribe({
      next: (enhanced) => {
        this.enhancing.set(false);
        this.projectForm.patchValue({ description: enhanced });
        this.toastService.success('AI Enhanced!', 'Description has been improved');
      },
      error: () => {
        this.enhancing.set(false);
        this.toastService.error('AI Error', 'Could not enhance description');
      },
    });
  }

  confirmDelete(project: Project) {
    this.openMenuId.set(null);
    this.deleteTarget.set(project);
  }

  onDelete() {
    const target = this.deleteTarget();
    if (!target) return;
    this.saving.set(true);
    this.projectService.deleteProject(target.id).subscribe({
      next: (res) => {
        this.saving.set(false);
        if (res.success) {
          this.projects.update((ps) => ps.filter((p) => p.id !== target.id));
          this.toastService.success('Project deleted', target.name);
        }
        this.deleteTarget.set(null);
      },
      error: (err) => {
        this.saving.set(false);
        this.toastService.error('Error', err?.error?.message || 'Failed to delete project');
      },
    });
  }

  toggleMenu(id: string) {
    this.openMenuId.update((curr) => (curr === id ? null : id));
  }

  canManage(project: Project): boolean {
    const user = this.authService.currentUserValue;
    return (
      user?.role === 'Admin' ||
      project.createdById === user?.id ||
      (project as any).createdByUserId === user?.id
    );
  }

  getCompletionPercentage(project: Project): number {
    const total = project.totalTasks ?? project.taskStats?.total ?? 0;
    const completed = project.completedTasks ?? project.taskStats?.completed ?? 0;
    if (total > 0) {
      return Math.round((completed / total) * 100);
    }
    return project.taskStats?.completionPercentage ?? 0;
  }

  onSearch(event: Event) {
    this.searchTerm.set((event.target as HTMLInputElement).value);
    this.currentPage.set(1);
    this.loadProjects();
  }

  onStatusFilter(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.statusFilter.set(value || '');
    this.currentPage.set(1);
    this.loadProjects();
  }

  onPriorityFilter(event: Event) {
    const value = (event.target as HTMLSelectElement).value;
    this.priorityFilter.set(value || '');
    this.currentPage.set(1);
    this.loadProjects();
  }

  changePage(page: number) {
    if (page < 1 || (this.pagination()?.totalPages ?? 0) < page) {
      return;
    }
    this.currentPage.set(page);
    this.loadProjects();
  }

  getStatusLabel(status?: number | string): string {
    if (status === undefined || status === null) return PROJECT_STATUS_LABELS[1]; // Default: Active
    const statusNum = typeof status === 'string' ? parseInt(status) : status;
    return PROJECT_STATUS_LABELS[statusNum] || PROJECT_STATUS_LABELS[1];
  }

  getStatusBadgeClass(status?: number | string): string {
    const statusNum = typeof status === 'string' ? parseInt(status) : (status ?? 1);
    const map: Record<number, string> = {
      0: 'badge badge-secondary', // Planning
      1: 'badge badge-success', // Active
      2: 'badge badge-warning', // OnHold
      3: 'badge badge-primary', // Completed
      4: 'badge badge-secondary', // Cancelled
    };
    return map[statusNum] || 'badge badge-secondary';
  }

  getPriorityLabel(priority?: number | string): string {
    if (priority === undefined || priority === null) return PRIORITY_LABELS[1]; // Default: Medium
    const priorityNum = typeof priority === 'string' ? parseInt(priority) : priority;
    return PRIORITY_LABELS[priorityNum] || PRIORITY_LABELS[1];
  }

  getPriorityBadgeClass(priority?: number | string): string {
    const priorityNum = typeof priority === 'string' ? parseInt(priority) : (priority ?? 1);
    const map: Record<number, string> = {
      0: 'badge badge-success', // Low
      1: 'badge badge-info', // Medium
      2: 'badge badge-warning', // High
      3: 'badge badge-danger', // Critical
    };
    return map[priorityNum] || 'badge badge-info';
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}
