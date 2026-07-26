# API Collection Guide

**Project**: Smart Task Management System  
**API Version**: v1  
**Last Updated**: July 26, 2026

---

## 📦 Files Included

1. **SmartTaskManagement.postman_collection.json** - Complete Postman collection with all endpoints
2. **SmartTaskManagement.postman_environment.json** - Environment variables for local development
3. **Swagger UI** - Available at `https://localhost:7125/swagger` when running the API

---

## 🚀 Getting Started

### Option 1: Import into Postman

1. **Open Postman Desktop or Web**
2. **Import Collection**:
   - Click "Import" button
   - Select `SmartTaskManagement.postman_collection.json`
   - Click "Import"

3. **Import Environment**:
   - Click "Import" button
   - Select `SmartTaskManagement.postman_environment.json`
   - Click "Import"

4. **Select Environment**:
   - In top-right corner, select "Smart Task Management - Local" environment
   - The environment is now active

5. **Start Testing**:
   - Ensure the backend API is running on `https://localhost:7125`
   - Start with the "Login" request to get authentication tokens

### Option 2: Use Swagger UI

1. **Run the Backend API**
   ```bash
   cd backend/SmartTaskManagement/src/SmartTaskManagement.API
   dotnet run
   ```

2. **Open Swagger**
   - Navigate to: `https://localhost:7125/swagger`
   - Interactive API documentation with "Try it out" feature

3. **Authenticate**
   - Click "Authorize" button at the top
   - Login first to get access token
   - Paste token in the "Value" field
   - Click "Authorize" then "Close"

---

## 🔐 Authentication Flow

### Step-by-Step Authentication

**1. Login (Get Tokens)**
```
POST /api/auth/login
Body: {
  "email": "admin@task.com",
  "password": "Admin@123"
}

Response: {
  "success": true,
  "data": {
    "accessToken": "eyJhbGc...",  // 15 min expiry
    "refreshToken": "...",         // 7 days expiry
    "user": { ... }
  }
}
```

**2. Automatic Token Handling**
- The Postman collection has a **test script** on the Login request
- It automatically saves `accessToken` and `refreshToken` to environment variables
- All subsequent requests use `{{accessToken}}` from the environment

**3. Token Expiry & Refresh**
```
POST /api/auth/refresh
Body: {
  "refreshToken": "{{refreshToken}}"
}

Response: {
  "success": true,
  "data": {
    "accessToken": "new-token",
    "refreshToken": "new-refresh-token"
  }
}
```

**4. Logout**
```
POST /api/auth/logout
Body: {
  "refreshToken": "{{refreshToken}}"
}
```

---

## 👤 Default Test Users

| Email | Password | Role | System Role | Description |
|-------|----------|------|-------------|-------------|
| admin@task.com | Admin@123 | Admin | Admin | Full system access |
| pm@task.com | Manager@123 | ProjectManager | ProjectManager | Can manage projects |
| tm@task.com | Member@123 | TeamMember | TeamMember | Regular team member |

---

## 📋 API Endpoints Overview

### 🔑 Authentication (`/api/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/auth/register` | Register new user | No |
| POST | `/auth/login` | Login and get tokens | No |
| POST | `/auth/refresh` | Refresh access token | No |
| POST | `/auth/logout` | Logout and revoke token | No |

### 📁 Projects (`/api/projects`)

| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| POST | `/projects` | Create project | Admin |
| GET | `/projects` | List all projects with pagination | Authenticated |
| GET | `/projects/{id}` | Get project details | Authenticated |
| PUT | `/projects/{id}` | Update project | Admin |
| DELETE | `/projects/{id}` | Delete project (soft) | Admin |
| POST | `/projects/{id}/members` | Add member to project | Admin or PM with Manager role |
| DELETE | `/projects/{id}/members/{userId}` | Remove member | Admin or PM with Manager role |

