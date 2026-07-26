# 🔧 Troubleshooting Guide - Feature Not Working Issues

## 🚨 Current Issue Analysis

You're experiencing the following issues:
- ❌ Cannot create project
- ❌ Cannot view project details
- ❌ Cannot create task
- ❌ Cannot move task on Kanban board
- ❌ Cannot add comment to task
- ❌ Cannot complete task
- ❌ Activity logs not showing

## 🔍 Root Cause Investigation

### Issue 1: Backend Already Running (File Locked)
```
Error: The process cannot access the file 'SmartTaskManagement.API.dll' 
because it is being used by another process.
Locked by: SmartTaskManagement.API (16260), Visual Studio Insiders (13612)
```

**This means:**
- Backend API is already running (process ID 16260)
- Visual Studio has locked the files
- You cannot rebuild while it's running

---

## 🛠️ STEP-BY-STEP FIX GUIDE

### ✅ Step 1: Stop All Running Processes

#### Option A: Stop from Visual Studio
1. If Visual Studio is open, press `Shift + F5` or click the red ⏹️ **Stop** button
2. Close Visual Studio completely

#### Option B: Stop from Task Manager
```bash
# Open PowerShell as Administrator
Get-Process -Name "SmartTaskManagement.API" | Stop-Process -Force
Get-Process -Name "dotnet" | Where-Object {$_.MainWindowTitle -like "*SmartTask*"} | Stop-Process -Force
```

#### Option C: Manually Kill Process
1. Press `Ctrl + Shift + Esc` (Task Manager)
2. Go to **Details** tab
3. Find `SmartTaskManagement.API.exe` or `dotnet.exe`
4. Right-click → **End Task**
5. Repeat for all related processes

---

### ✅ Step 2: Check if Backend is Actually Running

Open your browser and try:
- **Swagger UI:** `https://localhost:7125/swagger`
- **Health Check:** `https://localhost:7125/health`

**If you see Swagger UI or "Healthy" response:**
✅ Backend is running - Skip to Step 4

**If you see "Site can't be reached" or connection error:**
❌ Backend is NOT running - Go to Step 3

---

### ✅ Step 3: Start Backend Properly

```powershell
# Navigate to API directory
cd backend/SmartTaskManagement/src/SmartTaskManagement.API

# Run the API
dotnet run

# You should see:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7125
#       Now listening on: http://localhost:5012
```

**Wait for these logs:**
```
info: Smart Task Management API starting on https://localhost:7125
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

✅ Backend is now running!

**Keep this terminal/window OPEN** - don't close it!

---

### ✅ Step 4: Check Frontend is Running

Open **NEW** terminal/PowerShell window:

```powershell
cd frontend
npm start

# OR if not installed:
npm install
npm start
```

**Wait for:**
```
** Angular Live Development Server is listening on localhost:4200 **
✔ Compiled successfully.
```

✅ Frontend is now running!

Open browser: `http://localhost:4200`

---

### ✅ Step 5: Test Backend API with Swagger

1. Open `https://localhost:7125/swagger` in browser
2. You should see Swagger UI with all API endpoints
3. Test **Auth** endpoints first:

#### Test 1: Register User
1. Expand `POST /api/auth/register`
2. Click **"Try it out"**
3. Paste this JSON:
```json
{
  "firstName": "Test",
  "lastName": "User",
  "email": "test@example.com",
  "password": "Test@1234",
  "confirmPassword": "Test@1234",
  "role": "Admin"
}
```
4. Click **"Execute"**
5. You should get **201 Created** response with:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresAt": "2026-07-23T...",
    "user": {
      "id": "...",
      "firstName": "Test",
      "lastName": "User",
      "email": "test@example.com",
      "role": "Admin"
    }
  },
  "message": "Registration successful."
}
```

#### Test 2: Login
1. Expand `POST /api/auth/login`
2. Click **"Try it out"**
3. Paste:
```json
{
  "email": "test@example.com",
  "password": "Test@1234"
}
```
4. Click **"Execute"**
5. **Copy the `accessToken`** from response

#### Test 3: Create Project (Authorized)
1. Click **"Authorize"** button (top right, 🔓 icon)
2. In popup, type: `Bearer <your-access-token>`
   - Example: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
3. Click **"Authorize"** then **"Close"**
4. Now 🔓 icon should show 🔒 (locked = authorized)
5. Expand `POST /api/projects`
6. Click **"Try it out"**
7. Paste:
```json
{
  "name": "Test Project",
  "description": "This is a test project created from Swagger",
  "status": "Active",
  "priority": "High",
  "startDate": "2026-07-23",
  "endDate": "2026-12-31"
}
```
8. Click **"Execute"**

**✅ Expected:** 201 Created with project data  
**❌ If 401 Unauthorized:** Token expired or not set  
**❌ If 400 Bad Request:** Check validation errors

---

### ✅ Step 6: Check Browser Console for Frontend Errors

1. Open frontend: `http://localhost:4200`
2. Press `F12` (Developer Tools)
3. Go to **Console** tab
4. Try to register/login

