# Part 5 - Complete RowVersion Optimistic Concurrency Control

## Goal
Complete the RowVersion (Option B) implementation to prevent concurrent update conflicts when multiple users edit the same task simultaneously.

## Current Status

### ✅ Already Completed
1. **Entity** - `TaskItem.cs` has `RowVersion` property
2. **Configuration** - `TaskItemConfiguration.cs` has `.IsRowVersion()` configured
3. **Migration** - Migration `20260726094648_afterchange.cs` adds RowVersion column to Tasks table
4. **Database** - RowVersion column should be in database (if migration was applied)

### ❌ Remaining Work

#### Backend (4 tasks)
1. **Add RowVersion to TaskDto** - Return RowVersion in GET responses
2. **Add RowVersion to UpdateTaskDto** - Accept RowVersion in PUT requests
3. **Handle DbUpdateConcurrencyException** - Catch and return friendly error message
4. **Verify Migration Applied** - Ensure database has RowVersion column

#### Frontend (3 tasks)
5. **Add rowVersion to Task interface** - Store RowVersion from API
6. **Send rowVersion in update requests** - Include in PUT payload
7. **Handle 409 Conflict response** - Show user-friendly error with refresh option

## Implementation Plan

### Step 1: Update Backend DTOs

**File:** `backend/SmartTaskManagement/src/SmartTaskManagement.Application/DTOs/Tasks/TaskDto.cs`
```csharp
public sealed record TaskDto(
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
    DateTime? LastModifiedAtUtc,
    byte[] RowVersion  // ← ADD THIS
);
```

**File:** `backend/SmartTaskManagement/src/SmartTaskManagement.Application/DTOs/Tasks/UpdateTaskDto.cs`
```csharp
public sealed record UpdateTaskDto(
    string Title,
    string Description,
    int Priority,
    DateTime? DueDate,
    Guid? AssignedToUserId = null,
    int? Status = null,
    byte[]? RowVersion = null  // ← ADD THIS (optional for backward compatibility)
);
```

### Step 2: Update TaskService Mapping

**File:** `backend/SmartTaskManagement/src/SmartTaskManagement.Infrastructure/Services/TaskService.cs`

**In GetByIdAsync() and GetAllAsync()** - Add RowVersion to mapping:
```csharp
RowVersion = task.RowVersion,  // Add this line
```

### Step 3: Update TaskService.UpdateAsync()

**File:** `backend/SmartTaskManagement/src/SmartTaskManagement.Infrastructure/Services/TaskService.cs`

**Modify UpdateAsync()** to:
1. Accept RowVersion from UpdateTaskDto
2. Set it on the entity before update
3. Catch DbUpdateConcurrencyException and throw BusinessException

```csharp
public async Task<ServiceResult<TaskDto>> UpdateAsync(Guid id, UpdateTaskDto dto, string userId)
{
    try
    {
        // ... existing authorization code ...

        // Set properties
        existingTask.Title = dto.Title;
        existingTask.Description = dto.Description;
        existingTask.Priority = (TaskPriority)dto.Priority;
        existingTask.DueDate = dto.DueDate;
        existingTask.AssignedToUserId = dto.AssignedToUserId;
        
        if (dto.Status.HasValue)
            existingTask.Status = (TaskStatus)dto.Status.Value;

        // Set RowVersion if provided (for concurrency check)
        if (dto.RowVersion != null && dto.RowVersion.Length > 0)
        {
            _context.Entry(existingTask).Property(t => t.RowVersion).OriginalValue = dto.RowVersion;
        }

        await _taskRepository.UpdateAsync(existingTask, userId);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(id, userId);
    }
    catch (DbUpdateConcurrencyException)
    {
        throw new BusinessException("The task was modified by another user. Please refresh and try again.");
    }
}
```

### Step 4: Verify Database Migration

Run this query to check if RowVersion column exists:
```sql
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Tasks' AND COLUMN_NAME = 'RowVersion'
```

If not exists, apply migration:
```powershell
cd backend/SmartTaskManagement/src/SmartTaskManagement.Infrastructure
dotnet ef database update --startup-project ../SmartTaskManagement.API
```

### Step 5: Update Frontend Task Interface

**File:** `frontend/src/app/core/models/app.models.ts`

Find `TaskItem` interface and add:
```typescript
export interface TaskItem {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  priority: Priority;
  dueDate?: string;
  projectId: string;
  projectName?: string;
  assignedToUserId?: string;
  assignedToName?: string;
  createdByName?: string;
  createdAt: string;
  updatedAt?: string;
  comments?: TaskComment[];
  activityLogs?: ActivityLog[];
  estimatedHours?: number;
  rowVersion?: string;  // ← ADD THIS (Base64 encoded byte array)
}
```

### Step 6: Update Frontend Update Request