### ✅ Tasks (`/api/tasks`, `/api/projects/{projectId}/tasks`)

| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| POST | `/projects/{projectId}/tasks` | Create task | Admin or PM with Manager role |
| GET | `/projects/{projectId}/tasks` | List tasks by project | Project member |
| GET | `/tasks/{id}` | Get task details | Project member |
| PUT | `/tasks/{id}` | Update task | Hybrid RBAC (see below) |
| DELETE | `/tasks/{id}` | Delete task (soft) | Admin or PM with Manager role |

**Task Update Authorization Rules**:
- **Admin**: Can update any task
- **ProjectManager with Manager role**: Can update any task in their project
- **ProjectManager with Member role**: Can only update their assigned tasks
- **TeamMember**: Can only update their assigned tasks

### 💬 Comments (`/api/tasks/{taskId}/comments`)

| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| POST | `/tasks/{taskId}/comments` | Add comment | Project member |
| GET | `/tasks/{taskId}/comments` | Get all comments | Project member |

### 📊 Dashboard (`/api/dashboard`)

| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| GET | `/dashboard/stats` | Get dashboard statistics | Authenticated |

**Statistics Include**:
- Total projects count
- Active projects count
- Total tasks count
- Tasks by status (ToDo, InProgress, Completed, Cancelled)
- Tasks by priority (Low, Medium, High, Critical)
- Overdue tasks count

### 🤖 AI Enhancement (`/api/ai`)

| Method | Endpoint | Description | Required Role | Rate Limit |
|--------|----------|-------------|---------------|------------|
| POST | `/ai/improve-description` | Enhance task description with AI | Authenticated | 10 req/min |

**Request Body**:
```json
{
  "description": "fix bug",
  "taskTitle": "Bug Fix Task"
}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "improvedDescription": "Implement and complete bug fix task by identifying, diagnosing, and resolving the reported software defect."
  }
}
```

### 👥 Users (`/api/users`)

| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| GET | `/users/me` | Get current user profile | Authenticated |
| GET | `/users` | List all users with pagination | Authenticated |

---

## 🔄 Request/Response Patterns

### Standard Request Headers
```
Authorization: Bearer {{accessToken}}
Content-Type: application/json
```

### Standard Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": null
}
```

### Standard Error Response
```json
{
  "success": false,
  "message": "Error message",
  "data": null,
  "errors": [
    "Detailed error 1",
    "Detailed error 2"
  ]
}
```

### HTTP Status Codes

| Code | Meaning | When It Occurs |
|------|---------|----------------|
| 200 | OK | Successful GET, PUT, DELETE |
| 201 | Created | Successful POST (resource created) |
| 400 | Bad Request | Validation errors, invalid input |
| 401 | Unauthorized | Missing or invalid token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Optimistic concurrency conflict (RowVersion mismatch) |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server-side error |

---

## 🎯 Testing Workflow

### Complete End-to-End Test Flow

**1. Authentication**
```
1. POST /api/auth/login (admin@task.com)
   → Saves accessToken automatically
```

**2. Create a Project**
```
2. POST /api/projects
   Body: {
     "name": "Test Project",
     "description": "Testing project",
     "status": "Active",
     "priority": "High"
   }
   → Copy project ID from response
   → Set as {{projectId}} environment variable
```

**3. Add Members to Project**
```
3. GET /api/users
   → Get user IDs

4. POST /api/projects/{{projectId}}/members
   Body: {
     "userId": "user-guid",
     "projectRole": "Member"
   }
```

**4. Create Tasks**
```
5. POST /api/projects/{{projectId}}/tasks
   Body: {
     "title": "Implement feature",
     "description": "Need to implement new feature",
     "status": "ToDo",
     "priority": "High"
   }
   → Copy task ID from response
   → Set as {{taskId}} environment variable
```

**5. Use AI Enhancement**
```
6. POST /api/ai/improve-description
   Body: {
     "description": "fix bug in login",
     "taskTitle": "Login Bug"
   }
   → Get improved description