**Look for errors:**

#### Error 1: CORS Error
```
Access to fetch at 'https://localhost:7125/api/auth/login' from origin 'http://localhost:4200' 
has been blocked by CORS policy
```

**Fix:** Check backend `Program.cs`:
```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// AND later:
app.UseCors("AllowAngular");
```

#### Error 2: Net::ERR_CONNECTION_REFUSED
```
GET https://localhost:7125/api/projects net::ERR_CONNECTION_REFUSED
```

**Fix:** Backend is not running! Go back to Step 3.

#### Error 3: 401 Unauthorized
```
GET https://localhost:7125/api/projects 401 (Unauthorized)
```

**Fix:** 
- Token expired or not sent
- Logout and login again
- Check localStorage has `authToken`

#### Error 4: 400 Bad Request with Validation Errors
```json
{
  "success": false,
  "errors": ["Description is required"]
}
```

**Fix:** Frontend validation not matching backend - Already fixed in previous update!

---

### ✅ Step 7: Test Frontend Features One by One

#### Test 1: Registration ✅
```
1. Go to http://localhost:4200
2. Click "Sign Up" or "Register"
3. Fill form:
   - First Name: John
   - Last Name: Doe
   - Email: john.doe@test.com
   - Password: Test@1234
   - Confirm Password: Test@1234
   - Role: Admin
4. Click "Register"
```

**Expected:**
- ✅ Success toast: "Registration successful"
- ✅ Auto-redirect to Dashboard
- ✅ User name shown in header

**If fails:**
- Check browser console for errors
- Check Network tab (F12 → Network) for API call
- Check backend terminal for errors

#### Test 2: Create Project ✅
```
1. Click "Projects" from sidebar
2. Click "New Project" button
3. Fill form:
   - Name: My First Project
   - Description: This is my first test project (min 1 char required)
   - Status: Active
   - Priority: High
4. Click "Create Project"
```

**Expected:**
- ✅ Form validation shows if fields empty
- ✅ Character counter updates: 0/500
- ✅ Success toast: "Project created! My First Project"
- ✅ Modal closes
- ✅ Project appears in list

**Debug:**
```javascript
// Check in browser console
localStorage.getItem('authToken') // Should return token
```

#### Test 3: Create Task ✅
```
1. Click on project card
2. In project details, click "+ New Task"
3. Fill form:
   - Title: My First Task
   - Description: Task description here
   - Status: ToDo
   - Priority: High
   - Assigned To: (select yourself)
   - Due Date: Tomorrow
4. Click "Create Task"
```

**Expected:**
- ✅ Task appears in Kanban board under "To Do"
- ✅ Task card shows all info
- ✅ Success toast

#### Test 4: Move Task on Kanban ✅
```
1. Drag task card from "To Do" column
2. Drop it in "In Progress" column
```

**OR click-based:**
```
1. Click on task card
2. Change status dropdown to "InProgress"
3. Click "Save"
```

**Expected:**
- ✅ Task moves to new column
- ✅ Column counts update
- ✅ Dashboard updates

---

## 🔍 Advanced Debugging

### Check Database Connection

```powershell
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
dotnet ef database update
```