**File:** `frontend/src/app/core/models/app.models.ts`

Find `UpdateTaskRequest` interface and add:
```typescript
export interface UpdateTaskRequest {
  title: string;
  description: string;
  priority: number;
  dueDate?: string | null;
  assignedToUserId?: string | null;
  status?: number;
  rowVersion?: string;  // ← ADD THIS
}
```

### Step 7: Include RowVersion in Update Calls

**File:** `frontend/src/app/features/projects/project-detail/project-detail.component.ts`

**In onTaskSubmit()** when building update payload:
```typescript
const updatePayload: UpdateTaskRequest = {
  title: this.taskForm.value.title!,
  description: this.taskForm.value.description!,
  priority: this.taskForm.value.priority!,
  dueDate: this.taskForm.value.dueDate || null,
  assignedToUserId: this.taskForm.value.assignedToUserId || null,
  status: this.taskForm.value.status!,
  rowVersion: editingTask.rowVersion  // ← ADD THIS
};
```

**In quickChangeStatus()** when updating status:
```typescript
const updatePayload: UpdateTaskRequest = {
  title: task.title,
  description: task.description,
  priority: task.priority,
  dueDate: task.dueDate,
  assignedToUserId: task.assignedToUserId,
  status: newStatus,
  rowVersion: task.rowVersion  // ← ADD THIS
};
```

**In moveTask()** when moving between columns:
```typescript
const updatePayload: UpdateTaskRequest = {
  title: task.title,
  description: task.description,
  priority: task.priority,
  dueDate: task.dueDate,
  assignedToUserId: task.assignedToUserId,
  status: newStatus,
  rowVersion: task.rowVersion  // ← ADD THIS
};
```

### Step 8: Handle Concurrency Errors in Frontend

**File:** `frontend/src/app/features/projects/project-detail/project-detail.component.ts`

Update error handlers to detect concurrency conflicts:

```typescript
error: (err) => {
  this.savingTask.set(false);
  
  // Check for concurrency conflict
  if (err.status === 409 || err?.error?.message?.includes('modified by another user')) {
    this.toastService.error(
      'Conflict Detected',
      'This task was modified by another user. Please refresh and try again.',
      { duration: 5000 }
    );
    // Automatically reload the task
    this.loadProject();
  } else {
    this.toastService.error('Error', err?.error?.message || 'Failed to update task');
  }
}
```

## Testing Scenarios

### Test 1: Single User Update (Should Work)
1. User A opens task
2. User A modifies title
3. User A saves → ✅ Success

### Test 2: Concurrent Updates (Should Detect Conflict)
1. User A opens task (gets RowVersion: v1)
2. User B opens same task (gets RowVersion: v1)
3. User A modifies title and saves → ✅ Success (RowVersion becomes v2)
4. User B modifies description and saves with old v1 → ❌ 409 Conflict
5. User B sees error: "Task was modified by another user"
6. User B refreshes and sees User A's changes
7. User B makes changes again with new RowVersion v2 → ✅ Success

### Test 3: Kanban Board Drag & Drop
1. User A drags task to "In Progress"
2. User B simultaneously drags same task to "Completed"
3. First request succeeds, second gets 409 Conflict
4. Board auto-refreshes showing correct state

## Benefits

✅ **Prevents Lost Updates** - No more overwriting each other's changes  
✅ **Better User Experience** - Clear error message instead of silent data loss  
✅ **Audit Trail Intact** - Activity logs remain accurate  
✅ **Lightweight** - No pessimistic locking needed  
✅ **Automatic** - Database handles versioning

## Error Response Example

**Before RowVersion:**
```
User A: Sets status to "In Progress"
User B: Sets status to "Completed" (overwrites User A's change)
Result: Lost update, activity log shows only User B's action
```

**After RowVersion:**
```
User A: Sets status to "In Progress" ✅
User B: Tries to set status to "Completed" ❌
Response: 409 Conflict - "Task was modified by another user"
User B: Refreshes, sees "In Progress", then sets to "Completed" ✅
Result: Both changes preserved, accurate activity log
```

## Files to Modify

**Backend (3 files):**
1. `DTOs/Tasks/TaskDto.cs` - Add RowVersion property
2. `DTOs/Tasks/UpdateTaskDto.cs` - Add RowVersion parameter
3. `Services/TaskService.cs` - Map RowVersion, handle concurrency exception

**Frontend (2 files):**
4. `core/models/app.models.ts` - Add rowVersion to interfaces
5. `features/projects/project-detail/project-detail.component.ts` - Send rowVersion, handle 409 errors

**Database:**
6. Verify migration applied (RowVersion column exists in Tasks table)

---
**Status:** Ready to implement  
**Estimated Time:** 30-45 minutes  
**Risk:** Low (non-breaking change, backward compatible)
