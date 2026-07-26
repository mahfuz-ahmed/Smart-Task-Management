# Configuration Guide

This document explains how to configure the application with your actual credentials after cloning from GitHub.

---

## 🔧 Backend Configuration

### appsettings.json

**Location**: `backend/SmartTaskManagement/src/SmartTaskManagement.API/appsettings.json`

**Update these values:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SmartTaskManagementDb;Integrated Security=true;Encrypt=false;MultipleActiveResultSets=True;Connection Timeout=30;"
  },
  "JwtSettings": {
    "SecretKey": "YOUR_JWT_SECRET_KEY_HERE_MINIMUM_64_CHARACTERS_REQUIRED",
    "Issuer": "SmartTaskManagement",
    "Audience": "SmartTaskManagementClient",
    "ExpiryMinutes": 15
  },
  "AiSettings": {
    "GitHubToken": "YOUR_GITHUB_MODELS_TOKEN_HERE"
  }
}
```

### Configuration Steps:

**1. Database Connection String**

Replace `YOUR_SERVER` with your SQL Server instance name:
- **LocalDB**: `(localdb)\\MSSQLLocalDB`
- **SQL Server Express**: `.\\SQLEXPRESS`
- **Full SQL Server**: `YOUR_MACHINE_NAME` or `localhost`

Example:
```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SmartTaskManagementDb;Integrated Security=true;Encrypt=false;"
```

**2. JWT Secret Key**

Generate a secure random string (minimum 64 characters):

**PowerShell:**
```powershell
-join ((65..90) + (97..122) + (48..57) + (33..47) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

**Or use online generator**: https://randomkeygen.com/

Example:
```json
"SecretKey": "Abc123!@#Xyz789$%^Def456&*()Ghi012+=-Jkl345_[]Mno678~`{}Pqr901|"
```

**3. GitHub Models Token**

Get your token from GitHub:
1. Go to https://github.com/settings/tokens
2. Click "Generate new token (classic)"
3. Select scopes: `repo`, `read:org`
4. Click "Generate token"
5. Copy the token (starts with `ghp_`)

Example:
```json
"GitHubToken": "ghp_1234567890abcdefghijklmnopqrstuvwxyz"
```

---

## 🔐 Security Best Practices

### Option 1: User Secrets (Recommended for Development)

**Initialize user secrets:**
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
dotnet user-secrets init
```

**Set secrets:**
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=SmartTaskManagementDb;..."
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_SECRET_KEY"
dotnet user-secrets set "AiSettings:GitHubToken" "YOUR_GITHUB_TOKEN"
```

**User secrets are stored outside the project directory and never committed to Git.**

### Option 2: Environment Variables (Recommended for Production)

**Set environment variables:**

**Windows (PowerShell):**
```powershell
$env:ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;..."
$env:JwtSettings__SecretKey="YOUR_SECRET_KEY"
$env:AiSettings__GitHubToken="YOUR_GITHUB_TOKEN"
```

**Linux/Mac:**
```bash
export ConnectionStrings__DefaultConnection="Server=YOUR_SERVER;..."
export JwtSettings__SecretKey="YOUR_SECRET_KEY"
export AiSettings__GitHubToken="YOUR_GITHUB_TOKEN"
```

### Option 3: appsettings.Development.json (Local Only)

**Create**: `appsettings.Development.json` (already in .gitignore)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_ACTUAL_SERVER;..."
  },
  "JwtSettings": {
    "SecretKey": "YOUR_ACTUAL_SECRET"
  },
  "AiSettings": {
    "GitHubToken": "YOUR_ACTUAL_TOKEN"
  }
}
```

This file overrides `appsettings.json` in Development environment and is **NOT committed to GitHub**.

---

## 📝 Configuration Priority

ASP.NET Core reads configuration in this order (later overrides earlier):

1. `appsettings.json` (committed, no secrets)
2. `appsettings.{Environment}.json` (not committed, has secrets)
3. User Secrets (Development only)
4. Environment Variables (all environments)
5. Command-line arguments

---

## ✅ Verification

After configuration, verify it works:

**1. Build:**
```bash
cd backend/SmartTaskManagement/src/SmartTaskManagement.API
dotnet build
```

**2. Apply Migrations:**
```bash
dotnet ef database update
```

**3. Run:**
```bash
dotnet run
```

**4. Test:**
- Open: `https://localhost:7125/swagger`
- Try: Login endpoint with `admin@task.com` / `Admin@123`
- Should return tokens ✅

---

## 🎯 Production Configuration

For production deployment:

**Azure App Service:**
- Use Application Settings (Configuration blade)
- Set as connection strings and app settings
- Automatically maps to configuration

**Docker:**
- Use environment variables in docker-compose.yml
- Or use .env file (not committed)

**Kubernetes:**
- Use Secrets and ConfigMaps
- Mount as environment variables

---

## 📞 Troubleshooting

### Issue: "Connection string not found"
- Check `appsettings.json` format
- Verify environment variables syntax (double underscore `__`)
- Check file name: `appsettings.Development.json`

### Issue: "Invalid JWT secret"
- Must be at least 64 characters
- Use complex characters (letters, numbers, symbols)

### Issue: "GitHub API unauthorized"
- Check token is valid (not expired)
- Verify token has correct permissions
- Token should start with `ghp_`

---

## 🔒 Never Commit:

❌ **Real database connection strings**  
❌ **Production JWT secrets**  
❌ **API keys and tokens**  
❌ **Passwords**  
❌ **Private keys**

✅ **Always use placeholders in appsettings.json**  
✅ **Use User Secrets or Environment Variables**  
✅ **Document configuration in this file**

---

**Keep your secrets safe!** 🔐