**Expected:**
```
Build succeeded.
Applying migration...
Done.
```

**If fails:**
- Check SQL Server is running
- Check connection string in `appsettings.json`

### Check Migrations

```powershell
cd backend/SmartTaskManagement/src/SmartTaskManagement.Infrastructure
dotnet ef migrations list --startup-project ../SmartTaskManagement.API
```

Should show list of migrations.

### Re-create Database (if corrupted)

```powershell
# Drop database
dotnet ef database drop --startup-project ../SmartTaskManagement.API

# Re-create
dotnet ef database update --startup-project ../SmartTaskManagement.API
```

---

## 📋 Checklist Before Testing

- [ ] Visual Studio closed (no file locks)
- [ ] All old dotnet processes killed
- [ ] Backend running on https://localhost:7125
- [ ] Swagger UI accessible at https://localhost:7125/swagger
- [ ] Frontend running on http://localhost:4200
- [ ] Browser console open (F12)
- [ ] Network tab open to see API calls
- [ ] SQL Server running
- [ ] Database exists and has tables

---

## 🚀 Quick Start Script

Save this as `start-app.ps1`:

```powershell
# Kill old processes
Get-Process -Name "SmartTaskManagement.API" -ErrorAction SilentlyContinue | Stop-Process -Force
Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {$_.MainWindowTitle -like "*Angular*"} | Stop-Process -Force

# Start Backend (in new window)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\backend\SmartTaskManagement\src\SmartTaskManagement.API'; dotnet run"

# Wait for backend
Start-Sleep -Seconds 10

# Start Frontend (in new window)
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\frontend'; npm start"

Write-Host "✅ Backend starting at https://localhost:7125"
Write-Host "✅ Frontend starting at http://localhost:4200"
Write-Host "⏳ Wait 30 seconds then open http://localhost:4200"
```

Run it:
```powershell
.\start-app.ps1
```

---

## 🐛 Common Specific Issues & Fixes

### Issue: "Cannot create project"

**Symptom:** Click "Create Project", nothing happens or shows error

**Diagnosis:**
1. Open browser console (F12)
2. Check for errors
3. Go to Network tab
4. Try creating project again
5. Look for POST request to `/api/projects`

**Possible Causes:**

#### A. Validation Error (400 Bad Request)
- **Check:** Response shows "Description is required"
- **Fix:** Already fixed! Pull latest changes or ensure Description field has validation

#### B. Unauthorized (401)
- **Check:** Response shows 401 status
- **Fix:** Logout and login again (token expired)

#### C. Backend not running
- **Check:** Request shows "ERR_CONNECTION_REFUSED"
- **Fix:** Start backend with `dotnet run`

#### D. CORS error
- **Check:** Console shows CORS policy error
- **Fix:** Check CORS configuration in `Program.cs`

### Issue: "Cannot view project details"

**Symptom:** Click project card, nothing happens

**Check:**
1. Is project ID valid?
2. Check router configuration in `app.routes.ts`
3. Check if route `/projects/:id` exists

**Fix:**
```typescript
// In app.routes.ts
{
  path: 'projects/:id',
  component: ProjectDetailComponent,
  canActivate: [authGuard]
}
```

### Issue: "Kanban board not working"

**Symptom:** Cannot drag tasks or change status

**Check:**
1. Are tasks loading?
2. Check browser console for errors
3. Check if task status enum matches backend

**Debug:**
```typescript
// In component, check:
console.log('Tasks loaded:', this.tasks());
console.log('Task statuses:', this.tasks().map(t => t.status));
```

### Issue: "Activity logs not showing"

**Check:**
1. Backend API `/api/tasks/{id}/activity` endpoint
2. Frontend service calling correct endpoint
3. Response data structure matches frontend model

---

## 📝 Testing Checklist (Copy & Paste)

