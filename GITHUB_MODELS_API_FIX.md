# GitHub Models API Fix

## Issue
The application was experiencing connection failures when trying to use the GitHub Models AI service. The error was:
```
System.Net.Http.HttpRequestException: No such host is known. (models.inference.ai.azure.com:443)
System.Net.Sockets.SocketException (11001): No such host is known.
```

## Root Cause
The Azure endpoint `https://models.inference.ai.azure.com` was **deprecated on July 17, 2025**. 

According to [GitHub's changelog](https://github.blog/changelog/2025-07-17-deprecation-of-azure-endpoint-for-github-models/):
> "This change follows the launch of the GitHub Models API on May 15, 2025, which offers a fully supported, billable, and enterprise-ready way to access GitHub-hosted models through https://models.github.ai."

## Changes Made

### 1. Updated API Endpoint
**File**: `backend/SmartTaskManagement/src/SmartTaskManagement.Infrastructure/Services/GitHubModelsAiService.cs`

**Before**:
```csharp
private const string Endpoint = "https://models.inference.ai.azure.com/chat/completions";
private const string Model    = "gpt-4o-mini";
```

**After**:
```csharp
private const string Endpoint = "https://models.github.ai/inference/chat/completions";
private const string Model    = "openai/gpt-4o-mini";
```

### 2. Model ID Format
GitHub Models requires the model ID to be in the format `{publisher}/{model_name}`. For OpenAI models, the prefix `openai/` must be added.

## References
- [GitHub Models API Documentation](https://docs.github.com/en/rest/models/inference)
- [GitHub Models Quickstart](https://docs.github.com/en/github-models/quickstart)
- [Deprecation Announcement](https://github.blog/changelog/2025-07-17-deprecation-of-azure-endpoint-for-github-models/)

## Next Steps
1. Rebuild the application
2. Test the AI service with the new endpoint
3. Verify that the GitHub token in `appsettings.json` is valid and has proper permissions
4. Monitor the logs for successful API calls

## Testing
To test the AI service:
1. Start the application
2. Create or edit a task
3. Use the "Improve Description" feature
4. Check the logs for successful API calls or any new error messages

## Token Configuration
The GitHub token is configured in `appsettings.json`:
```json
"AiSettings": {
  "GitHubToken": "YOUR_GITHUB_MODELS_TOKEN_HERE"
}
```

**Important**: Ensure this token:
- Is valid and not expired
- Has the necessary permissions for GitHub Models API
- Is kept secure and not committed to public repositories
