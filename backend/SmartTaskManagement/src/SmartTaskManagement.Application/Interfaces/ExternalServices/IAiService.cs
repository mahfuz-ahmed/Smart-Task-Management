namespace SmartTaskManagement.Application.Interfaces.ExternalServices;

/// <summary>
/// Strategy interface — swap GitHub Models for any other AI provider
/// by changing only the DI registration. No controller/service changes needed.
/// </summary>
public interface IAiService
{
    Task<string> ImproveDescriptionAsync(
        string description,
        string? taskTitle = null,
        CancellationToken ct = default);
}