```markdown
### Backend Testing (Swagger)
- [ ] Swagger UI loads at https://localhost:7125/swagger
- [ ] Can register new user (201 Created)
- [ ] Can login with user (200 OK)
- [ ] Access token received
- [ ] Can authorize in Swagger with token
- [ ] Can create project (201 Created)
- [ ] Can get projects list (200 OK)
- [ ] Can create task in project (201 Created)
- [ ] Can update task status (200 OK)

### Frontend Testing
- [ ] App loads at http://localhost:4200
- [ ] Register page loads
- [ ] Can register new user
- [ ] Auto-redirects to dashboard
- [ ] Dashboard shows stats
- [ ] Can navigate to Projects page
- [ ] "New Project" button works
- [ ] Project creation form opens
- [ ] Form validation works (required fields)
- [ ] Character counter updates
- [ ] Date validation works (end > start)
- [ ] Can create project successfully
- [ ] Project appears in list
- [ ] Can click project to view details
- [ ] Can create task in project
- [ ] Task appears in Kanban board
- [ ] Can drag task between columns
- [ ] Can change task status via dropdown
- [ ] Can add comment to task
- [ ] Activity log shows changes
- [ ] Search works
- [ ] Filters work

### Browser Console
- [ ] No errors in console
- [ ] API calls successful (200, 201)
- [ ] No CORS errors
- [ ] Token stored in localStorage
```

---

## 🆘 Still Not Working?

### Get Full Diagnostic Info

Run this in PowerShell:

```powershell
Write-Host "=== BACKEND STATUS ===" -ForegroundColor Cyan
$backendProcess = Get-Process -Name "SmartTaskManagement.API" -ErrorAction SilentlyContinue
if ($backendProcess) {
    Write-Host "✅ Backend is RUNNING (PID: $($backendProcess.Id))" -ForegroundColor Green
} else {
    Write-Host "❌ Backend is NOT running" -ForegroundColor Red
}

Write-Host "`n=== FRONTEND STATUS ===" -ForegroundColor Cyan
$nodeProcess = Get-Process -Name "node" -ErrorAction SilentlyContinue
if ($nodeProcess) {
    Write-Host "✅ Node process found (might be frontend)" -ForegroundColor Green
} else {
    Write-Host "❌ No Node process running" -ForegroundColor Red
}

Write-Host "`n=== PORT STATUS ===" -ForegroundColor Cyan
$backendPort = netstat -ano | findstr ":7125"
if ($backendPort) {
    Write-Host "✅ Port 7125 is in use (backend)" -ForegroundColor Green
    Write-Host $backendPort
} else {
    Write-Host "❌ Port 7125 is free (backend not listening)" -ForegroundColor Red
}

$frontendPort = netstat -ano | findstr ":4200"
if ($frontendPort) {
    Write-Host "✅ Port 4200 is in use (frontend)" -ForegroundColor Green
    Write-Host $frontendPort
} else {
    Write-Host "❌ Port 4200 is free (frontend not listening)" -ForegroundColor Red
}

Write-Host "`n=== DATABASE ===" -ForegroundColor Cyan
$sqlProcess = Get-Process -Name "sqlservr" -ErrorAction SilentlyContinue
if ($sqlProcess) {
    Write-Host "✅ SQL Server is running" -ForegroundColor Green
} else {
    Write-Host "⚠️  SQL Server process not found" -ForegroundColor Yellow
}

Write-Host "`n=== NEXT STEPS ===" -ForegroundColor Cyan
if (-not $backendProcess) {
    Write-Host "1. Start backend: cd backend\SmartTaskManagement\src\SmartTaskManagement.API; dotnet run"
}
if (-not $nodeProcess) {
    Write-Host "2. Start frontend: cd frontend; npm start"
}
Write-Host "3. Open http://localhost:4200 in browser"
Write-Host "4. Press F12 and check Console for errors"
```

Save screenshot and share output!

---

## 📧 Need More Help?

Provide this info:
1. Output of diagnostic script above
2. Screenshot of browser console (F12)
3. Screenshot of Network tab showing failed API call
4. Backend terminal logs
5. Frontend terminal logs

---

**Remember:** Most issues are caused by:
1. ❌ Backend not running
2. ❌ Token expired (logout/login fixes)
3. ❌ CORS not configured
4. ❌ Validation mismatch (already fixed!)

Follow steps 1-7 carefully and you'll be up and running! 🚀
