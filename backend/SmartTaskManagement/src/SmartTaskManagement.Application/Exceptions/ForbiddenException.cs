using SmartTaskManagement.Application.Common.Constants;

namespace SmartTaskManagement.Application.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.") : base(message, ErrorCodes.Forbidden)
    {
    }

    public ForbiddenException(string message, Exception innerException) : base(message, ErrorCodes.Forbidden, innerException)
    {
    }
}