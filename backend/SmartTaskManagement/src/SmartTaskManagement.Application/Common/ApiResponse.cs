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
    public string? ErrorId { get; init; }
    public string? ErrorCode { get; init; }

    // ── Static Factory Methods ───────────────────────────────────────────────

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = Array.Empty<string>(),
            ErrorId = null,
            ErrorCode = null
        };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null, string? errorId = null, string? errorCode = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors is not null ? errors.ToList().AsReadOnly() : Array.Empty<string>(),
            ErrorId = errorId,
            ErrorCode = errorCode
        };

    public static ApiResponse<T> Fail(IEnumerable<string> errors, string? errorId = null, string? errorCode = null) =>
        new()
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors.ToList().AsReadOnly(),
            ErrorId = errorId,
            ErrorCode = errorCode
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
    public string? ErrorId { get; init; }
    public string? ErrorCode { get; init; }

    // ── Static Factory Methods ───────────────────────────────────────────────

    public static ApiResponse Ok(string message = "Success") =>
        new()
        {
            Success = true,
            Message = message,
            Errors = Array.Empty<string>(),
            ErrorId = null,
            ErrorCode = null
        };

    public static ApiResponse Fail(string message, IEnumerable<string>? errors = null, string? errorId = null, string? errorCode = null) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors is not null ? errors.ToList().AsReadOnly() : Array.Empty<string>(),
            ErrorId = errorId,
            ErrorCode = errorCode
        };

    public static ApiResponse Fail(IEnumerable<string> errors, string? errorId = null, string? errorCode = null) =>
        new()
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors.ToList().AsReadOnly(),
            ErrorId = errorId,
            ErrorCode = errorCode
        };
}