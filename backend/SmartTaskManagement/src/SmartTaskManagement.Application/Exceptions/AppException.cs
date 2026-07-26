namespace SmartTaskManagement.Application.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public AppException(string message, int statusCode = 400) : base(message) => StatusCode = statusCode;
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.", 404) { }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message, 403) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public sealed class BusinessException : AppException
{
    public BusinessException(string message) : base(message, 400) { }
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication required.")
        : base(message, 401) { }
}
