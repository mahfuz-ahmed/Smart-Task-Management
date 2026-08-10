using SmartTaskManagement.Application.Common.Constants;

namespace SmartTaskManagement.Application.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication is required.") : base(message, ErrorCodes.Unauthorized)
    {
    }

    public UnauthorizedException(string message, Exception innerException) : base(message, ErrorCodes.Unauthorized, innerException)
    {
    }
}