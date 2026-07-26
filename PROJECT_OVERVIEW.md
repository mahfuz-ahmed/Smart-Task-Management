# Smart Task Management System - Complete Project Overview

**Version:** 1.0.0  
**Date:** July 2026  
**Author:** Mahfuz Ahmed  
**Assignment:** Software Engineer || .NET Full Stack

## Default Seeded Accounts

On the first application startup, the database is automatically migrated and seeded with the following demo accounts.

| Role            | Email                 | Password       |
| --------------- | --------------------- | -------------- |
| **Admin**       | `admin@smarttask.com` | `Admin@123456` |
| **Team Member** | `user@smarttask.com`  | `User@123456`  |

> **⚠️ Important**
> - ✅ Auto-created on first run (only if database is empty)
> - ❌ Admin Cannot be created manually via registration
> - 📝 New users register as **Project Manager** or **Team Member**

---

## 📋 Table of Contents

1. [Executive Summary](#executive-summary)
2. [Technology Stack](#technology-stack)
3. [System Architecture](#system-architecture)
4. [Backend Architecture](#backend-architecture)
5. [Frontend Architecture](#frontend-architecture)
6. [Database Design](#database-design)
7. [Features & Functionality](#features--functionality)
8. [Security Implementation](#security-implementation)
9. [Performance Optimizations](#performance-optimizations)
10. [Deployment Guide](#deployment-guide)

---

## 📖 Executive Summary

The **Smart Task Management System** is a modern, production-ready web application built with **.NET 10** and **Angular 22**. It demonstrates enterprise-level software engineering practices including clean architecture, industry-standard security, AI integration

### Key Highlights:

- ✅ **Industry-Standard Hybrid RBAC** (System + Project-level roles)
- ✅ **N-Layer Architecture** with SOLID principles
- ✅ **AI-Powered** description enhancement (GitHub Models GPT-4o-mini)
- ✅ **Optimistic Concurrency** (RowVersion)
- ✅ **Soft Delete** with complete audit trail
- ✅ **Rate Limiting** & Health Checks
- ✅ **JWT Authentication** with Refresh Tokens
- ✅ **Angular 22 Standalone Components**
- ✅ **RESTful API** with comprehensive error handling
---

## 🛠 Technology Stack

### Backend Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 10.0 | Runtime framework |
| **C#** | 12.0 | Programming language |
| **ASP.NET Core** | 10.0 | Web API framework |
| **Entity Framework Core** | 10.0 | ORM for database access |
| **SQL Server** | 2019+ | Relational database |
| **GitHub Models** | GPT-4o-mini | AI text enhancement |
| **Serilog** | 4.0 | Structured logging |
| **FluentValidation** | 11.11 | Input validation |
| **BCrypt** | Latest | Password hashing |
| **AspNetCoreRateLimit** | 5.0 | Rate limiting |
| **Swagger/OpenAPI** | 7.3 | API documentation |

### Frontend Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **Angular** | 22.0 | Frontend framework |
| **TypeScript** | 6.0 | Type-safe JavaScript |
| **RxJS** | 7.8 | Reactive programming |
| **Angular Material** | 22.0 | UI component library |
| **Bootstrap** | 5.x | CSS framework |

## 🏗 System Architecture

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client Layer                          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         Angular 22 (Standalone Components)           │   │
│  │  • Reactive Forms  • Lazy Load    │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↕ HTTPS REST API
┌─────────────────────────────────────────────────────────────┐
│                      API Gateway Layer                       │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           ASP.NET Core 10 Web API                    │   │
│  │  • Controllers • Middleware • Rate Limiting          │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    Business Logic Layer                      │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              Service Layer (Infrastructure)          │   │
│  │  • Business Rules • Authorization • AI Integration   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                    Data Access Layer                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │    Repository Pattern + Unit of Work + EF Core      │   │
│  │  • CRUD Operations • Query Optimization              │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────────┐
│                       Database Layer                         │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              SQL Server 2019+                        │   │
│  │  • Relational Data • Indexes • Triggers              │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘

              ┌────────────────────────────┐
              │   External Services        │
              ├────────────────────────────┤
              │ • GitHub Models API       │
              │ • Email Service (future)  │
              └────────────────────────────┘
```


## 🎯 Backend Architecture

### N-Layer Architecture Structure

```
SmartTaskManagement/
│
├── SmartTaskManagement.API/              # Presentation Layer
│   ├── Controllers/                      # API Controllers
│   │   ├── AuthController.cs            # Authentication endpoints
│   │   ├── ProjectsController.cs        # Project CRUD
│   │   ├── TasksController.cs           # Task management
│   │   ├── DashboardController.cs       # Statistics
│   │   ├── AiController.cs              # AI enhancement
│   │   ├── CommentsController.cs        # Task comments
│   │   └── UsersController.cs           # User management
│   │
│   ├── Middleware/                       # Cross-cutting concerns
│   │   └── GlobalExceptionMiddleware.cs # Exception handling
│   │
│   └── Program.cs                        # App configuration
│
├── SmartTaskManagement.Application/      # Application Layer
│   ├── DTOs/                            # Data Transfer Objects
│   │   ├── Auth/                        # Login, Register, Token
│   │   ├── Projects/                    # Project DTOs
│   │   ├── Tasks/                       # Task DTOs
│   │   ├── Comments/                    # Comment DTOs
│   │   └── AI/                          # AI request/response
│   │
│   ├── Interfaces/                       # Service contracts
│   │   ├── IAuthService.cs
│   │   ├── IProjectService.cs
│   │   ├── ITaskService.cs
│   │   └── IAiService.cs
│   │
│   ├── Validators/                       # FluentValidation
│   │   └── ...Validator.cs
│   │
│   └── Common/                           # Shared utilities
│       ├── ApiResponse.cs               # Consistent response wrapper
│       └── PagedResult.cs               # Pagination wrapper
│
├── SmartTaskManagement.Infrastructure/   # Infrastructure Layer
│   ├── Services/                        # Business logic implementation
│   │   ├── AuthService.cs              # JWT, refresh tokens
│   │   ├── ProjectService.cs           # Project business rules
│   │   ├── TaskService.cs              # Task business rules
│   │   ├── AiService.cs                # GitHub Models integration
│   │   └── AuthorizationHelper.cs      # Hybrid RBAC logic
│   │
│   ├── Repositories/                    # Data access implementation
│   │   ├── BaseRepository.cs           # Generic CRUD
│   │   ├── ProjectRepository.cs        # Project-specific queries
│   │   ├── TaskRepository.cs           # Task-specific queries
│   │   └── UserRepository.cs           # User-specific queries
│   │
│   ├── Data/                            # Database context
│   │   ├── AppDbContext.cs             # EF Core DbContext
│   │   └── DatabaseSeeder.cs           # Initial data seeding
│   │
│   ├── Identity/                        # Authentication setup
│   │   └── JwtSettings.cs
│   │
│   └── Migrations/                      # EF Core migrations
│
└── SmartTaskManagement.Domain/           # Domain Layer
    ├── Entities/                        # Domain entities
    │   ├── BaseEntity.cs               # Audit fields, soft delete
    │   ├── User.cs                     # User entity
    │   ├── Project.cs                  # Project entity
    │   ├── TaskItem.cs                 # Task entity
    │   ├── ProjectMember.cs            # Project membership
    │   ├── Comment.cs                  # Task comments
    │   └── TaskActivityLog.cs          # Activity tracking
    │
    ├── Enums/                           # Domain enumerations
    │   ├── UserRole.cs                 # Admin, ProjectManager, TeamMember
    │   ├── ProjectRole.cs              # Manager, Member
    │   ├── ProjectStatus.cs            # Planning, Active, Completed, etc.
    │   ├── TaskStatus.cs               # ToDo, InProgress, Completed, etc.
    │   └── Priority.cs                 # Low, Medium, High, Critical
    │
    └── Interfaces/                      # Domain contracts
        ├── IRepository.cs
        ├── IProjectRepository.cs
        ├── ITaskRepository.cs
        └── IUnitOfWork.cs
```

### Design Patterns Implemented

1. **Repository Pattern** - Data access abstraction
2. **Unit of Work Pattern** - Transaction management
3. **Dependency Injection** - Loose coupling
4. **Factory Pattern** - Entity creation
5. **Strategy Pattern** - Authorization logic
6. **Middleware Pattern** - Cross-cutting concerns
7. **DTO Pattern** - Data transfer separation

---

## 💻 Frontend Architecture

### Angular 22 Standalone Architecture

```
frontend/src/app/
│
├── core/                                 # Core singleton services
│   ├── guards/                          # Route protection
│   │   ├── auth.guard.ts               # Authentication guard
│   │   └── role.guard.ts               # Authorization guard
│   │
│   ├── interceptors/                    # HTTP interceptors
│   │   └── auth.interceptor.ts         # JWT token injection
│   │
│   ├── services/                        # Business services
│   │   ├── auth.service.ts             # Authentication
│   │   ├── project.service.ts          # Project API
│   │   ├── task.service.ts             # Task API
│   │   ├── dashboard.service.ts        # Dashboard API
│   │   ├── ai.service.ts               # AI enhancement
│   │   └── toast.service.ts            # Toast notifications
│   │
│   └── models/                          # TypeScript interfaces
│       └── app.models.ts               # All domain models
│
├── features/                            # Feature modules (lazy loaded)
│   ├── auth/                           # Authentication feature
│   │   ├── login/                      # Login page
│   │   │   ├── login.component.ts     # Standalone component
│   │   │   ├── login.component.html
│   │   │   └── login.component.css
│   │   └── register/                   # Registration page
│   │       └── ...
│   │
│   ├── dashboard/                      # Dashboard feature
│   │   ├── dashboard.component.ts     # Standalone component
│   │   └── ...
│   │
│   ├── projects/                       # Project management
│   │   ├── projects.component.ts      # Project list (standalone)
│   │   ├── project-detail/            # Project details
│   │   │   ├── project-detail.component.ts
│   │   │   └── ...
│   │   └── ...
│   │
│   └── tasks/                          # Task management
│       └── ...
│
├── shared/                              # Shared components
│   ├── components/                     # Reusable components
│   │   ├── confirmation-modal/        # Delete confirmation
│   │   └── ...
│   │
│   └── toast-container/                # Toast notifications
│       └── toast-container.component.ts
│
├── app.component.ts                     # Root component (standalone)
├── app.config.ts                        # App configuration
└── app.routes.ts                        # Route definitions
```

### Angular Modern Features

1. **Standalone Components** - No NgModules
2. **Signal-based State** - Reactive state management
3. **Functional Providers** - `provideRouter()`, `provideHttpClient()`
4. **Inject Function** - Modern dependency injection
5. **Reactive Forms** - Type-safe forms
6. **Lazy Loading** - Feature-based code splitting
7. **Route Guards** - Functional guards
8. **HTTP Interceptors** - Functional interceptors

---

## 🗄 Database Design

### Entity Relationship Diagram

```
┌─────────────┐           ┌──────────────────┐           ┌─────────────┐
│    Users    │           │ ProjectMembers   │           │  Projects   │
├─────────────┤           ├──────────────────┤           ├─────────────┤
│ Id (PK)     │◄─────────┤│ Id (PK)          │├─────────►│ Id (PK)     │
│ FirstName   │           │ UserId (FK)      │           │ Name        │
│ LastName    │           │ ProjectId (FK)   │           │ Description │
│ Email       │           │ ProjectRole      │           │ Status      │
│ PasswordHash│           │ IsActive         │           │ Priority    │
│ Role        │           │ InvitedByUserId  │           │ StartDate   │
│ IsActive    │           │ JoinedAtUtc      │           │ EndDate     │
│ CreatedAt   │           └──────────────────┘           │ CreatedBy   │
│ IsDeleted   │                                          │ CreatedAt   │
└─────────────┘                                          │ RowVersion  │
      │                                                  │ IsDeleted   │
      │                                                  └─────────────┘
      │                                                        │
      │                                                        │
      └────────────────────────┬───────────────────────────────┘
                               │
                               ▼
                        ┌─────────────┐
                        │   Tasks     │
                        ├─────────────┤
                        │ Id (PK)     │
                        │ ProjectId   │
                        │ Title       │
                        │ Description │
                        │ Status      │
                        │ Priority    │
                        │ DueDate     │
                        │ AssignedTo  │
                        │ CreatedBy   │
                        │ CreatedAt   │
                        │ RowVersion  │
                        │ IsDeleted   │
                        └─────────────┘
                               │
                ┌──────────────┴──────────────┐
                │                             │
                ▼                             ▼
         ┌─────────────┐            ┌─────────────────┐
         │  Comments   │            │ TaskActivityLogs│
         ├─────────────┤            ├─────────────────┤
         │ Id (PK)     │            │ Id (PK)         │
         │ TaskId (FK) │            │ TaskId (FK)     │
         │ UserId (FK) │            │ UserId (FK)     │
         │ Content     │            │ Action          │
         │ CreatedAt   │            │ FieldChanged    │
         │ IsDeleted   │            │ OldValue        │
         └─────────────┘            │ NewValue        │
                                    │ CreatedAt       │
                                    │ IsDeleted       │
                                    └─────────────────┘
```

### Key Database Features

1. **Soft Delete** - `IsDeleted` flag on all entities
2. **Audit Trail** - `CreatedAtUtc`, `CreatedByUserId`, `DeletedAtUtc`, `DeletedBy`
3. **Optimistic Concurrency** - `RowVersion` (timestamp) on Projects and Tasks
4. **Indexes** - Strategic indexes on foreign keys and frequently queried fields
5. **Cascading Deletes** - Soft delete cascades to related entities

---

## ⚡ Features & Functionality

### 1. Authentication & Authorization

#### Features:
- ✅ User Registration with email validation
- ✅ Login with JWT access token (15 min expiry)
- ✅ Refresh Token rotation (7 days expiry)
- ✅ Logout (token revocation)
- ✅ Password hashing with BCrypt
- ✅ Role-based authorization (Admin, ProjectManager, TeamMember)

#### Security:
- ✅ HTTPS only
- ✅ Secure HTTP-only cookies for refresh tokens
- ✅ Token expiration handling
- ✅ Automatic token refresh on 401

---

### 2. Project Management

#### Features:
- ✅ Create Project (Admin only)
- ✅ Update Project (Admin only)
- ✅ Delete Project (Admin only, soft delete)
- ✅ View Project Details (members, tasks, statistics)
- ✅ List Projects (paginated, filtered)
- ✅ Search Projects (name, description)
- ✅ Sort Projects (name, date)

#### Business Rules:
- Only Admin can create/update/delete projects
- Project creator automatically becomes a Manager member
- Soft delete cascades to tasks, comments, activity logs

---

### 3. Task Management

#### Features:
- ✅ Create Task (Admin, PM with Manager role)
- ✅ Update Task (role-based permissions)
- ✅ Delete Task (Admin, PM with Manager role)
- ✅ Assign Task to users
- ✅ Update Task Status
- ✅ Set Priority (Low, Medium, High, Critical)
- ✅ Set Due Date
- ✅ Add Comments
- ✅ Activity Logging (automatic)

#### Task Status Flow:
```
ToDo → InProgress → Completed
  ↓         ↓          ↓
       Cancelled ←────┘
```

#### Authorization Matrix:
| Role | Project Role | Create | Update Any | Update Assigned | Delete |
|------|--------------|--------|------------|----------------|--------|
| Admin | - | ✅ | ✅ | ✅ | ✅ |
| PM | Manager | ✅ | ✅ | ✅ | ✅ |
| PM | Member | ❌ | ❌ | ✅ | ❌ |
| TM | - | ❌ | ❌ | ✅ | ❌ |

---

### 4. Project Membership

#### Features:
- ✅ Add Member to Project (Admin, PM with Manager role)
- ✅ Remove Member from Project
- ✅ Assign Project Role (Manager, Member)
- ✅ View Project Members with role badges

#### Project Roles:
- **Manager**: Full project access (create, update, delete tasks, manage members)
- **Member**: Limited access (update assigned tasks only)

---

### 5. Dashboard

#### Statistics:
- ✅ Total Projects
- ✅ Total Tasks
- ✅ My Tasks count
- ✅ Completed vs Pending Tasks
- ✅ Tasks by Status (breakdown)
- ✅ Tasks by Priority (breakdown)
- ✅ Upcoming Due Tasks (next 7 days)
- ✅ Recent Activity

#### Performance:
- ✅ Cached statistics (5-minute cache)
- ✅ Role-based data filtering
- ✅ Efficient SQL queries

---

### 6. AI-Powered Description Enhancement

#### Features:
- ✅ Task description improvement
- ✅ Project description improvement
- ✅ Grammar correction
- ✅ Professional tone adjustment
- ✅ Expansion of short descriptions
- ✅ Actionable descriptions

#### Implementation:
- **Provider**: GitHub Models (https://models.github.ai)
- **Model**: openai/gpt-4o-mini
- **Rate Limit**: 10 requests/minute per user
- **Input Validation**: 10-1000 characters
- **Timeout**: 30 seconds

#### Example:
```
Input:  "fix bug"
Output: "Investigate and resolve the reported bug in the login 
         authentication flow. Steps: 1) Reproduce the issue, 
         2) Identify root cause, 3) Implement fix, 4) Test 
         thoroughly, 5) Deploy to production."
```

---

### 7. Search, Sorting & Pagination

#### Features:
- ✅ Keyword search (projects, tasks)
- ✅ Multi-field filtering (status, priority, date range)
- ✅ Sorting (ASC/DESC, multiple fields)
- ✅ Configurable page size
- ✅ Total count and page navigation

#### Performance:
- ✅ Indexed columns for fast search
- ✅ Query optimization with EF.Functions.Like
- ✅ COUNT query before data fetch

---

## 🔒 Security Implementation

### 1. Authentication

```csharp
// JWT Configuration
{
  "SecretKey": "64+ character secret",
  "Issuer": "SmartTaskManagement",
  "Audience": "SmartTaskManagementClient",
  "ExpiryMinutes": 15
}
```

**Tokens:**
- **Access Token**: 15-minute expiry, in Authorization header
- **Refresh Token**: 7-day expiry, in HTTP-only cookie

---

### 2. Authorization - Hybrid RBAC

**Two-Level Authorization:**

1. **System Role** (User table):
   - Admin
   - ProjectManager
   - TeamMember

2. **Project Role** (ProjectMembers table):
   - Manager
   - Member

**Authorization Logic:**
```
Permission = System Role ∩ Project Role
```

**Example:**
```
User: John
System Role: ProjectManager
Project A: Manager → Full access
Project B: Member → Limited access
Project C: Not member → No access
```

---

### 3. Rate Limiting

```json
{
  "GeneralRules": [
    { "Endpoint": "post:/api/auth/login", "Period": "1m", "Limit": 5 },
    { "Endpoint": "post:/api/auth/register", "Period": "1m", "Limit": 3 },
    { "Endpoint": "post:/api/ai/*", "Period": "1m", "Limit": 10 },
    { "Endpoint": "*", "Period": "1m", "Limit": 100 }
  ]
}
```

---

### 4. Input Validation

- ✅ FluentValidation for all DTOs
- ✅ Required fields validation
- ✅ Length constraints
- ✅ Email format validation
- ✅ Date range validation
- ✅ Enum value validation

---

### 5. Security Headers

- ✅ HTTPS enforcement
- ✅ CORS configuration
- ✅ Content Security Policy
- ✅ X-Content-Type-Options
- ✅ X-Frame-Options

---

## ⚡ Performance Optimizations

### 1. Database Optimizations

- ✅ **Indexes**: Strategic indexes on FK and frequently queried columns
- ✅ **AsNoTracking()**: Read-only queries
- ✅ **Pagination**: Limit result sets
- ✅ **Query Optimization**: Include() for eager loading, explicit loading where needed
- ✅ **Caching**: Dashboard statistics (5-minute cache)

---

### 2. API Optimizations

- ✅ **Async/Await**: All endpoints async
- ✅ **DTOs**: Minimize data transfer
- ✅ **Compression**: Response compression
- ✅ **Rate Limiting**: Prevent abuse
- ✅ **Health Checks**: Monitor application health

---

### 3. Frontend Optimizations

- ✅ **Lazy Loading**: Feature modules loaded on demand
- ✅ **Signal-based State**: Efficient reactivity
- ✅ **OnPush Change Detection**: Reduce change detection cycles
- ✅ **HTTP Interceptors**: Centralized token management
- ✅ **Error Handling**: Global error interceptor

---

## 🚀 Deployment Guide

### Prerequisites

- **.NET 10 SDK**
- **Node.js 18+**
- **SQL Server 2019+**
- **Azure OpenAI API Key** (for AI features)

---

### Backend Deployment

1. **Update Connection String**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=SmartTaskDB;..."
}
```

2. **Run Migrations**:
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
dotnet ef database update
```

3. **Configure Secrets**:
```bash
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_SECRET_KEY"
dotnet user-secrets set "AiSettings:AzureOpenAIKey" "YOUR_API_KEY"
```

4. **Run Application**:
```bash
dotnet run
```

API available at: `https://localhost:7125`

---

### Frontend Deployment

1. **Install Dependencies**:
```bash
cd frontend
npm install
```

2. **Update API URL** (if needed):
```typescript
// environment.ts
export const environment = {
  apiUrl: 'https://your-api-url.com/api'
};
```

3. **Run Development Server**:
```bash
ng serve
```

App available at: `http://localhost:4200`

4. **Build for Production**:
```bash
ng build --configuration production
```

---

### Database Seeding

On first run, the database is automatically seeded with:

- **Admin User**:
  - Email: `admin@task.com`
  - Password: `Admin@123`

- **Project Manager User**:
  - Email: `pm@task.com`
  - Password: `Manager@123`

- **Team Member User**:
  - Email: `tm@task.com`
  - Password: `Member@123`

- **Sample Projects and Tasks**

---

## 📊 Project Statistics

| Metric | Count |
|--------|-------|
| **Backend** |  |
| Controllers | 7 |
| Services | 8 |
| Repositories | 6 |
| Entities | 7 |
| DTOs | 30+ |
| Validators | 10+ |
| Middleware | 1 |
| **Frontend** |  |
| Components | 20+ |
| Services | 10+ |
| Guards | 2 |
| Interceptors | 1 |
| Models | 20+ |
| **Total** |  |
| Lines of Code | 10,000+ |
| API Endpoints | 40+ |
| Database Tables | 7 |

---

## 🎯 Key Achievements

1. ✅ **Production-Ready Code Quality**
2. ✅ **Industry-Standard Architecture**
3. ✅ **Comprehensive Security**
4. ✅ **AI Integration**
5. ✅ **Real-Time Features**
6. ✅ **Performance Optimization**
7. ✅ **Modern Tech Stack**
8. ✅ **Complete Documentation**

---

## 📞 Support & Contact

**Developer**: Mahfuz Ahmed  
**Email**: mahfuz9432@gmail.com
**GitHub**: github.com/mahfuz-ahmed  

---

**Built with ❤️ using .NET 10 and Angular 22**
