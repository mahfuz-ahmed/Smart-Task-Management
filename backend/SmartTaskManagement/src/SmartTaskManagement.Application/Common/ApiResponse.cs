namespace SmartTaskManagement.Application.Common;

public class ApiResponse
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public DateTime TimestampUtc { get; init; }

    public string? ErrorId { get; init; }

    public string? ErrorCode { get; init; }

    protected ApiResponse()
    {
        TimestampUtc = DateTime.UtcNow;
    }

    public static ApiResponse Ok(string message = "Success")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            TimestampUtc = DateTime.UtcNow
        };
    }

    public static ApiResponse Fail(string message, IEnumerable<string>? errors = null, string? errorId = null, string? errorCode = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors is null ? Array.Empty<string>() : errors.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray(),
            ErrorId = errorId,
            ErrorCode = errorCode,
            TimestampUtc = DateTime.UtcNow
        };
    }
}

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    private ApiResponse()
    {
    }

    public static ApiResponse<T> Ok(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            TimestampUtc = DateTime.UtcNow
        };
    }
}