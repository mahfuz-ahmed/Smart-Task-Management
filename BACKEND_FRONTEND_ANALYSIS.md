# Backend-Frontend Integration Analysis

## Analysis Date: July 25, 2026

---

## 🔴 CRITICAL ISSUES

### 1. **Logout Endpoint - Parameter Mismatch**
**Severity:** HIGH  
**Location:** `AuthController.Logout` vs `auth.service.ts`

**Backend:**
```csharp
[HttpPost("logout")]
public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto, CancellationToken ct)
{
    // Expects: { "refreshToken": "token_value" }
}
```

**Frontend:**
```typescript
logout(): Observable<any> {
    const rfToken = this.refreshTokenValue || '';
    return this.http.post(`${this.apiUrl}/logout`, JSON.stringify(rfToken), {
        headers: { 'Content-Type': 'application/json' }
    })
}
```

**Problem:** Frontend sends a plain string (`"token_value"`), backend expects an object (`{ refreshToken: "token_value" }`).

**Impact:** Logout will always fail with 400 Bad Request.

**Fix Required:** Update frontend to send:
```typescript
return this.http.post(`${this.apiUrl}/logout`, { refreshToken: rfToken })
```

---

### 2. **Project Member - Missing Properties in Backend**
**Severity:** MEDIUM  
**Location:** `ProjectMemberDto` vs `project.service.ts`

**Frontend Expects:**
```typescript
interface ProjectMember {
  userId: string;
  userFullName: string;
  email: string;
  role: string;        // <-- This is "role"
  joinedAt: string;
}
```

**Backend Returns:**
```csharp
record ProjectMemberDto(
    Guid UserId,
    string UserFullName,
    string UserEmail,
    string ProjectRole,  // <-- This is "ProjectRole"
    DateTime JoinedAtUtc
);
```

**Problem:** Property name mismatch: `role` vs `ProjectRole`, `email` vs `UserEmail`, `joinedAt` vs `JoinedAtUtc`.

**Impact:** Frontend won't display member roles and emails correctly.

**Fix Options:**
1. Update backend DTO to match frontend naming
2. Update frontend interface to match backend naming
3. Add mapping in frontend service

---

### 3. **Task Comments - Missing Integration**
**Severity:** HIGH  
**Location:** `task.service.ts` vs `CommentsController`

**Frontend Implementation:**
```typescript
addComment(projectId: string, taskId: string, data: AddCommentRequest): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(
        `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/comments`, data
    );
}

deleteComment(projectId: string, taskId: string, commentId: string): Observable<ApiResponse<any>> {
    return this.http.delete<ApiResponse<any>>(
        `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/comments/${commentId}`
    );
}
```

**Backend Routes:**
```csharp
[Route("api/projects/{projectId:guid}/tasks/{taskId:guid}/comments")]
[HttpPost]
[HttpDelete("{commentId:guid}")]
```

**Problem:** Routes match, but:
1. Frontend `AddCommentRequest.content` should match backend `CreateCommentDto.Content`
2. Backend doesn't return comments in `TaskDto` - frontend expects `comments: TaskComment[]`
3. Missing `GetComments` call in task detail views

**Impact:** Comments feature partially broken.

---

### 4. **Task DTO - Missing Properties**
**Severity:** MEDIUM  
**Location:** `TaskDto` vs `TaskItem` (frontend model)

**Frontend Expects:**
```typescript
interface TaskItem {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: Priority;
  dueDate: string | null;
  projectId: string;
  projectName: string;
  assignedToUserId: string | null;
  assignedToName: string | null;
  createdById: string;
  createdByName: string;
  createdAt: string;
  updatedAt: string;
  estimatedHours: number | null;  // <-- Missing in backend
  tags: string[];                 // <-- Missing in backend
  comments: TaskComment[];        // <-- Missing in backend
  activityLogs: TaskActivityLog[]; // <-- Missing in backend
  attachments: TaskAttachment[];  // <-- Missing in backend
}
```

**Backend Returns:**
```csharp
record TaskDto(
    Guid Id,
    string Title,
    string Description,
    TaskStatus Status,
    string StatusName,
    TaskPriority Priority,
    string PriorityName,
    DateTime? DueDate,
    bool IsOverdue,
    Guid ProjectId,
    string ProjectName,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    int CommentCount,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc
);
```

**Missing in Backend:**
- `createdById` / `createdByName`
- `estimatedHours`
- `tags` array
- `comments` array (only has `CommentCount`)
- `activityLogs` array
- `attachments` array

**Impact:** 
- Task detail views will show incomplete information
- Features like tags, attachments won't work
- Created by information not displayed

---

### 5. **UpdateTaskStatusDto Parameter Mismatch**
**Severity:** MEDIUM  
**Location:** `task.service.ts` vs `TasksController`

**Frontend:**
```typescript
updateStatus(projectId: string, taskId: string, status: number): Observable<ApiResponse<TaskItem>> {
    return this.http.patch<ApiResponse<TaskItem>>(
        `${this.apiUrl}/projects/${projectId}/tasks/${taskId}/status`, 
        { status }  // Sends: { status: 1 }
    );
}
```

**Backend:**
```csharp
[HttpPatch("{taskId:guid}/status")]
public async Task<IActionResult> UpdateStatus(Guid projectId, Guid taskId,
    [FromBody] UpdateTaskStatusDto dto, CancellationToken ct)
```

**Problem:** Need to verify `UpdateTaskStatusDto` structure matches `{ status: number }`.

---

## 🟡 MEDIUM ISSUES

### 6. **Enum Value Consistency**
**Location:** Enums across frontend and backend

