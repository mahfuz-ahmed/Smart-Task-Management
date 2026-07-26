# GitHub Push Guide

**Project**: Smart Task Management System  
**Date**: July 26, 2026

---

## 📋 Pre-Push Checklist

Before pushing to GitHub, ensure:

- [ ] All code is working and tested
- [ ] README.md is complete
- [ ] PROMPTS.md documents AI usage
- [ ] API collection files are included
- [ ] No sensitive data (passwords, tokens) in code
- [ ] .gitignore is properly configured
- [ ] All documentation is up to date

---

## 🔧 Step-by-Step Instructions

### Step 1: Check Git Status

```bash
cd d:\Task\DataVancedBDLtd\Smart-Task-Management
git status
```

This shows what files are tracked/untracked.

---

### Step 2: Review .gitignore

Ensure these are in your `.gitignore`:

```
# Backend
**/bin/
**/obj/
**/Logs/
*.user
*.suo
appsettings.Development.json

# Frontend
node_modules/
dist/
.angular/

# IDE
.vs/
.vscode/
*.swp

# OS
.DS_Store
Thumbs.db

# Sensitive
*.env
**/appsettings.json
```

**⚠️ IMPORTANT**: Remove or mask sensitive data from `appsettings.json` before committing!

---

### Step 3: Initialize Git (if not already done)

```bash
git init
```

---

### Step 4: Add Remote Repository

**Option A: Create New Repo on GitHub First**

1. Go to https://github.com
2. Click "New repository"
3. Name it: `Smart-Task-Management`
4. Don't initialize with README (we already have one)
5. Click "Create repository"

**Option B: Use Existing Repo**

If you already have a repo, skip to Step 5.

**Then add remote:**

```bash
git remote add origin https://github.com/YOUR_USERNAME/Smart-Task-Management.git
```

Replace `YOUR_USERNAME` with your actual GitHub username.

---

### Step 5: Stage Files

**Add all files:**
```bash
git add .
```

**Or add specific files:**
```bash
git add README.md
git add PROMPTS.md
git add API_COLLECTION_GUIDE.md
git add backend/
git add frontend/
```

**Check what's staged:**
```bash
git status
```

---

### Step 6: Commit Changes

```bash
git commit -m "Initial commit: Smart Task Management System"
```

**Or more detailed:**
```bash
git commit -m "Initial commit: Smart Task Management System

Features:
- N-Layer Architecture with .NET 10 and Angular 22
- Hybrid RBAC (System + Project roles)
- JWT authentication with refresh tokens
- AI-powered task enhancement (GitHub Models)
- Complete API with Swagger documentation
- Responsive Angular frontend
- Comprehensive documentation"
```

---

### Step 7: Push to GitHub

**First push:**
```bash
git branch -M main
git push -u origin main
```

**Subsequent pushes:**
```bash
git push
```

---

### Step 8: Verify on GitHub

1. Go to your GitHub repository URL
2. Check that all files are visible
3. Verify README.md displays correctly
4. Check that .gitignore worked (no bin/, obj/, node_modules/)

---

## ⚠️ CRITICAL: Remove Sensitive Data

### Before Pushing, Update These Files:

#### 1. appsettings.json

**Current:**
```json
{
  "AiSettings": {
    "GitHubToken": "YOUR_GITHUB_MODELS_TOKEN_HERE"
  }
}
```

**Change to:**
```json
{
  "AiSettings": {
    "GitHubToken": "YOUR_GITHUB_MODELS_TOKEN_HERE"
  }
}
```

#### 2. Postman Environment

**Current:**
```json
{
  "key": "accessToken",
  "value": "actual-token-value"
}
```

**Change to:**
```json
{
  "key": "accessToken",
  "value": ""
}
```

---

## 🛡️ Security Best Practices

### What NOT to Push:

❌ **Never commit:**
- Real API keys or tokens
- Database connection strings with production credentials
- Passwords
- Private keys
- User data
- .env files with secrets

### Use Environment Variables:

**For Production:**
Create environment variables instead:
```bash
export GITHUB_TOKEN=your_actual_token
export DB_CONNECTION=your_actual_connection
```

**Document in README:**
```markdown
## Environment Variables

Create a `.env` file (not committed) with:
```
GITHUB_TOKEN=your_token_here
DB_CONNECTION=your_connection_string
```
```

---

