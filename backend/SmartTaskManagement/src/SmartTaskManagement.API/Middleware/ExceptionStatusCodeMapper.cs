using System.Net;
using FluentValidation;
using SmartTaskManagement.Application.Exceptions;

namespace SmartTaskManagement.API.Exceptions;

public static class ExceptionStatusCodeMapper
{
    public static HttpStatusCode Map(Exception exception)
    {
        return exception switch
        {
            ValidationException =>
                HttpStatusCode.BadRequest,

            NotFoundException =>
                HttpStatusCode.NotFound,

            ForbiddenException =>
                HttpStatusCode.Forbidden,

            UnauthorizedException =>
                HttpStatusCode.Unauthorized,

            ConflictException =>
                HttpStatusCode.Conflict,

            BusinessException =>
                HttpStatusCode.BadRequest,

            _ =>
                HttpStatusCode.InternalServerError
        };
    }
}