# AI Prompts Used in Development

**Project**: Smart Task Management System  
**Developer**: Mahfuz Ahmed  
**Date**: July 2026  
**AI Assistant**: Kiro (Claude Sonnet 4.5)

---

## 📋 Table of Contents

1. [Project Setup & Architecture](#project-setup--architecture)
2. [Backend Development](#backend-development)
3. [Frontend Development](#frontend-development)
4. [Bug Fixes & Troubleshooting](#bug-fixes--troubleshooting)
5. [AI Integration](#ai-integration)
6. [Documentation](#documentation)

---

## 🏗 Project Setup & Architecture

### Initial Project Structure

**Prompt 1: Project Architecture Design**
```
I need to build a Smart Task Management System using .NET 10 and Angular 22. 
Requirements:
- N-Layer Architecture (API, Application, Infrastructure, Domain)
- Clean separation of concerns
- Repository pattern with Unit of Work
- JWT authentication
- Role-based authorization (Admin, ProjectManager, TeamMember)
- Project membership with roles (Manager, Member)
- Soft delete on all entities
- Entity Framework Core with SQL Server

Can you help me design the architecture and create the initial project structure?
```

**Prompt 2: Database Schema Design**
```
Design a database schema for a task management system with:
- Users (with roles)
- Projects (with status, priority)
- Tasks (with status, priority, due dates)
- Project Members (with project-specific roles)
- Comments on tasks
- Activity logs for task changes
- Soft delete support
- Audit trail (CreatedAt, CreatedBy, DeletedAt, DeletedBy)
- Optimistic concurrency using RowVersion

Provide EF Core entity classes with proper relationships.
```

---

## 🎯 Backend Development

### Authentication & Authorization

**Prompt 3: JWT Authentication Implementation**
```
Implement JWT authentication for ASP.NET Core 10 with:
- Login endpoint returning access token (15 min expiry) and refresh token (7 days)
- Register endpoint with password hashing (BCrypt)
- Refresh token endpoint with token rotation
- Logout endpoint to revoke refresh tokens
- Store refresh tokens in database
- HTTP-only cookies for refresh tokens
```

**Prompt 4: Hybrid RBAC Authorization**
```
Implement a hybrid RBAC system where:
1. System Role (from Users table): Admin, ProjectManager, TeamMember
2. Project Role (from ProjectMembers table): Manager, Member

Authorization logic:
- Admin: Full access to everything
- ProjectManager with Manager role in project: Full project access
- ProjectManager with Member role in project: Limited access (only assigned tasks)
- TeamMember: Can only update their assigned tasks

Create an AuthorizationHelper class with methods like:
- EnsureCanCreateTask
- EnsureCanUpdateTask
- EnsureCanDeleteTask
```

### API Controllers

**Prompt 5: Projects Controller**
```
Create a ProjectsController with these endpoints:
- POST /api/projects - Create project (Admin only)
- GET /api/projects - List projects with pagination, search, filtering
- GET /api/projects/{id} - Get project details with members and tasks
- PUT /api/projects/{id} - Update project (Admin only)
- DELETE /api/projects/{id} - Soft delete project (Admin only)
- POST /api/projects/{id}/members - Add member to project
- DELETE /api/projects/{id}/members/{userId} - Remove member

Include proper authorization, validation, and error handling.
```

**Prompt 6: Tasks Controller**
```
Create a TasksController with hybrid RBAC:
- POST /api/projects/{projectId}/tasks - Create task (Admin or PM with Manager role)
- GET /api/projects/{projectId}/tasks - List tasks with pagination
- GET /api/tasks/{id} - Get task details
- PUT /api/tasks/{id} - Update task (role-based permissions)
- DELETE /api/tasks/{id} - Soft delete (Admin or PM with Manager role)
- POST /api/tasks/{id}/comments - Add comment to task
- GET /api/tasks/{id}/activity-logs - Get task activity history

Automatically log all task changes to TaskActivityLog table.
```

### Services & Business Logic

**Prompt 7: Service Layer Implementation**
```
Implement service classes for:
- AuthService: Login, Register, RefreshToken, Logout
- ProjectService: CRUD with authorization checks
- TaskService: CRUD with authorization and activity logging
- DashboardService: Statistics with caching

Use repository pattern through UnitOfWork.
Apply authorization before any data modification.
```

**Prompt 8: Repository Pattern**
```
Implement:
1. Generic BaseRepository<TEntity, TKey> with:
   - GetByIdAsync, GetAllAsync, AddAsync, Update, Delete
   - Soft delete support (set IsDeleted = true)
   
2. Specific repositories:
   - ProjectRepository with GetByIdWithDetailsAsync, GetPagedAsync
   - TaskRepository with GetByProjectIdAsync, GetPagedAsync
   - UserRepository with GetByEmailAsync

3. UnitOfWork pattern to manage transactions
```

---

## 💻 Frontend Development

### Angular Setup

**Prompt 9: Angular 22 Standalone Setup**
```
Set up an Angular 22 project with:
- Standalone components (no NgModules)
- Signal-based state management
- Functional routing with lazy loading
- Functional guards (auth.guard, role.guard)
- Functional HTTP interceptor for JWT tokens
- Angular Material for UI components
- Bootstrap for additional styling
```

**Prompt 10: Authentication Service**
```
Create an AuthService with:
- login(email, password): Observable<LoginResponse>
- register(userData): Observable<void>
- logout(): Observable<void>
- refreshToken(): Observable<TokenResponse>
- currentUser signal with user data
- isAuthenticated signal
- Auto token refresh on 401 responses
- Store tokens in localStorage
```

### Feature Modules

**Prompt 11: Projects Feature**
```
Create a projects feature with standalone components:
- ProjectsComponent: List all projects with search, filter, pagination
- ProjectDetailComponent: Show project details, members, tasks
- ProjectFormComponent: Create/Edit project form with reactive forms
- AddMemberModalComponent: Add members to project

Use Angular Material tables, dialogs, and form controls.
Implement proper error handling and loading states.
```

**Prompt 12: Tasks Feature**
```
Create tasks feature with:
- Task list view with status columns (Kanban board style)
- Task detail view with comments section
- Task form with:
  * Title, Description (required)
  * Status dropdown (ToDo, InProgress, Completed, Cancelled)
  * Priority dropdown (Low, Medium, High, Critical)
  * Due date picker
  * Assignee dropdown (project members only)
  * AI enhance button for description

Use signals for reactive state management.
```

### Shared Components

**Prompt 13: Reusable Components**
```
Create shared standalone components:
- ConfirmationModalComponent: Generic confirmation dialog
- ToastContainerComponent: Toast notifications with types (success, error, warning)
- LoadingSpinnerComponent: Loading indicator
- ErrorMessageComponent: Display validation errors

Make them reusable across the application.
```

---

## 🐛 Bug Fixes & Troubleshooting

**Prompt 14: Role Name Mismatch**
```
I'm getting authorization errors. The [Authorize(Roles = "ProjectManager")] 
attribute is not working. The JWT token contains:
"http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "ProjectManager"

But ASP.NET Core is looking for a claim named "role" without the namespace.
How do I fix this role claim mapping issue?
```

**Prompt 15: Optimistic Concurrency Conflicts**
```
When two users try to update the same task, I get DbUpdateConcurrencyException.
How should I handle this properly?
- Detect conflict using RowVersion
- Return appropriate error to client
- Let user resolve conflict
```

**Prompt 16: Soft Delete Query Filtering**
```
I need to automatically filter out soft-deleted records in all queries.
Implement EF Core global query filters for:
- Users where IsDeleted == false
- Projects where IsDeleted == false
- Tasks where IsDeleted == false

Also handle cascading soft deletes (when project is deleted, soft delete all tasks).
```

---

## 🤖 AI Integration

**Prompt 17: GitHub Models API Integration**
```
Integrate GitHub Models API for task description enhancement:
- Use GPT-4o-mini model
- Endpoint: https://models.github.ai/inference/chat/completions
- Model ID format: openai/gpt-4o-mini
- Implement with HttpClient
- Add rate limiting (10 requests/minute)
- Provide fallback when API fails
- Validate input (10-1000 characters)

Create a prompt that improves task descriptions by:
1. Correcting grammar
2. Making it more professional
3. Expanding vague descriptions
4. Using actionable language
```

**Prompt 18: AI Service Error Handling**
```
The GitHub Models API is returning "No such host is known" error.
The endpoint models.inference.ai.azure.com is deprecated.
Help me:
1. Find the correct new endpoint
2. Update the code
3. Fix the model ID format
4. Test the connection
```

---

**Prompt 19: Architecture Documentation**
```
Create ARCHITECTURE_DECISIONS.md explaining:
- Why N-Layer Architecture was chosen
- Why NOT CQRS or Clean Architecture
- Why Hybrid RBAC over traditional RBAC
- Technology choices (why .NET 10, Angular 22, SQL Server)
- Repository Pattern rationale
- Performance optimizations
- Security decisions
- Trade-offs and future improvements
```

**Prompt 20: API Documentation**
```
Generate Swagger/OpenAPI documentation for all endpoints including:
- Request/response schemas
- Authentication requirements
- Authorization roles needed
- Example requests and responses
- Error codes and messages
- Rate limiting information
```

---

## 🔧 Performance & Optimization

**Prompt 21: Database Performance**
```
Optimize database queries:
1. Add strategic indexes on foreign keys and frequently queried columns
2. Use AsNoTracking() for read-only queries
3. Implement pagination on all list endpoints
4. Use Include() for eager loading related entities
5. Cache dashboard statistics for 5 minutes
```

**Prompt 22: Frontend Performance**
```
Optimize Angular application:
1. Implement lazy loading for feature modules
2. Use OnPush change detection strategy
3. Use signals instead of BehaviorSubject where possible
4. Minimize bundle size with tree-shaking
5. Use Angular Material's virtual scrolling for large lists
```

---

## 📊 Statistics

**Total Development Sessions**: 15+  
**Total Prompts Used**: 50+  
**Lines of Code Generated**: 10,000+  
**Time Saved**: ~80 hours  

---

## 📝 Notes

- All prompts were iterative - many required 2-3 follow-ups for refinement
- Code was reviewed and tested before integration
- Architecture decisions were made by developer, AI provided implementation
- Documentation was generated, then customized for project specifics
- Security practices were validated against industry standards

---

