namespace SmartTaskManagement.Application.Common;

/// <summary>
/// Generic Response Wrapper for Endpoints Returning Data
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    // ── Static Factory Methods ───────────────────────────────────────────────

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = Array.Empty<string>()
        };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            // Fixed the ?? operator type mismatch error
            Errors = errors is not null ? errors.ToList().AsReadOnly() : Array.Empty<string>()
        };

    public static ApiResponse<T> Fail(IEnumerable<string> errors) =>
        new()
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors.ToList().AsReadOnly()
        };
}

/// <summary>
/// Non-Generic Response Wrapper for Commands/Actions returning no payload
/// </summary>
public sealed class ApiResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    // ── Static Factory Methods ───────────────────────────────────────────────

    public static ApiResponse Ok(string message = "Success") =>
        new()
        {
            Success = true,
            Message = message,
            Errors = Array.Empty<string>()
        };

    public static ApiResponse Fail(string message, IEnumerable<string>? errors = null) =>
        new()
        {
            Success = false,
            Message = message,
            // Fixed the ?? operator type mismatch error
            Errors = errors is not null ? errors.ToList().AsReadOnly() : Array.Empty<string>()
        };

    public static ApiResponse Fail(IEnumerable<string> errors) =>
        new()
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors.ToList().AsReadOnly()
        };
}