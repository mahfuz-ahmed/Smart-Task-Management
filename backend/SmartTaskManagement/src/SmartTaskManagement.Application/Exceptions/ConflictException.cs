using SmartTaskManagement.Application.Common.Constants;

namespace SmartTaskManagement.Application.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string message, string errorCode = ErrorCodes.Conflict) : base(message, errorCode)
    {
    }

    public ConflictException(string message, string errorCode, Exception innerException) : base(message, errorCode, innerException)
    {
    }
}