```

**6. Add Comments**
```
7. POST /api/tasks/{{taskId}}/comments
   Body: {
     "content": "Working on this task now"
   }

8. GET /api/tasks/{{taskId}}/comments
   → View all comments
```

**7. Update Task**
```
9. PUT /api/tasks/{{taskId}}
   Body: {
     "title": "Updated title",
     "description": "Updated description",
     "status": "InProgress",
     "priority": "High",
     "rowVersion": "AAAAAAAAAAA="  // Get from GET request
   }
```

**8. Dashboard Statistics**
```
10. GET /api/dashboard/stats
    → View overall statistics
```

**9. Cleanup**
```
11. DELETE /api/tasks/{{taskId}}
12. DELETE /api/projects/{{projectId}}
13. POST /api/auth/logout
```

---

## 🔍 Common Query Parameters

### Pagination
All list endpoints support pagination:
```
?page=1&pageSize=10
```

### Search
```
?searchTerm=keyword
```

### Filtering

**Projects**:
```
?status=Active&priority=High
```

**Tasks**:
```
?status=ToDo&priority=High&assignedToUserId=user-guid
```

---

## ⚙️ Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `baseUrl` | API base URL | `https://localhost:7125` |
| `accessToken` | JWT access token (auto-set by login) | `eyJhbGc...` |
| `refreshToken` | Refresh token (auto-set by login) | `refresh-token-here` |
| `projectId` | Current project ID for testing | `guid` |
| `taskId` | Current task ID for testing | `guid` |
| `userId` | Current user ID for testing | `guid` |

**Note**: The `accessToken` and `refreshToken` are automatically set when you run the Login request due to the test script.

---

## 🛡️ Security Features

### Rate Limiting

Configured rate limits per endpoint:

| Endpoint | Limit | Window |
|----------|-------|--------|
| `/api/auth/login` | 5 requests | 1 minute |
| `/api/auth/register` | 3 requests | 1 minute |
| `/api/ai/improve-description` | 10 requests | 1 minute |
| All other endpoints | 100 requests | 1 minute |

**Response when rate limit exceeded**:
```json
{
  "statusCode": 429,
  "message": "API calls quota exceeded! Maximum allowed: 5 per 1m."
}
```

### Authentication

- **JWT Bearer Token** authentication
- **Access Token**: 15 minutes expiry
- **Refresh Token**: 7 days expiry
- Token stored in HTTP-only cookies (frontend)
- Token rotation on refresh

### Authorization

**Hybrid RBAC System**:
1. **System Role** (User.Role): Admin, ProjectManager, TeamMember
2. **Project Role** (ProjectMember.ProjectRole): Manager, Member

Authorization checked at:
- Controller level: `[Authorize(Roles = "Admin")]`
- Service level: `AuthorizationHelper.EnsureCanUpdateTask()`

---

## 🐛 Troubleshooting

### Issue: 401 Unauthorized

**Cause**: Missing or expired access token

**Solution**:
1. Run the Login request again
2. Check that {{accessToken}} is set in environment
3. Verify the token hasn't expired (15 min lifetime)
4. Use Refresh Token endpoint if token expired

### Issue: 403 Forbidden

**Cause**: Insufficient permissions

**Solution**:
1. Check the user's system role
2. Verify project membership for project-specific operations
3. Review authorization rules in API_COLLECTION_GUIDE.md

### Issue: 409 Conflict

**Cause**: Optimistic concurrency conflict (RowVersion mismatch)

**Solution**:
1. GET the latest version of the resource
2. Copy the current `rowVersion` value
3. Include it in your PUT request
4. Try the update again

### Issue: 429 Too Many Requests

**Cause**: Rate limit exceeded

**Solution**:
1. Wait for the rate limit window to reset (1 minute)
2. Reduce request frequency
3. Check rate limits in this guide

