# 🧪 Smart Task Management System - Complete Testing Guide

## 📋 Table of Contents
1. [Prerequisites](#prerequisites)
2. [Complete Business Flow Testing](#complete-business-flow-testing)
3. [Feature-by-Feature Testing](#feature-by-feature-testing)
4. [Edge Cases & Validation Testing](#edge-cases--validation-testing)
5. [Security & Authorization Testing](#security--authorization-testing)
6. [Performance & Load Testing](#performance--load-testing)
7. [Common Issues & Solutions](#common-issues--solutions)

---

## Prerequisites

### 1. Start Backend API
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
dotnet run
```
✅ Backend running at: `https://localhost:7125`  
✅ Swagger UI: `https://localhost:7125/swagger`

### 2. Start Frontend Angular App
```bash
cd frontend
npm start
```
✅ Frontend running at: `http://localhost:4200`

### 3. Check Database
- SQL Server running
- Database: `SmartTaskManagementDb`
- Tables created via migrations
- Seed data loaded (demo users)

---

## 🎯 Complete Business Flow Testing

### **Scenario 1: New User Onboarding & Project Creation**

#### **Step 1: User Registration**
1. Navigate to `http://localhost:4200`
2. Click **"Sign Up"** or **"Register"** button
3. Fill registration form:
   ```
   First Name: John
   Last Name: Doe
   Email: john.doe@example.com
   Password: Test@1234
   Confirm Password: Test@1234
   Role: Admin (or ProjectManager or TeamMember)
   ```
4. Click **"Register"** button

**✅ Expected Result:**
- Success toast notification: "Registration successful"
- Automatically logged in
- Redirected to Dashboard
- JWT token stored in localStorage
- User profile visible in header

**❌ Test Negative Cases:**
- Empty fields → Should show validation errors
- Password mismatch → "Passwords do not match"
- Weak password → "Password must contain..."
- Duplicate email → "Email already registered"

---

#### **Step 2: View Dashboard**
1. After login, you should see Dashboard with:
   - **Total Projects** count
   - **Total Tasks** count
   - **My Tasks** count (assigned to you)
   - **Completed Tasks** vs **Pending Tasks**
   - **Overdue Tasks** count
   - **Upcoming Tasks** (due in next 7 days)
   - **Tasks by Status** chart (ToDo, InProgress, Completed, Cancelled)
   - **Tasks by Priority** chart (Low, Medium, High, Critical)
   - **Recent Activity Feed**
   - **Project Progress** cards

**✅ Expected Result:**
- All counts initially showing 0 (for new user)
- Charts showing empty states
- No recent activity
- Clean, responsive glassmorphic UI

---

#### **Step 3: Create First Project**
1. Click **"Projects"** from sidebar/menu
2. Click **"New Project"** button (top right)
3. Fill project form:
   ```
   Project Name: E-Commerce Platform
   Description: Build a modern e-commerce platform with payment integration and inventory management
   Status: Active
   Priority: High
   Start Date: 2026-07-23
   End Date: 2026-12-31
   ```
4. Watch character counter update as you type in Description
5. Click **"Create Project"** button

**✅ Expected Result:**
- Character counter shows: `0/500` → `96/500`
- Form validates all required fields
- Success toast: "Project created! E-Commerce Platform"
- Modal closes automatically
- New project card appears in projects grid
- Project shows:
  - Name, description
  - Status badge (Active)
  - Priority accent bar (orange for High)
  - Task stats (0 total, 0 completed)
  - Your avatar as creator
  - Three-dot menu for actions

**❌ Test Validation:**
- Try without Name → "Name is required"
- Try with 1-2 chars name → "Name must be at least 3 characters"
- Try without Description → "Description is required"
- Try End Date before Start Date → "End date must be after start date"

---

#### **Step 4: Add Project Members**
1. Click on project card or three-dot menu → **"View Details"**
2. Navigate to **"Members"** tab or section
3. Click **"Add Member"** button
4. Enter member details:
   ```
   Email: jane.smith@example.com
   Role: Member (or ProjectManager)
   ```
5. Click **"Add Member"**

**✅ Expected Result:**
- Success toast: "Member added to project"
- Member appears in members list with:
  - Full name
  - Email
  - Role badge
  - Join date
  - Remove button (for admin/PM)

**❌ Test Negative:**
- Non-existent email → "User not found"
- Already added member → "Member already exists in project"

---

#### **Step 5: Create Tasks in Project**
1. In Project Details page, go to **"Tasks"** tab
2. Click **"New Task"** or **"+ Add Task"** button
3. Fill task form:
   ```
   Title: Design Database Schema
   Description: Create ERD and design normalized database schema for e-commerce platform
   Status: ToDo
   Priority: High
   Assigned To: John Doe (yourself or member)
   Due Date: 2026-08-15
   ```
4. **OPTIONAL:** Click **"AI Enhance"** button to improve description
5. Click **"Create Task"**

**✅ Expected Result:**
- Task appears in Kanban board under "To Do" column
- Task card shows:
  - Title
  - Priority badge
  - Due date
  - Assigned user avatar
  - Task description (truncated)
- AI Enhanced description (if used):
  - More professional wording
  - Action-oriented
  - Clear and concise

**Create More Tasks:**
```
Task 2: Setup Development Environment
Description: Install Docker, Node.js, .NET SDK, configure VS Code extensions
Priority: Critical
Due Date: 2026-07-25

Task 3: Implement User Authentication
Description: Build JWT-based auth with role-based access control
Priority: High
Due Date: 2026-08-20

Task 4: Design Homepage UI
Description: Create responsive homepage with hero section and product showcase
Priority: Medium
Due Date: 2026-08-30
```

---

#### **Step 6: Move Tasks on Kanban Board**
1. View tasks in Kanban view (default)
2. **Drag and drop** OR click task and change status:
   - "Design Database Schema" → Move to **"In Progress"**
   - "Setup Development Environment" → Move to **"In Progress"**
   
**✅ Expected Result:**
- Tasks smoothly move between columns
- Status updates in real-time
- Activity log records the change: "changed status to InProgress"
- Dashboard stats update automatically

**Alternative: Click-based Status Change**
1. Click on task card to open details
2. Use status dropdown or status buttons
3. Click desired status (ToDo, InProgress, Completed, Cancelled)

---

#### **Step 7: Update Task Details**
1. Click on **"Design Database Schema"** task
2. Update task:
   ```
   Add Comment: "Started working on user and product tables"
   Update Priority: Critical (was High)
   Update Description: Add more details
   ```
3. Click **"Save"** or **"Update Task"**

**✅ Expected Result:**
- Task details updated
- Comment appears in comments section with:
  - Your name and avatar
  - Timestamp
  - Comment text
- Activity log shows:
  - "Priority changed from High to Critical"
  - "Description updated"
  - "Comment added"

---

#### **Step 8: Complete Tasks**
1. Move **"Setup Development Environment"** to **"Completed"**
2. Check dashboard:
   - Completed Tasks count increased
   - Pending Tasks count decreased
   - Tasks by Status chart updated
   - Project progress percentage increased

**✅ Expected Result:**
- Task moved to "Completed" column
- Task card shows completed checkmark/badge
- Green accent on completed tasks
- Project card shows updated completion: "1/4 tasks completed (25%)"

---

#### **Step 9: View Task Activity & Comments**
1. Click on any task to open details modal/page
2. Scroll to **"Activity"** section
3. View activity log entries:
   ```
   ✅ Task created by John Doe - 2 hours ago
   ✅ Status changed to InProgress by John Doe - 1 hour ago
   ✅ Priority changed from High to Critical by John Doe - 30 min ago
   ✅ Comment added by John Doe - 15 min ago
   ✅ Status changed to Completed by John Doe - 5 min ago
   ```

**✅ Expected Result:**
- Complete audit trail of all changes
- User names and timestamps
- Clear action descriptions
- Chronological order (newest first)

---

#### **Step 10: Add Task Comments**
1. In task details, find **"Comments"** section
2. Type comment:
   ```
   "Great work! Database schema looks good. Please add indexes for foreign keys."
   ```
3. Click **"Add Comment"** or **"Post"**

**✅ Expected Result:**
- Comment appears immediately
- Shows your avatar and name
- "Just now" or timestamp
- Can edit/delete own comments (if implemented)

---

### **Scenario 2: Multi-User Collaboration (Requires 2+ Users)**

#### **Step 1: Register Second User**
1. Logout first user (John Doe)
2. Register new user:
   ```
   Name: Jane Smith
   Email: jane.smith@example.com
   Password: Test@1234
   Role: TeamMember
   ```
3. Login as Jane

#### **Step 2: Jane Views Assigned Tasks**
1. Go to Dashboard → **"My Tasks"** section
2. Click **"Tasks"** from menu → Filter by "Assigned to Me"
3. View tasks assigned by John

**✅ Expected Result:**
- Jane sees only tasks assigned to her
- Can update status and add comments
- Cannot delete project (not admin)
- Can view project details

#### **Step 3: Real-time Notifications (if SignalR working)**
1. Keep Jane logged in on one browser
2. Login as John on another browser (or incognito)
3. As John: Assign a new task to Jane
4. As Jane: Watch for notification

**✅ Expected Result:**
- Bell icon shows notification badge
- Toast notification appears
- Notification message: "John Doe assigned you to 'New Task Name'"

---

### **Scenario 3: Search, Filter & Pagination**

#### **Step 1: Create Multiple Projects & Tasks**
- Create 5-10 projects with different:
  - Statuses (Active, OnHold, Completed)
  - Priorities (Low, Medium, High, Critical)
- Create 20+ tasks across projects

#### **Step 2: Test Project Search**
1. Go to **Projects** page
2. Type in search box: `"ecommerce"`
3. Results filter in real-time

**✅ Expected Result:**
- Only matching projects shown
- Search works on name and description
- No page reload (client-side filtering)

#### **Step 3: Test Task Filters**
1. Go to **Tasks** page
2. Apply filters:
   - **Status:** InProgress
   - **Priority:** High
   - **Assigned To:** John Doe
3. Click **"Apply Filters"**

**✅ Expected Result:**
- Only tasks matching ALL filters shown
- Filter counts update
- Can clear filters

#### **Step 4: Test Sorting**
1. Click column headers to sort:
   - Sort by **Due Date** (ascending/descending)
   - Sort by **Priority** (Critical → Low)
   - Sort by **Status**

**✅ Expected Result:**
- Data re-orders correctly
- Sort indicator shows (↑ ↓)
- Works with filters

#### **Step 5: Test Pagination**
1. If you have 20+ tasks, pagination appears
2. Test:
   - Next/Previous buttons
   - Jump to specific page
   - Change page size (10, 25, 50, 100)

**✅ Expected Result:**
- Smooth page transitions
- Correct page numbers
- Total count displayed
- "Showing 1-10 of 47 tasks"

---

### **Scenario 4: Dashboard Analytics**

#### **Step 1: Create Diverse Data**
- 5 projects (2 Active, 1 OnHold, 1 Completed, 1 Archived)
- 30 tasks:
  - 8 ToDo
  - 10 InProgress
  - 10 Completed
  - 2 Cancelled
- Priorities:
  - 5 Low
  - 10 Medium
  - 10 High
  - 5 Critical

#### **Step 2: Verify Dashboard Metrics**
1. Go to Dashboard
2. Check counts match your data:
   ```
   Total Projects: 5
   Total Tasks: 30
   My Tasks: [number assigned to you]
   Completed: 10
   Pending: 18 (ToDo + InProgress)
   Overdue: [tasks past due date]
   Upcoming: [tasks due in next 7 days]
   ```

#### **Step 3: Verify Charts**
1. **Tasks by Status** pie/bar chart:
   - ToDo: 8
   - InProgress: 10
   - Completed: 10
   - Cancelled: 2

2. **Tasks by Priority** chart:
   - Low: 5
   - Medium: 10
   - High: 10
   - Critical: 5

**✅ Expected Result:**
- Visual charts render correctly
- Colors match priority/status
- Percentages calculated correctly
- Interactive (hover shows details)

#### **Step 4: Recent Activity Feed**
1. View last 10 activities
2. Should show:
   - Task created/updated
   - Status changes
   - Assignments
   - Comments added
   - With user names and timestamps

**✅ Expected Result:**
- Chronological order (newest first)
- Clear action descriptions
- Links to tasks/projects
- User avatars

---

### **Scenario 5: AI Task Description Enhancement**

#### **Step 1: Create Task with Poor Description**
1. Create new task:
   ```
   Title: fix bug
   Description: the login thing not working properly need to fix asap
   ```
2. Click **"AI Enhance"** button

#### **Step 2: AI Processing**
**✅ Expected Result:**
- Loading spinner appears
- Button shows "Enhancing..." with spinner
- After 2-5 seconds:
  ```
  Enhanced Description:
  "Investigate and resolve the authentication issue preventing users from 
  logging into the system. Identify the root cause of the login failure, 
  implement a fix, and verify the solution across all supported browsers."
  ```

#### **Step 3: Accept Enhanced Description**
1. Review AI suggestion
2. Click **"Use Enhanced"** or manually edit
3. Save task

**✅ Expected Result:**
- Professional, actionable description
- Grammar corrected
- More detailed and clear
- Imperative mood (commands)

**❌ Test Error Cases:**
- No internet → "AI enhancement failed"
- Invalid API key → Fallback to original
- Empty description → "Description cannot be empty"

---

## 🧩 Feature-by-Feature Testing

### **1. Authentication & Authorization**

#### Test JWT Token
1. Login successfully
2. Open Browser DevTools → Application → Local Storage
3. Check for token: `authToken` or `token`
4. Copy token value
5. Go to [jwt.io](https://jwt.io)
6. Paste token and decode
7. Verify claims:
   ```json
   {
     "sub": "user-guid",
     "email": "john.doe@example.com",
     "given_name": "John",
     "family_name": "Doe",
     "role": "Admin",
     "jti": "token-id",
     "exp": 1234567890
   }
   ```

#### Test Token Expiry & Refresh
1. Wait for token to expire (15 minutes by default)
2. Make any API request
3. System should:
   - Detect expired token
   - Call `/api/auth/refresh-token`
   - Get new access token
   - Retry original request
4. **OR** manually expire token:
   - Clear localStorage
   - Try accessing protected page
   - Should redirect to login

#### Test Role-Based Access
1. Login as **TeamMember**
2. Try to:
   - ❌ Delete project (should fail - only Admin/PM)
   - ✅ View projects (allowed)
   - ✅ Create tasks (allowed)
   - ❌ Remove project members (should fail)

3. Login as **Admin**
4. Try all actions:
   - ✅ All CRUD operations allowed
   - ✅ Can delete any project
   - ✅ Can manage all members

**✅ Expected Result:**
- 403 Forbidden for unauthorized actions
- Error toast: "You don't have permission..."
- Button disabled or hidden for restricted actions

---

### **2. Project Management**

#### Create Project
- ✅ All fields validate correctly
- ✅ Character counter works
- ✅ Date validation works
- ✅ Success notification
- ✅ Project appears in list

#### Edit Project
1. Click three-dot menu → **"Edit"**
2. Modal pre-fills with existing data
3. Change name: `"E-Commerce Platform v2"`
4. Click **"Update Project"**

**✅ Expected:**
- Changes reflect immediately
- No page reload
- Success toast

#### Delete Project
1. Click three-dot menu → **"Delete"**
2. Confirmation modal appears:
   ```
   Are you sure you want to delete 'E-Commerce Platform v2'?
   This action cannot be undone. All tasks will be deleted.
   ```
3. Click **"Delete"** (red button)

**✅ Expected:**
- Project removed from list
- All tasks deleted (cascade)
- Success toast
- Can undo? (if implemented)

---

### **3. Task Management**

#### Task CRUD Operations
Test all operations:
- ✅ Create task
- ✅ Edit task details
- ✅ Change status (drag/drop or dropdown)
- ✅ Change priority
- ✅ Assign/reassign to user
- ✅ Update due date
- ✅ Delete task (with confirmation)

#### Task Views
1. **Kanban View:**
   - 4 columns (ToDo, InProgress, Completed, Cancelled)
   - Drag and drop works
   - Task counts per column
   - Smooth animations

2. **List View:**
   - Tabbed table view
   - Sort by any column
   - Filters work
   - Pagination

3. **My Tasks View:**
   - Shows only tasks assigned to me
   - Filter by status
   - Quick status updates

---

### **4. Comments & Activity Logs**

#### Test Comments
1. Add comment to task
2. Edit comment (if allowed)
3. Delete comment (if allowed)
4. View comments from other users

**✅ Expected:**
- Real-time updates (if SignalR)
- Markdown support? (if implemented)
- Mention users? (if implemented)

#### Test Activity Logs
1. Perform multiple actions on a task:
   - Create
   - Change status
   - Change priority
   - Reassign
   - Add comment
   - Update title/description
2. View activity log

**✅ Expected:**
- All actions logged
- Correct timestamps
- User names
- Before/after values for changes

---

### **5. Notifications (SignalR)**

#### Test Real-time Notifications
1. Open app in two browsers (User A & User B)
2. User A assigns task to User B
3. User B should receive notification instantly

**✅ Expected:**
- Bell icon badge increments
- Toast notification pops up
- Notification appears in list
- Click notification → navigates to task

#### Test Notification Types
- Task assigned to you
- Task status changed
- Comment added to your task
- Task due date approaching
- Project updated

---

## 🔒 Security & Authorization Testing

### Test SQL Injection
Try malicious inputs:
```sql
-- In login email field:
admin' OR '1'='1'--
admin@example.com'; DROP TABLE Users;--

-- In search fields:
'; DELETE FROM Tasks WHERE '1'='1
```

**✅ Expected Result:**
- All blocked by parameterized queries
- No database damage
- Invalid input error

### Test XSS (Cross-Site Scripting)
Try in task title/description:
```html
<script>alert('XSS')</script>
<img src=x onerror=alert('XSS')>
```

**✅ Expected Result:**
- HTML encoded/sanitized
- No script execution
- Displays as text

### Test CSRF Protection
- All POST/PUT/DELETE should have CORS configured
- JWT token required in Authorization header

### Test Rate Limiting
1. Try to login with wrong password 10 times quickly
2. Should get rate limited after 5 attempts

**✅ Expected:**
```json
{
  "message": "Too many requests. Please try again later.",
  "statusCode": 429
}
```

---

## ⚡ Performance Testing

### Load Testing
1. Create 100+ projects
2. Create 500+ tasks
3. Test:
   - Page load time < 2 seconds
   - Search results instant
   - Smooth scrolling
   - No memory leaks

### API Response Times
Check in Network tab:
- GET `/api/projects` < 200ms
- GET `/api/tasks` < 300ms
- POST `/api/projects` < 500ms
- Dashboard stats < 500ms

---

## 🐛 Common Issues & Solutions

### Issue 1: CORS Error
```
Access to fetch at 'https://localhost:7125/api/...' blocked by CORS policy
```

**Solution:**
- Check `Program.cs` has CORS configured
- Ensure Angular URL in allowed origins: `http://localhost:4200`

### Issue 2: 401 Unauthorized
```
GET /api/projects 401 Unauthorized
```

**Solution:**
- Check JWT token in localStorage
- Token might be expired
- Try logout and login again

### Issue 3: 400 Bad Request (Validation Error)
```json
{
  "errors": ["Description is required"]
}
```

**Solution:**
- Check form validation matches backend
- Ensure required fields filled
- Check data types match

### Issue 4: SignalR Connection Failed
```
WebSocket connection to 'wss://localhost:7125/hubs/notifications' failed
```

**Solution:**
- Check SignalR hub registered in `Program.cs`
- Ensure token passed in query string
- Check browser console for details

---

## 📊 Test Coverage Checklist

### Authentication ✅
- [x] Register new user
- [x] Login with valid credentials
- [x] Login with invalid credentials
- [x] Logout
- [x] Token refresh
- [x] Password validation
- [x] Email validation

### Projects ✅
- [x] Create project
- [x] Edit project
- [x] Delete project
- [x] View project details
- [x] Search projects
- [x] Filter projects
- [x] Sort projects
- [x] Add project member
- [x] Remove project member

### Tasks ✅
- [x] Create task
- [x] Edit task
- [x] Delete task
- [x] Change task status
- [x] Change task priority
- [x] Assign task
- [x] Set due date
- [x] View task details
- [x] Kanban board drag-drop
- [x] Task list view
- [x] Task filters
- [x] Task search

### Dashboard ✅
- [x] View statistics
- [x] View charts
- [x] Recent activity
- [x] Project progress

### AI Feature ✅
- [x] Enhance task description
- [x] Handle AI errors

### Comments & Activity ✅
- [x] Add comment
- [x] View comments
- [x] View activity logs

### Security ✅
- [x] Authorization checks
- [x] Input validation
- [x] SQL injection prevention
- [x] XSS prevention
- [x] Rate limiting

---

## 🎓 Testing Best Practices

1. **Test as Different Users:** Admin, ProjectManager, TeamMember
2. **Test Edge Cases:** Empty inputs, max lengths, special characters
3. **Test Error Scenarios:** Network errors, server errors, validation failures
4. **Test Responsive Design:** Mobile, tablet, desktop
5. **Test Browser Compatibility:** Chrome, Firefox, Edge
6. **Test Performance:** Large datasets, slow networks
7. **Test Accessibility:** Keyboard navigation, screen readers

---

## 📝 Bug Reporting Template

When you find a bug, report it with:

```markdown
**Title:** Cannot create project with special characters in name

**Steps to Reproduce:**
1. Go to Projects page
2. Click "New Project"
3. Enter name: "E-Commerce & Retail"
4. Fill description
5. Click "Create Project"

**Expected Result:**
Project created successfully

**Actual Result:**
Error: "Invalid characters in project name"

**Environment:**
- Browser: Chrome 120
- OS: Windows 11
- Backend: ASP.NET Core 10
- Frontend: Angular 22

**Screenshots:**
[Attach screenshot]

**Console Errors:**
[Paste console logs]
```

---

## 🚀 Ready to Test!

Follow this guide systematically to test your entire application. Good luck! 🎉
