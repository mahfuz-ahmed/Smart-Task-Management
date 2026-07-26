# Smart Task Management System
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-22.0-DD0031?style=flat&logo=angular)](https://angular.io/)
[![TypeScript](https://img.shields.io/badge/TypeScript-6.0-3178C6?style=flat&logo=typescript)](https://www.typescriptlang.org/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?style=flat&logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)

---

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


## 📋 Table of Contents

- [Overview](#-overview)
- [Key Features](#-key-features)
- [Technology Stack](#-technology-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [Default Credentials](#-default-credentials)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Database Schema](#-database-schema)
- [Security Features](#-security-features)
- [Development](#-development)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🎯 Overview

Smart Task Management System is a full-stack web application designed for teams to manage projects and tasks efficiently. Built with modern technologies and best practices, it features **AI-powered task enhancement**, **hybrid role-based access control**, and a **responsive Angular frontend**.

### Highlights

✅ **N-Layer Architecture** - Clean separation of concerns  
✅ **Hybrid RBAC** - System-level + Project-level roles  
✅ **AI Integration** - GitHub Models GPT-4o-mini for task descriptions  
✅ **JWT Authentication** - Secure access with refresh tokens  
✅ **Soft Delete** - Complete audit trail  
✅ **Optimistic Concurrency** - RowVersion-based conflict resolution  
✅ **Rate Limiting** - Per-endpoint protection  
✅ **RESTful API** - Comprehensive Swagger documentation  
✅ **Responsive UI** - Angular Material + Bootstrap  

---

## 🌟 Key Features

### 🔐 Authentication & Authorization

- **JWT Bearer Token Authentication**
  - Access tokens (15-minute expiry)
  - Refresh tokens (7-day expiry with rotation)
  - Secure password hashing with BCrypt
  - Token stored in localStorage (frontend) and database (refresh tokens)

- **Hybrid RBAC System**
  - **System Roles**: Admin, ProjectManager, TeamMember
  - **Project Roles**: Manager, Member
  - Fine-grained permissions based on both role types
  - Authorization checks at controller and service levels

### 📊 Project Management

- ✅ Create, Read, Update, Delete (CRUD) operations
- ✅ Project status tracking (Active, OnHold, Completed, Cancelled)
- ✅ Priority levels (Low, Medium, High, Critical)
- ✅ Start and end date management
- ✅ Project member management with role assignment
- ✅ Pagination, search, and filtering
- ✅ Soft delete with audit trail

### 📋 Task Management

- ✅ Task CRUD with status workflow (ToDo, InProgress, Completed, Cancelled)
- ✅ Priority management (Low, Medium, High, Critical)
- ✅ Due date tracking with overdue detection
- ✅ Task assignment to project members
- ✅ Comments and collaboration
- ✅ Activity logging (automatic change tracking)
- ✅ AI-powered description enhancement
- ✅ Filtering by status, priority, and assignee

### 🤖 AI Integration

- **GitHub Models API** (GPT-4o-mini)
  - Automatic task description enhancement
  - Grammar and spelling correction
  - Professional language improvement
  - Clarity and readability optimization
  - Rate limiting (10 requests/minute per user)
  - Fallback mechanism when API unavailable

### 📈 Dashboard & Analytics

- ✅ Total project count
- ✅ Active projects tracking
- ✅ Total tasks overview
- ✅ Tasks by status breakdown
- ✅ Tasks by priority distribution
- ✅ Overdue tasks monitoring
- ✅ Statistics caching (5-minute TTL)

### 💬 Comments & Activity

- ✅ Task comments with timestamps
- ✅ Automatic activity logging for task changes
- ✅ Change history tracking (status, priority, assignment)
- ✅ User attribution for all changes

---

## 🛠 Technology Stack

### Backend (.NET 10)

| Technology | Version | Purpose |
|------------|---------|---------|
| **ASP.NET Core** | 10.0 | Web API framework |
| **C#** | 12.0 | Programming language |
| **Entity Framework Core** | 10.0 | ORM for database access |
| **SQL Server** | 2019+ | Relational database |
| **GitHub Models** | GPT-4o-mini | AI text enhancement |
| **Serilog** | 4.0+ | Structured logging |
| **FluentValidation** | 11.11+ | Input validation |
| **BCrypt.Net** | Latest | Password hashing |
| **AspNetCoreRateLimit** | 5.0+ | API rate limiting |
| **Swashbuckle** | 7.3+ | Swagger/OpenAPI docs |

### Frontend (Angular 22)

| Technology | Version | Purpose |
|------------|---------|---------|
| **Angular** | 22.0 | Frontend framework |
| **TypeScript** | 6.0 | Type-safe JavaScript |
| **RxJS** | 7.8+ | Reactive programming |
| **Angular Material** | 22.0 | UI component library |
| **Bootstrap** | 5.x | CSS framework |

---

## 🏗 Architecture

### Backend Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Client (Angular 22)                    │
└─────────────────────────────────────────────────────────┘
                            ↕ HTTPS REST API
┌─────────────────────────────────────────────────────────┐
│                   API Layer (Controllers)                │
│  • ProjectsController  • TasksController                │
│  • AuthController      • DashboardController            │
│  • CommentsController  • AiController                    │
└─────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────┐
│              Application Layer (Services)                │
│  • ProjectService    • TaskService                      │
│  • AuthService       • DashboardService                  │
│  • AiService         • AuthorizationHelper              │
└─────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────┐
│           Infrastructure Layer (Repositories)            │
│  • ProjectRepository • TaskRepository                    │
│  • UserRepository    • UnitOfWork                       │
│  • DbContext         • Migrations                        │
└─────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────┐
│                  Domain Layer (Entities)                 │
│  • User    • Project   • Task   • TaskComment           │
│  • ProjectMember • TaskActivityLog • RefreshToken       │
└─────────────────────────────────────────────────────────┘
                            ↕
┌─────────────────────────────────────────────────────────┐
│                   Database (SQL Server)                  │
└─────────────────────────────────────────────────────────┘

              ┌────────────────────────────┐
              │   External Services        │
              ├────────────────────────────┤
              │ • GitHub Models API        │
              │ • Email Service (future)   │
              └────────────────────────────┘
```

### Design Patterns

- ✅ **Repository Pattern** - Data access abstraction
- ✅ **Unit of Work** - Transaction management
- ✅ **Dependency Injection** - IoC container
- ✅ **DTO Mapping** - Separation of concerns
- ✅ **Service Layer** - Business logic encapsulation

---

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 20+** - [Download](https://nodejs.org/)
- **SQL Server 2019+** or **SQL Server LocalDB**
- **Postman** (optional) - [Download](https://www.postman.com/downloads/)

---

### Installation

#### 1. Clone the Repository

```bash
git clone https://github.com/mahfuz-ahmed/Smart-Task-Management.git
cd Smart-Task-Management
```

#### 2. Backend Setup

**Step 1: Navigate to API Project**
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
```

**Step 2: Update Connection String** (Optional)

Edit `appsettings.json` if needed:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SmartTaskManagementDb;Integrated Security=true;Encrypt=false;MultipleActiveResultSets=True;Connection Timeout=30;"
  }
}
```

**Step 3: Restore Dependencies**
```bash
dotnet restore
```

**Step 4: Apply Database Migrations**
```bash
dotnet ef database update
```

This creates the database and seeds it with default users and sample data.

**Step 5: Trust HTTPS Certificate** (First time only)
```bash
dotnet dev-certs https --trust
```

**Step 6: Run the API**
```bash
dotnet run
```

The API will start on:
- **HTTPS**: `https://localhost:7125`
- **HTTP**: `http://localhost:5012`
- **Swagger**: `https://localhost:7125/swagger`

#### 3. Frontend Setup

**Step 1: Navigate to Frontend Directory**
```bash
cd frontend
```

**Step 2: Install Dependencies**
```bash
npm install
```

**Step 3: Start Development Server**
```bash
ng serve
```

Or:
```bash
npm start
```

The application will be available at: `http://localhost:4200`

---

## 🔑 Default Credentials

The database is seeded with three test users:

| Role | Email | Password | Permissions |
|------|-------|----------|-------------|
| **Admin** | admin@task.com | Admin@123 | Full system access |
| **Project Manager** | pm@task.com | Manager@123 | Project management |
| **Team Member** | tm@task.com | Member@123 | Task updates (assigned) |

---

## 📡 API Documentation

### Access Points

- **Swagger UI**: `https://localhost:7125/swagger`
- **Postman Collection**: `SmartTaskManagement.postman_collection.json`
- **API Guide**: `API_COLLECTION_GUIDE.md`

### Quick API Reference

#### Authentication

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
```

#### Projects

```http
GET    /api/projects
POST   /api/projects
GET    /api/projects/{id}
PUT    /api/projects/{id}
DELETE /api/projects/{id}
POST   /api/projects/{id}/members
DELETE /api/projects/{id}/members/{userId}
```

#### Tasks

```http
GET    /api/projects/{projectId}/tasks
POST   /api/projects/{projectId}/tasks
GET    /api/tasks/{id}
PUT    /api/tasks/{id}
DELETE /api/tasks/{id}
```

#### Comments

```http
POST /api/tasks/{taskId}/comments
GET  /api/tasks/{taskId}/comments
```

#### AI

```http
POST /api/ai/improve-description
```

#### Dashboard

```http
GET /api/dashboard/stats
```

For detailed documentation, see:
- **`API_COLLECTION_GUIDE.md`** - Complete endpoint reference
- **Swagger UI** - Interactive API explorer

---

## 📁 Project Structure

### Backend Structure

```
backend/SmartTaskManagement/
├── src/
│   ├── SmartTaskManagement.API/          # Web API Layer
│   │   ├── Controllers/                   # API Endpoints
│   │   │   ├── AuthController.cs
│   │   │   ├── ProjectsController.cs
│   │   │   ├── TasksController.cs
│   │   │   ├── CommentsController.cs
│   │   │   ├── DashboardController.cs
│   │   │   ├── AiController.cs
│   │   │   └── UsersController.cs
│   │   ├── Middleware/                    # Custom Middleware
│   │   │   └── GlobalExceptionMiddleware.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json        # Launch profiles
│   │   ├── appsettings.json              # Configuration
│   │   └── Program.cs                     # App entry point
│   │
│   ├── SmartTaskManagement.Application/   # Application Layer
│   │   ├── DTOs/                          # Data Transfer Objects
│   │   ├── Interfaces/                    # Service interfaces
│   │   └── Mappings/                      # DTO mappings
│   │
│   ├── SmartTaskManagement.Infrastructure/ # Infrastructure Layer
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs    # EF Core context
│   │   │   └── DbSeeder.cs               # Data seeding
│   │   ├── Repositories/                  # Data access
│   │   │   ├── BaseRepository.cs
│   │   │   ├── ProjectRepository.cs
│   │   │   ├── TaskRepository.cs
│   │   │   ├── UserRepository.cs
│   │   │   └── UnitOfWork.cs
│   │   ├── Services/                      # Business services
│   │   │   ├── AuthService.cs
│   │   │   ├── ProjectService.cs
│   │   │   ├── TaskService.cs
│   │   │   ├── DashboardService.cs
│   │   │   ├── GitHubModelsAiService.cs
│   │   │   └── AuthorizationHelper.cs
│   │   └── Migrations/                    # EF Migrations
│   │
│   └── SmartTaskManagement.Domain/         # Domain Layer
│       ├── Entities/                       # Domain entities
│       │   ├── User.cs
│       │   ├── Project.cs
│       │   ├── TaskItem.cs
│       │   ├── ProjectMember.cs
│       │   ├── TaskComment.cs
│       │   ├── TaskActivityLog.cs
│       │   ├── RefreshToken.cs
│       │   └── BaseEntity.cs
│       └── Enums/                          # Domain enums
│           ├── UserRole.cs
│           ├── ProjectStatus.cs
│           ├── TaskStatus.cs
│           ├── Priority.cs
│           └── ProjectRole.cs
└── SmartTaskManagement.sln                 # Solution file
```

### Frontend Structure

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/                          # Core module
│   │   │   ├── guards/                    # Route guards
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── role.guard.ts
│   │   │   ├── interceptors/              # HTTP interceptors
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   ├── models/                    # TypeScript interfaces
│   │   │   └── services/                  # Core services
│   │   │       ├── auth.service.ts
│   │   │       ├── project.service.ts
│   │   │       ├── task.service.ts
│   │   │       ├── dashboard.service.ts
│   │   │       ├── ai.service.ts
│   │   │       └── user.service.ts
│   │   │
│   │   ├── features/                      # Feature modules
│   │   │   ├── auth/                      # Authentication
│   │   │   │   ├── login/
│   │   │   │   └── register/
│   │   │   ├── dashboard/                 # Dashboard
│   │   │   ├── projects/                  # Projects
│   │   │   │   ├── project-list/
│   │   │   │   ├── project-detail/
│   │   │   │   └── project-form/
│   │   │   ├── tasks/                     # Tasks
│   │   │   │   ├── task-list/
│   │   │   │   ├── task-detail/
│   │   │   │   └── task-form/
│   │   │   └── shell/                     # App shell
│   │   │
│   │   ├── shared/                        # Shared components
│   │   │   └── components/
│   │   │       ├── confirmation-modal/
│   │   │       └── toast-container/
│   │   │
│   │   ├── app.component.ts               # Root component
│   │   ├── app.config.ts                  # App configuration
│   │   └── app.routes.ts                  # Route configuration
│   │
│   ├── assets/                            # Static assets
│   ├── index.html                         # HTML entry point
│   ├── main.ts                            # TypeScript entry point
│   └── styles.css                         # Global styles
│
├── angular.json                           # Angular CLI config
├── package.json                           # Dependencies
├── tsconfig.json                          # TypeScript config
└── README.md                              # Frontend docs
```

---

## 🗄 Database Schema

### Core Entities

```
Users
├── Id (PK)
├── Email (Unique)
├── PasswordHash
├── FullName
├── Role (Admin/ProjectManager/TeamMember)
├── IsDeleted
└── Audit fields

Projects
├── Id (PK)
├── Name
├── Description
├── Status
├── Priority
├── StartDate
├── EndDate
├── RowVersion (Concurrency)
└── Audit fields

ProjectMembers
├── Id (PK)
├── ProjectId (FK)
├── UserId (FK)
├── ProjectRole (Manager/Member)
└── JoinedAt

TaskItem (Tasks)
├── Id (PK)
├── ProjectId (FK)
├── AssignedToUserId (FK, nullable)
├── Title
├── Description
├── Status
├── Priority
├── DueDate
├── RowVersion (Concurrency)
└── Audit fields

TaskComments
├── Id (PK)
├── TaskId (FK)
├── UserId (FK)
├── Content
└── CreatedAt

TaskActivityLog
├── Id (PK)
├── TaskId (FK)
├── UserId (FK)
├── ActionType (Created/Updated/Deleted)
├── Description
└── Timestamp

RefreshTokens
├── Id (PK)
├── UserId (FK)
├── Token (Unique)
├── ExpiresAtUtc
├── CreatedAtUtc
└── RevokedAtUtc
```

### Relationships

- User **1:N** Projects (via ProjectMembers)
- User **1:N** Tasks (as assignee)
- User **1:N** Comments
- User **1:N** ActivityLogs
- User **1:N** RefreshTokens
- Project **1:N** Tasks
- Project **1:N** ProjectMembers
- Task **1:N** Comments
- Task **1:N** ActivityLogs

---

## 🔒 Security Features

### Authentication

✅ **JWT Bearer Tokens**
- Access tokens with 15-minute expiry
- Refresh tokens with 7-day expiry
- Token rotation on refresh
- Secure token storage

✅ **Password Security**
- BCrypt hashing algorithm
- Minimum length: 6 characters
- Required: uppercase, lowercase, digit, special character

### Authorization

✅ **Hybrid RBAC**
- System-level roles (Admin, ProjectManager, TeamMember)
- Project-level roles (Manager, Member)
- Fine-grained permission checks

✅ **Authorization Rules**
- Admin: Full system access
- ProjectManager + Manager role: Full project access
- ProjectManager + Member role: Limited to assigned tasks
- TeamMember: Own assigned tasks only

### API Security

✅ **Rate Limiting**
- Login: 5 requests/minute
- Register: 3 requests/minute
- AI Enhancement: 10 requests/minute
- General: 100 requests/minute

✅ **Input Validation**
- FluentValidation for all DTOs
- SQL injection prevention (parameterized queries)
- XSS protection (sanitized output)

✅ **Data Protection**
- Soft delete (no hard deletes)
- Audit trails (who, when)
- Optimistic concurrency (RowVersion)
- HTTPS enforcement (development + production)

---

## 💻 Development

### Running in Development Mode

**Backend:**
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
dotnet watch run
```
Hot reload enabled - changes automatically restart the server.

**Frontend:**
```bash
cd frontend
ng serve --open
```
Live reload enabled - changes automatically refresh the browser.

### Building for Production

**Backend:**
```bash
dotnet publish -c Release -o ./publish
```

**Frontend:**
```bash
ng build --configuration production
```
Output in `frontend/dist/`

### Database Migrations

**Create new migration:**
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
dotnet ef migrations add MigrationName --project ../SmartTaskManagement.Infrastructure
```

**Apply migrations:**
```bash
dotnet ef database update
```

**Rollback migration:**
```bash
dotnet ef database update PreviousMigrationName
```

**Generate SQL script:**
```bash
dotnet ef migrations script -o database-script.sql
```

---

## 🧪 Testing

### Manual Testing

**Swagger UI:**
```
https://localhost:7125/swagger
```
Interactive API testing with "Try it out" feature.

**Postman Collection:**
1. Import `SmartTaskManagement.postman_collection.json`
2. Import `SmartTaskManagement.postman_environment.json`
3. Select environment in Postman
4. Run "Login" to get tokens
5. Test other endpoints

### Test Workflow

1. **Login** as Admin
2. **Create Project**
3. **Add Members** to project
4. **Create Tasks**
5. **Enhance Description** with AI
6. **Add Comments**
7. **Update Task Status**
8. **View Dashboard** stats

---

## 🚀 Deployment

### Backend Deployment

**Azure App Service:**
```bash
az webapp up --name smart-task-api --resource-group myResourceGroup
```

**Docker:**
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
docker build -t smart-task-api .
docker run -p 8080:80 smart-task-api
```

### Frontend Deployment

**Azure Static Web Apps:**
```bash
az staticwebapp create --name smart-task-frontend --resource-group myResourceGroup
```

**Netlify:**
```bash
ng build --configuration production
netlify deploy --prod --dir=dist/frontend/browser
```

### Environment Variables (Production)

**Backend:**
```env
ConnectionStrings__DefaultConnection=<production-db-connection>
JwtSettings__SecretKey=<production-secret-key>
AiSettings__GitHubToken=<github-models-token>
ASPNETCORE_ENVIRONMENT=Production
```

**Frontend:**
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-api-domain.com/api'
};
```

---

## 📚 Additional Documentation

- **`PROJECT_OVERVIEW.md`** - Architecture and design overview
- **`ARCHITECTURE_DECISIONS.md`** - Technical decisions explained
- **`API_COLLECTION_GUIDE.md`** - Complete API reference
- **`PROMPTS.md`** - AI assistance documentation
- **`FOR_INTERVIEWER.md`** - Quick evaluation guide

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# coding conventions
- Use Angular style guide
- Write meaningful commit messages
- Add unit tests for new features
- Update documentation

---

## 👤 Author

**Mahfuz Ahmed**

- GitHub: [@Mahfuz Ahmed](https://github.com/Mahfuz-Ahmed)
- LinkedIn: [Mahfuz Ahmed](https://linkedin.com/in/the-mahfuz-ahmed)

---

## 🙏 Acknowledgments

- [ASP.NET Core](https://docs.microsoft.com/aspnet/core) - Web framework
- [Angular](https://angular.io) - Frontend framework
- [Entity Framework Core](https://docs.microsoft.com/ef/core) - ORM
- [GitHub Models](https://github.com/marketplace/models) - AI API
- [Angular Material](https://material.angular.io) - UI components
- [Bootstrap](https://getbootstrap.com) - CSS framework

---

## 📞 Support & Contact

**Developer**: Mahfuz Ahmed  
**Email**: mahfuz9432@gmail.com
**GitHub**: github.com/mahfuz-ahmed  

---

<div align="center">

**Built with ❤️ using .NET 10 and Angular 22**

⭐ Star this repo if you find it helpful!

</div>