### Issue: SSL Certificate Error

**Cause**: Development HTTPS certificate not trusted

**Solution**:
1. **Postman**: Settings → General → SSL certificate verification → OFF
2. **Browser**: Accept the self-signed certificate
3. **Production**: Use valid SSL certificate

### Issue: Connection Refused

**Cause**: API not running

**Solution**:
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
dotnet run
```

---

## 📊 Example Requests & Responses

### Create Project

**Request**:
```http
POST https://localhost:7125/api/projects
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "name": "E-Commerce Platform",
  "description": "Build a modern e-commerce platform with payment integration",
  "status": "Active",
  "priority": "High",
  "startDate": "2026-07-26T00:00:00Z",
  "endDate": "2026-12-31T00:00:00Z"
}
```

**Response** (201 Created):
```json
{
  "success": true,
  "message": "Project created successfully",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "E-Commerce Platform",
    "description": "Build a modern e-commerce platform with payment integration",
    "status": "Active",
    "priority": "High",
    "startDate": "2026-07-26T00:00:00Z",
    "endDate": "2026-12-31T00:00:00Z",
    "createdAtUtc": "2026-07-26T10:00:00Z",
    "memberCount": 0,
    "taskCount": 0,
    "rowVersion": "AAAAAAAAAAA="
  },
  "errors": null
}
```

### Create Task with AI Enhancement

**Request 1** - Enhance Description:
```http
POST https://localhost:7125/api/ai/improve-description
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "description": "add payment gateway",
  "taskTitle": "Payment Integration"
}
```

**Response 1**:
```json
{
  "success": true,
  "data": {
    "improvedDescription": "Implement payment integration by integrating a secure payment gateway solution that supports multiple payment methods including credit cards, debit cards, and digital wallets."
  }
}
```

**Request 2** - Create Task with Enhanced Description:
```http
POST https://localhost:7125/api/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6/tasks
Authorization: Bearer {{accessToken}}
Content-Type: application/json

