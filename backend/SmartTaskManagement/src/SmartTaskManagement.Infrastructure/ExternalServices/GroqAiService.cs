using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartTaskManagement.Application.Interfaces.ExternalServices;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SmartTaskManagement.Infrastructure.Services;

public sealed class GroqAiService : IAiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GroqAiService> _logger;

    private static readonly string[] CandidateModels = new[]
    {
        "llama-3.3-70b-versatile",
        "llama3-8b-8192",
        "mixtral-8x7b-32768"
    };

    public GroqAiService(HttpClient http, IConfiguration config, ILogger<GroqAiService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    public async Task<string> ImproveDescriptionAsync(
        string description, string? taskTitle = null, CancellationToken ct = default)
    {
        var apiKey = _config["AiSettings:ApiKey"]
            ?? _config["AiSettings:GroqApiKey"]
            ?? _config["AiSettings:GitHubToken"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Groq API Key is missing in AiSettings. Returning fallback enhancement.");
            return BuildFallbackImprovement(description, taskTitle);
        }

        var configuredModel = _config["AiSettings:Model"];
        var modelsToTry = new List<string>();

        if (!string.IsNullOrWhiteSpace(configuredModel))
        {
            modelsToTry.Add(configuredModel);
        }

        foreach (var m in CandidateModels)
        {
            if (!modelsToTry.Contains(m))
            {
                modelsToTry.Add(m);
            }
        }

        var systemPrompt = """
            You are a professional technical writer for software project management.
            When given a task description, you must:
            1. Correct grammar and spelling.
            2. Improve clarity and readability.
            3. Make descriptions more professional.
            4. Expand short or vague descriptions with relevant detail.
            5. Produce actionable task descriptions using imperative language.
            Return ONLY the improved task description. Do not add explanations, lists, headers, or comments.
            """;

        var userPrompt = taskTitle != null
            ? $"Task Title: {taskTitle}\n\nDescription: {description}"
            : $"Description: {description}";

        const string endpoint = "https://api.groq.com/openai/v1/chat/completions";

        foreach (var model in modelsToTry)
        {
            var body = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userPrompt }
                },
                temperature = 0.7,
                max_tokens = 500
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
            request.Headers.UserAgent.ParseAdd("SmartTaskManagement");
            request.Content = JsonContent.Create(body);

            try
            {
                var response = await _http.SendAsync(request, ct);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: ct);
                    var improved = result?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

                    if (!string.IsNullOrWhiteSpace(improved))
                    {
                        _logger.LogInformation("Groq AI successfully enhanced description using model '{Model}'.", model);
                        return improved;
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning(
                    "Groq AI API with model '{Model}' returned {StatusCode}. Response: {ErrorContent}",
                    model,
                    response.StatusCode,
                    errorContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq AI HTTP call failed for model '{Model}'.", model);
            }
        }

        _logger.LogWarning("All Groq AI models failed or API key invalid. Returning fallback enhancement.");
        return BuildFallbackImprovement(description, taskTitle);
    }

    private static string BuildFallbackImprovement(string description, string? taskTitle)
    {
        if (string.IsNullOrWhiteSpace(description)) return description;

        var baseText = description.Trim();
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
