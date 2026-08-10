using SmartTaskManagement.Application.Common.Constants;

namespace SmartTaskManagement.Application.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string resource, object key) : base($"{resource} with key '{key}' was not found.", ErrorCodes.NotFound)
    {
    }

    public NotFoundException(string message) : base(message, ErrorCodes.NotFound)
    {
    }
}