{
  "title": "Payment Integration",
  "description": "Implement payment integration by integrating a secure payment gateway solution that supports multiple payment methods including credit cards, debit cards, and digital wallets.",
  "status": "ToDo",
  "priority": "High",
  "dueDate": "2026-08-15T00:00:00Z",
  "assignedToUserId": null
}
```

**Response 2**:
```json
{
  "success": true,
  "message": "Task created successfully",
  "data": {
    "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
    "title": "Payment Integration",
    "description": "Implement payment integration by integrating a secure payment gateway solution...",
    "status": "ToDo",
    "statusName": "To Do",
    "priority": "High",
    "priorityName": "High",
    "dueDate": "2026-08-15T00:00:00Z",
    "isOverdue": false,
    "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "projectName": "E-Commerce Platform",
    "assignedToUserId": null,
    "assignedToUserName": null,
    "commentCount": 0,
    "createdAtUtc": "2026-07-26T10:05:00Z",
    "lastModifiedAtUtc": null,
    "rowVersion": "AAAAAAAAAAB="
  }
}
```

### Get Dashboard Statistics

**Request**:
```http
GET https://localhost:7125/api/dashboard/stats
Authorization: Bearer {{accessToken}}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "totalProjects": 5,
    "activeProjects": 3,
    "totalTasks": 23,
    "tasksByStatus": {
      "toDo": 8,
      "inProgress": 10,
      "completed": 4,
      "cancelled": 1
    },
    "tasksByPriority": {
      "low": 5,
      "medium": 10,
      "high": 6,
      "critical": 2
    },
    "overdueTasks": 3
  }
}
```

---

## 📝 Validation Rules

### Register Request
- Email: Required, valid email format, unique
- Password: Required, min 6 characters, must contain uppercase, lowercase, digit, special char
- FullName: Required, 2-100 characters
- Role: Required, must be valid enum value

### Project Request
- Name: Required, 3-200 characters, unique
- Description: Optional, max 2000 characters
- Status: Required, valid enum (Active, OnHold, Completed, Cancelled)
- Priority: Required, valid enum (Low, Medium, High, Critical)
- StartDate: Optional
- EndDate: Optional, must be after StartDate if both provided

### Task Request
- Title: Required, 3-200 characters
- Description: Required, 10-2000 characters
- Status: Required, valid enum (ToDo, InProgress, Completed, Cancelled)
- Priority: Required, valid enum (Low, Medium, High, Critical)
- DueDate: Optional
- AssignedToUserId: Optional, must be project member

### AI Enhancement Request
- Description: Required, 10-1000 characters
- TaskTitle: Optional, max 200 characters

---

## 🎓 For Interviewers

### How to Evaluate This API Collection

**1. Import & Setup** (2 minutes)
- Import collection and environment into Postman
- Start the backend API
- Verify Swagger is accessible

**2. Authentication Flow** (3 minutes)
- Test Login with default credentials
- Verify token auto-saves to environment
- Check token is used in subsequent requests
- Test Refresh Token endpoint

**3. CRUD Operations** (5 minutes)
- Create a project (Admin role)
- Add members to project
- Create tasks in project
- Update task status
- Add comments to task
- Delete resources (soft delete)

**4. Authorization Testing** (5 minutes)
- Login as different users (Admin, PM, TM)
- Verify role-based access control
- Test project-level permissions
- Verify hybrid RBAC (system + project roles)

**5. AI Integration** (3 minutes)
- Test AI description enhancement
- Verify rate limiting (10 req/min)
- Check fallback when AI fails
- Validate input constraints

**6. Advanced Features** (5 minutes)
- Test pagination on list endpoints
- Try search and filtering
- Verify optimistic concurrency (RowVersion)
- Check soft delete behavior
- Test activity logging

**Total Evaluation Time**: ~23 minutes

### Key Points to Highlight

✅ **RESTful Design**: Standard HTTP methods and status codes  
✅ **Authentication**: JWT with refresh tokens and token rotation  
✅ **Authorization**: Hybrid RBAC (system + project-level)  
✅ **Validation**: Comprehensive input validation with clear error messages  
✅ **Rate Limiting**: Protected endpoints with configurable limits  
✅ **AI Integration**: GitHub Models API with fallback mechanism  
✅ **Pagination**: All list endpoints support pagination  
✅ **Soft Delete**: Resources are never hard-deleted  
✅ **Audit Trail**: CreatedAt, ModifiedAt, DeletedAt tracking  
✅ **Concurrency**: Optimistic concurrency with RowVersion  
✅ **Error Handling**: Consistent error response format  
✅ **Documentation**: Swagger UI + Postman collection  

---

## 📚 Additional Resources

### Related Documentation Files
- `README.md` - Project setup and overview
- `PROJECT_OVERVIEW.md` - Architecture and design decisions
- `ARCHITECTURE_DECISIONS.md` - Technical choices explained
- `PROMPTS.md` - AI prompts used in development
- `GITHUB_MODELS_API_FIX.md` - AI integration fix details

### Online Documentation
- **Swagger UI**: `https://localhost:7125/swagger` (when API is running)
- **GitHub Repository**: [Your repo link here]

### API Client Tools
- **Postman**: https://www.postman.com/downloads/
- **Insomnia**: https://insomnia.rest/
- **cURL**: Command-line HTTP client
- **HTTPie**: User-friendly HTTP client

---

For issues or questions about the API collection:

1. **Check Swagger UI**: Most detailed and up-to-date documentation
2. **Review this guide**: Common issues covered in Troubleshooting section
3. **Check logs**: Backend logs in `Logs/` folder with detailed errors
4. **Test with Postman**: Interactive testing and debugging

---

**Last Updated**: July 26, 2026  
**API Version**: v1.0  
**Collection Version**: 1.0  

