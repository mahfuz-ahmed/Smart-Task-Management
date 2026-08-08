namespace SmartTaskManagement.Application.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public string? UserMessage { get; }

    public AppException(string message, string errorCode, int statusCode = 400, string? userMessage = null) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        UserMessage = userMessage;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.", "NOT_FOUND", 404) { }
}

public sealed class ForbiddenException : AppException
{
    // Bengali default message as requested
    public ForbiddenException(string message = "আপনাকে এই কাজ করার অনুমতি নেই.")
        : base(message, "FORBIDDEN", 403) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, "CONFLICT", 409) { }
}

public sealed class BusinessException : AppException
{
    public BusinessException(string message, string errorCode = "BUSINESS_RULE_VIOLATION") : base(message, errorCode, 400) { }
}

public sealed class UnauthorizedException : AppException
{
    // Bengali default message as requested
    public UnauthorizedException(string message = "প্রমাণীকরণ প্রয়োজন.")
        : base(message, "UNAUTHORIZED", 401) { }
}