**Frontend:**
```typescript
enum TaskStatus {
  ToDo = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
  InReview = 5,
  Blocked = 6,
  OnHold = 7,
}

enum ProjectStatus {
  Planning = 0,
  Active = 1,
  OnHold = 2,
  Completed = 3,
  Cancelled = 4,
}

enum Priority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}
```

**Action Required:** Verify backend enums match these values exactly.

---

### 7. **Query Parameter Naming Inconsistency**
**Severity:** LOW  
**Location:** `task.service.ts` vs `TasksController`

**Frontend:**
```typescript
interface TaskQueryParams {
  pageNumber?: number;  // <-- Uses "pageNumber"
  pageSize?: number;
}
```

**Backend (Projects):**
```typescript
// In ProjectQueryParams
page?: number;         // <-- Uses "page"
pageSize?: number;
```

**Problem:** Inconsistent naming for pagination parameters.

**Impact:** Pagination might not work correctly for tasks.

---

### 8. **Dashboard - Missing Endpoint Verification**
**Severity:** MEDIUM  
**Location:** Dashboard stats

**Frontend Expects:**
```typescript
interface DashboardStats {
  totalProjects: number;
  totalTasks: number;
  myTasks: number;
  completedTasks: number;
  overdueTasks: number;
  upcomingTasks: number;
  tasksByStatus: { [key: string]: number };
  tasksByPriority: { [key: string]: number };
  recentActivity: ActivityItem[];
  projectProgress: ProjectProgressItem[];
}
```

**Action Required:** Verify `DashboardController` returns all these properties.

---

### 9. **AI Enhancement Feature**
**Severity:** LOW  
**Location:** AI controller integration

**Frontend Expects:**
```typescript
interface EnhanceDescriptionRequest {
  description: string;
  context?: string;
}

interface EnhanceDescriptionResponse {
  improvedDescription: string;
}
```

**Action Required:** Verify `AiController` matches these contracts.

---

## 🟢 BUSINESS LOGIC ISSUES

### 10. **ProjectMember - Role Enum Mismatch**
**Location:** `AddMemberRequest` vs `AddProjectMemberDto`

**Frontend:**
```typescript
interface AddMemberRequest {
  userId: string;
  projectRole: number; // 1=Manager, 2=Member
}
```

**Backend:**
```csharp
record AddProjectMemberDto(
    Guid UserId,
    int ProjectRole   // 1=Manager, 2=Member
);
```

**Issue:** Both use numbers, but should verify enum consistency. Consider using string enum values for clarity.

---

### 11. **Missing Validator for LogoutRequestDto**
**Severity:** LOW  
**Location:** `AuthController`

**Current:**
```csharp
[HttpPost("logout")]
public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto, CancellationToken ct)
{
    // No validation
}
```

**Recommendation:** Add FluentValidation validator for consistency:
```csharp
public class LogoutRequestValidator : AbstractValidator<LogoutRequestDto>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required");
    }
}
```

---

### 12. **Error Response Consistency**
**Location:** All controllers

**Current Backend:**
```csharp
return BadRequest(ApiResponse<object>.Fail(v.Errors.Select(e => e.ErrorMessage)));
```

**Frontend Expects:**
```typescript
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}
```

**Issue:** Verify `ApiResponse.Fail()` method properly sets:
- `success: false`
- `message: "Validation failed"` (or similar)
- `errors: ["error1", "error2"]`
- `data: null`

---

### 13. **User Authorization - Role-Based Access Control**
**Location:** All controllers

**Current State:**
```csharp
// Most role-based attributes are commented out
//[Authorize(Roles = "Admin,ProjectManager")]
[Authorize]
```

**Issue:** Role-based access control is disabled. This could lead to:
- Regular users creating/deleting projects
- Non-managers adding/removing members
- Security vulnerabilities

**Recommendation:** Re-enable role-based authorization for production.

---

### 14. **Soft Delete Verification**
**Location:** Entity configurations

**Action Required:** Verify that:
1. All entities have `IsDeleted` flag
2. Global query filters are properly configured
3. Deleted entities are not returned in queries
4. Frontend doesn't receive deleted records

---

## 📋 RECOMMENDED FIXES PRIORITY

### Priority 1 (Critical - Fix Immediately)
1. ✅ Fix logout endpoint parameter mismatch
2. ✅ Add missing TaskDto properties (createdById, createdByName, tags, estimatedHours)
3. ✅ Fix ProjectMember property naming
4. ✅ Integrate comments properly in TaskDto

### Priority 2 (High - Fix Before Production)
1. Add validator for LogoutRequestDto
2. Verify and fix enum consistency
3. Re-enable role-based authorization
4. Add missing task features (attachments, activity logs)

### Priority 3 (Medium - Improve User Experience)
1. Fix query parameter naming inconsistency
2. Add proper error messages
3. Verify dashboard endpoint structure
4. Add pagination metadata

### Priority 4 (Low - Enhancement)
1. Improve API documentation
2. Add request/response examples
3. Standardize date formats (UTC vs local)
4. Add API versioning

---

## 🔧 NEXT STEPS

1. **Create a plan** for each critical issue
2. **Test each endpoint** with Postman/Swagger
3. **Update DTOs** to match frontend expectations
4. **Run integration tests** between frontend and backend
5. **Document any breaking changes**

---

## 📝 VERIFICATION CHECKLIST

- [ ] Logout works correctly
- [ ] Project member list displays properly
- [ ] Task comments can be added/deleted
- [ ] Task details show all information
- [ ] Status update works
- [ ] Dashboard loads correctly
- [ ] AI enhancement works
- [ ] Pagination works on all endpoints
- [ ] Role-based access control is enforced
- [ ] All date/time fields are consistent

---

*Generated on: July 25, 2026*
