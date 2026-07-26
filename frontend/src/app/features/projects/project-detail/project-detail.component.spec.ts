import { TestBed } from '@angular/core/testing';
import { ProjectDetailComponent } from './project-detail.component';
import { ProjectService } from '../../../core/services/project.service';
import { TaskService } from '../../../core/services/task.service';
import { AiService } from '../../../core/services/ai.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserService } from '../../../core/services/user.service';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { Priority, TaskStatus } from '../../../core/models/app.models';

describe('ProjectDetailComponent', () => {
  let mockProjectService: any;
  let mockTaskService: any;
  let mockAiService: any;
  let mockToastService: any;
  let mockAuthService: any;
  let mockDialog: any;
  let mockUserService: any;

  beforeEach(async () => {
    mockProjectService = {
      getProject: vi
        .fn()
        .mockReturnValue(
          of({
            success: true,
            data: {
              id: 'proj1',
              name: 'Proj 1',
              createdByUserId: 'pm1',
              createdById: 'pm1',
              members: [],
            },
          }),
        ),
      addMember: vi.fn().mockReturnValue(of({ success: true })),
      removeMember: vi.fn().mockReturnValue(of({ success: true })),
    };
    mockTaskService = {
      getTasks: vi.fn().mockReturnValue(of({ success: true, data: { items: [], totalCount: 0 } })),
    };
    mockAiService = {
      enhanceDescription: vi.fn(),
    };
    mockToastService = {
      success: vi.fn(),
      error: vi.fn(),
      warning: vi.fn(),
    };
    mockAuthService = {
      currentUserValue: { id: 'user1', firstName: 'John', lastName: 'Doe', role: 'TeamMember' },
    };
    mockDialog = {
      open: vi.fn(),
    };
    mockUserService = {
      search: vi.fn().mockReturnValue(of({ success: true, data: [] })),
    };

    await TestBed.configureTestingModule({
      imports: [ProjectDetailComponent, NoopAnimationsModule],
      providers: [
        { provide: ProjectService, useValue: mockProjectService },
        { provide: TaskService, useValue: mockTaskService },
        { provide: AiService, useValue: mockAiService },
        { provide: ToastService, useValue: mockToastService },
        { provide: AuthService, useValue: mockAuthService },
        { provide: MatDialog, useValue: mockDialog },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => 'proj1' } } } },
        { provide: UserService, useValue: mockUserService },
        provideRouter([]),
      ],
    })
      .overrideProvider(MatDialog, { useValue: mockDialog })
      .compileComponents();
  });

  it('should create and load project data', () => {
    const fixture = TestBed.createComponent(ProjectDetailComponent);
    fixture.componentInstance.id = 'proj1';
    fixture.detectChanges();

    expect(fixture.componentInstance).toBeTruthy();
    expect(mockProjectService.getProject).toHaveBeenCalledWith('proj1');
    expect(mockTaskService.getTasks).toHaveBeenCalledWith('proj1', { pageSize: 100 });
  });

  describe('isProjectAdmin permissions', () => {
    it('should return true if user is Admin', () => {
      mockAuthService.currentUserValue = { id: 'admin1', role: 'Admin' };
      const fixture = TestBed.createComponent(ProjectDetailComponent);
      fixture.componentInstance.id = 'proj1';
      fixture.detectChanges();

      expect(fixture.componentInstance.isProjectAdmin()).toBe(true);
    });

    it('should return true if user is ProjectManager and owns the project (createdByUserId match)', () => {
      mockAuthService.currentUserValue = { id: 'pm1', role: 'ProjectManager' };
      const fixture = TestBed.createComponent(ProjectDetailComponent);
      fixture.componentInstance.id = 'proj1';
      fixture.detectChanges();

      expect(fixture.componentInstance.isProjectAdmin()).toBe(true);
    });

    it('should return false if user is ProjectManager but does not own the project', () => {
      mockAuthService.currentUserValue = { id: 'pm2', role: 'ProjectManager' };
      const fixture = TestBed.createComponent(ProjectDetailComponent);
      fixture.componentInstance.id = 'proj1';
      fixture.detectChanges();

      expect(fixture.componentInstance.isProjectAdmin()).toBe(false);
    });

    it('should return false if user is TeamMember', () => {
      mockAuthService.currentUserValue = { id: 'pm1', role: 'TeamMember' };
      const fixture = TestBed.createComponent(ProjectDetailComponent);
      fixture.componentInstance.id = 'proj1';
      fixture.detectChanges();

      expect(fixture.componentInstance.isProjectAdmin()).toBe(false);
    });
  });

  describe('Dialog integrations', () => {
    it('should call addMember when submitAddMember is executed with valid selection', () => {
      const fixture = TestBed.createComponent(ProjectDetailComponent);
      fixture.componentInstance.id = 'proj1';
      fixture.detectChanges();

      fixture.componentInstance.selectedUser.set({
        id: 'new-user-id',
        firstName: 'A',
        lastName: 'B',
        role: 'Member',
        email: 'a@b.com',
        fullName: 'A B',
      });
      fixture.componentInstance.projectRoleControl.setValue(2);

      fixture.componentInstance.submitAddMember();

      expect(mockProjectService.addMember).toHaveBeenCalledWith('proj1', {
        userId: 'new-user-id',
        projectRole: 2,
      });
      expect(mockToastService.success).toHaveBeenCalled();
    });

    it('should include current status when editing a task', () => {
      const fixture = TestBed.createComponent(ProjectDetailComponent);
      fixture.componentInstance.id = 'proj1';
      fixture.detectChanges();

      mockTaskService.updateTask = vi
        .fn()
        .mockReturnValue(
          of({
            success: true,
            data: {
              id: 'task-1',
              title: 'Existing',
              description: 'Desc',
              status: TaskStatus.InProgress,
              priority: Priority.Medium,
              dueDate: null,
              createdAt: '',
              updatedAt: '',
            },
          }),
        );

      fixture.componentInstance.editingTask.set({
        id: 'task-1',
        title: 'Existing',
        description: 'Desc',
        status: TaskStatus.ToDo,
        priority: Priority.Medium,
        dueDate: null,
        projectId: 'proj1',
        projectName: 'Proj 1',
        assignedToUserId: null,
        assignedToName: null,
        createdById: 'u1',
        createdByName: 'User',
        createdAt: '',
        updatedAt: '',
        estimatedHours: null,
        tags: [],
        comments: [],
        activityLogs: [],
        attachments: [],
      });
      fixture.componentInstance.taskForm.patchValue({
        title: 'Existing',
        description: 'Desc',
        priority: Priority.Medium,
        status: TaskStatus.InProgress,
        dueDate: '',
        estimatedHours: null,
        assignedToUserId: '',
      });

      fixture.componentInstance.onTaskSubmit();

      expect(mockTaskService.updateTask).toHaveBeenCalled();
      const payload = mockTaskService.updateTask.mock.calls[0][2];
      expect(payload).toMatchObject({
        title: 'Existing',
        description: 'Desc',
        priority: 2,
        status: TaskStatus.InProgress,
      });
    });

    it('should open ConfirmDialogComponent and call removeMember when confirmed', () => {
      const fixture = TestBed.createComponent(ProjectDetailComponent);
      fixture.componentInstance.id = 'proj1';
      fixture.detectChanges();

      mockDialog.open.mockReturnValue({
        afterClosed: () => of(true),
      });

      fixture.componentInstance.removeMember('remove-user-id');

      expect(mockDialog.open).toHaveBeenCalled();
      expect(mockProjectService.removeMember).toHaveBeenCalledWith('proj1', 'remove-user-id');
      expect(mockToastService.success).toHaveBeenCalledWith('Member removed');
    });

    it('should not call removeMember when cancel is clicked in ConfirmDialogComponent', () => {
      const fixture = TestBed.createComponent(ProjectDetailComponent);
      fixture.componentInstance.id = 'proj1';
      fixture.detectChanges();

      mockDialog.open.mockReturnValue({
        afterClosed: () => of(false),
      });

      fixture.componentInstance.removeMember('remove-user-id');

      expect(mockDialog.open).toHaveBeenCalled();
      expect(mockProjectService.removeMember).not.toHaveBeenCalled();
    });
  });
});