## 📝 Good Commit Messages

### Format:
```
<type>: <subject>

<body>

<footer>
```

### Examples:

```bash
git commit -m "feat: Add AI-powered task description enhancement"
git commit -m "fix: Resolve port configuration issue for PowerShell"
git commit -m "docs: Update README with complete setup instructions"
git commit -m "refactor: Improve authorization helper logic"
git commit -m "test: Add unit tests for TaskService"
```

### Types:
- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Formatting, missing semi-colons, etc.
- **refactor**: Code refactoring
- **test**: Adding tests
- **chore**: Maintain, dependencies, etc.

---

## 🔄 Regular Git Workflow

### Daily Workflow:

**1. Check status:**
```bash
git status
```

**2. Pull latest changes:**
```bash
git pull origin main
```

**3. Make your changes...**

**4. Stage changes:**
```bash
git add .
```

**5. Commit:**
```bash
git commit -m "Your message"
```

**6. Push:**
```bash
git push
```

---

## 🌿 Branching Strategy

### For Features:

```bash
# Create and switch to new branch
git checkout -b feature/add-email-notifications

# Make changes...
# Commit changes...

# Push branch
git push -u origin feature/add-email-notifications

# Create Pull Request on GitHub
# Merge after review
```

### For Bug Fixes:

```bash
git checkout -b fix/login-redirect-issue
# Fix the bug...
git add .
git commit -m "fix: Correct login redirect logic"
git push -u origin fix/login-redirect-issue
```

---

## 🔍 Useful Git Commands

### Check remote:
```bash
git remote -v
```

### View commit history:
```bash
git log --oneline
```

### View changes:
```bash
git diff
```

### Undo uncommitted changes:
```bash
git checkout -- filename
```

### Remove file from staging:
```bash
git reset HEAD filename
```

### Amend last commit:
```bash
git commit --amend -m "New message"
```

---

## 🚫 Common Issues & Solutions

### Issue: "Repository not found"
**Solution:**
```bash
git remote set-url origin https://github.com/YOUR_CORRECT_USERNAME/Smart-Task-Management.git
```

### Issue: "Permission denied"
**Solution:**
1. Check your GitHub credentials
2. Use HTTPS with personal access token
3. Or set up SSH keys

### Issue: "Merge conflict"
**Solution:**
```bash
git pull origin main
# Resolve conflicts in files
git add .
git commit -m "Resolve merge conflicts"
git push
```

### Issue: "Large files warning"
**Solution:**
Don't commit:
- node_modules/
- bin/, obj/
- Large binaries

Add to .gitignore and:
```bash
git rm -r --cached node_modules/
git commit -m "Remove node_modules from tracking"
```

---

## 📊 Repository Structure on GitHub

Your repository will look like:

```
Smart-Task-Management/
├── .github/               # GitHub-specific files
├── backend/               # Backend source code
├── frontend/              # Frontend source code
├── README.md              # Main documentation
├── PROMPTS.md            # AI prompts documentation
├── API_COLLECTION_GUIDE.md
├── PROJECT_OVERVIEW.md
├── ARCHITECTURE_DECISIONS.md
├── FOR_INTERVIEWER.md
├── SmartTaskManagement.postman_collection.json
├── SmartTaskManagement.postman_environment.json
├── .gitignore            # Git ignore rules
└── LICENSE               # License file (optional)
```

---

## ✅ Final Checklist Before Push

Before your first push, verify:

- [ ] Sensitive data removed/masked
- [ ] .gitignore is correct
- [ ] README.md is complete
- [ ] All documentation files included
- [ ] Code compiles and runs
- [ ] Tests pass (if any)
- [ ] Commit message is clear
- [ ] Remote URL is correct

---

## 🎓 For Submission

After pushing:

1. **Get Repository URL:**
   ```
   https://github.com/YOUR_USERNAME/Smart-Task-Management
   ```

2. **Share with Interviewer:**
   - Repository URL
   - README.md (visible on repo page)
   - All documentation files

3. **Ensure Public Repo** (if required):
   - Go to Settings → General
   - Make repository public (if currently private)

---

## 📞 Need Help?

If you encounter issues:

1. Check GitHub documentation: https://docs.github.com
2. Git documentation: https://git-scm.com/doc
3. Ask on Stack Overflow with the `git` tag

---

**Ready to push? Let's do it!** 🚀

