using SmartTaskManagement.Application.Common.Constants;

namespace SmartTaskManagement.Application.Exceptions;

public sealed class BusinessException : AppException
{
    public BusinessException(string message, string errorCode = ErrorCodes.BusinessRuleViolation) : base(message, errorCode)
    {
    }

    public BusinessException(string message, string errorCode, Exception innerException) : base(message, errorCode, innerException)
    {
    }
}