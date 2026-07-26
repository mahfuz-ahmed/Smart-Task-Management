using SmartTaskManagement.Application.Common.Extensions;
using SmartTaskManagement.Application.Interfaces;

namespace SmartTaskManagement.Infrastructure.Services;

public sealed class AiService : IAiService
{
    // Note: Inject your AI client here (e.g., OpenAI SDK, Gemini SDK, etc.)
    // private readonly OpenAiClient _aiClient;

    public async Task<string> ImproveDescriptionAsync(
        string description,
        string taskTitle,
        CancellationToken ct = default)
    {
        // 1. Sanitize the input to prevent injection attacks or unwanted content
        var cleanDescription = description.SanitizeDescription();
        var cleanTitle = taskTitle.SanitizeTitle();

        // 1. Prepare the prompt for the AI model
        // var prompt = $"Improve this task description '{cleanDescription}' for title '{cleanTitle}'";
        // var response = await _aiClient.GenerateAsync(prompt, ct);

        // Simulation Response
        await Task.Delay(100, ct);

        return $"[AI Enhanced]: {cleanDescription}";
    }
}