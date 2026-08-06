using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.Interfaces.ExternalServices;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SmartTaskManagement.Infrastructure.Services;

public sealed class GitHubModelsAiService : IAiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GitHubModelsAiService> _logger;

    private const string Endpoint = "https://models.github.ai/inference/chat/completions";
    private const string Model = "openai/gpt-4o-mini";

    public GitHubModelsAiService(HttpClient http, IConfiguration config, ILogger<GitHubModelsAiService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string> ImproveDescriptionAsync(
        string description, string? taskTitle = null, CancellationToken ct = default)
    {
        var token = _config["AiSettings:GitHubToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("GitHub token not configured. Returning fallback enhancement.");
            return BuildFallbackImprovement(description, taskTitle);
        }

        var system = """
            You are a professional technical writer for software project management.
            When given a task description, you must:
            1. Correct grammar and spelling.
            2. Improve clarity and readability.
            3. Make descriptions more professional.
            4. Expand short or vague descriptions with relevant detail.
            5. Produce actionable task descriptions using imperative language.
            Return ONLY the improved task description. Do not add explanations, lists, headers, or comments.
            """;

        //var system = """
        //You are an expert code reviewer. When reviewing code:
        //1. Identify bugs and potential issues
        //2. Suggest performance improvements
        //3. Check for security vulnerabilities
        //4. Recommend better practices
        //5. Provide actionable feedback

        //Format your response clearly with sections for: Issues, Improvements, Security, Best Practices.
        //""";

        var user = taskTitle != null
            ? $"Task Title: {taskTitle}\n\nDescription: {description}"
            : $"Description: {description}";

        var body = new
        {
            model = Model,
            messages = new[]
            {
                new { role = "system", content = system },
                new { role = "user",   content = user }
            },
            temperature = 0.7,
            max_tokens = 500
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        request.Headers.Add("Authorization", $"Bearer {token}");
        request.Headers.Add("Accept", "application/vnd.github+json");
        request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        request.Content = JsonContent.Create(body);

        try
        {
            var response = await _http.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogError(
                    "GitHub Models API returned {StatusCode}. Response: {ErrorContent}",
                    response.StatusCode,
                    errorContent);
                return BuildFallbackImprovement(description, taskTitle);
            }

            var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: ct);
            var improved = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            return string.IsNullOrWhiteSpace(improved)
                ? BuildFallbackImprovement(description, taskTitle)
                : improved;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI call failed. Returning fallback enhancement.");
            return BuildFallbackImprovement(description, taskTitle);
        }
    }

    private static string BuildFallbackImprovement(string description, string? taskTitle)
    {
        if (string.IsNullOrWhiteSpace(description)) return description;

        var baseText = description.Trim().Replace("\\s+", " ", StringComparison.Ordinal);
        var result = char.ToUpperInvariant(baseText[0]) + baseText[1..];

        if (!result.EndsWith(".", StringComparison.Ordinal) &&
            !result.EndsWith("!", StringComparison.Ordinal) &&
            !result.EndsWith("?", StringComparison.Ordinal))
        {
            result += ".";
        }

        if (taskTitle is { Length: > 0 })
        {
            var cleanedTitle = taskTitle.Trim();
            return $"Implement {cleanedTitle.ToLowerInvariant()} by {result.Substring(0, 1).ToLowerInvariant() + result[1..]}";
        }

        return result.StartsWith("Implement", StringComparison.OrdinalIgnoreCase)
            ? result
            : $"Implement and complete {result.Substring(0, 1).ToLowerInvariant() + result[1..]}";
    }

    private sealed class ChatResponse
    {
        [JsonPropertyName("choices")] public List<Choice>? Choices { get; set; }
    }

    private sealed class Choice
    {
        [JsonPropertyName("message")] public Msg? Message { get; set; }
    }

    private sealed class Msg
    {
        [JsonPropertyName("content")] public string? Content { get; set; }
    }
}