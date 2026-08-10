//using FluentValidation;
//using SmartTaskManagement.API.Exceptions;
//using SmartTaskManagement.Application.Common;
//using SmartTaskManagement.Application.Common.Constants;
//using SmartTaskManagement.Application.Exceptions;

//namespace SmartTaskManagement.API.Middleware;

//public sealed class GlobalExceptionMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly ILogger<GlobalExceptionMiddleware> _logger;
//    private readonly IHostEnvironment _environment;

//    public GlobalExceptionMiddleware(
//        RequestDelegate next,
//        ILogger<GlobalExceptionMiddleware> logger,
//        IHostEnvironment environment)
//    {
//        _next = next;
//        _logger = logger;
//        _environment = environment;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        try
//        {
//            await _next(context);
//        }
//        catch (Exception exception)
//        {
//            await HandleExceptionAsync(context, exception);
//        }
//    }

//    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
//    {
//        var errorId = GenerateErrorId();

//        LogException(exception, errorId);

//        if (context.Response.HasStarted)
//        {
//            _logger.LogWarning(
//                "Response has already started. " +
//                "Unable to write error response. ErrorId: {ErrorId}",
//                errorId);

//            return;
//        }

//        var details = GetExceptionDetails(exception);

//        var statusCode =
//            ExceptionStatusCodeMapper.Map(exception);

//        context.Response.Clear();

//        context.Response.StatusCode =
//            (int)statusCode;

//        context.Response.ContentType =
//            "application/json";

//        var response = ApiResponse.Fail(
//            details.Message,
//            details.Errors,
//            errorId,
//            details.ErrorCode);

//        await context.Response.WriteAsJsonAsync(response);
//    }

//    private ExceptionDetails GetExceptionDetails(Exception exception)
//    {
//        return exception switch
//        {
//            AppException appException =>
//                new ExceptionDetails(
//                    Message: appException.Message,
//                    ErrorCode: appException.ErrorCode,
//                    Errors: null),

//            ValidationException validationException =>
//                new ExceptionDetails(
//                    Message: "Validation failed.",
//                    ErrorCode: ErrorCodes.Validation,
//                    Errors: validationException.Errors
//                        .Select(x => x.ErrorMessage)
//                        .Distinct()
//                        .ToArray()),
//            _ =>
//                new ExceptionDetails(
//                    Message: _environment.IsDevelopment()
//                        ? exception.Message
//                        : "An unexpected error occurred.",
//                    ErrorCode: ErrorCodes.InternalError,
//                    Errors: null)
//        };
//    }

//    private void LogException(Exception exception, string errorId)
//    {
//        if (exception is AppException)
//        {
//            _logger.LogWarning(
//                exception,
//                "Application exception occurred. ErrorId: {ErrorId}",
//                errorId);

//            return;
//        }

//        _logger.LogError(
//            exception,
//            "Unhandled exception occurred. ErrorId: {ErrorId}",
//            errorId);
//    }

//    private static string GenerateErrorId()
//    {
//        return $"ERR-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-" +
//               $"{Guid.NewGuid():N}"[..8].ToUpperInvariant();
//    }

//    private sealed record ExceptionDetails(
//        string Message,
//        string ErrorCode,
//        IReadOnlyCollection<string>? Errors);
//}

using System.Security.Claims;
using FluentValidation;
using SmartTaskManagement.API.Exceptions;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Common.Constants;
using SmartTaskManagement.Application.Exceptions;

namespace SmartTaskManagement.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorId = GenerateErrorId();

        LogException(
            context,
            exception,
            errorId);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "Response has already started. " +
                "Unable to write error response. ErrorId: {ErrorId}",
                errorId);

            return;
        }

        var details = GetExceptionDetails(exception);

        var statusCode = ExceptionStatusCodeMapper.Map(exception);

        context.Response.Clear();

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.Fail(
            message: details.Message,
            errors: details.Errors,
            errorId: errorId,
            errorCode: details.ErrorCode);

        await context.Response.WriteAsJsonAsync(response);
    }

    private ExceptionDetails GetExceptionDetails(
        Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => new ExceptionDetails(
                    Message: "Validation failed.",
                    ErrorCode: ErrorCodes.Validation,
                    Errors: validationException.Errors
                        .Select(x => x.ErrorMessage)
                        .Distinct()
                        .ToArray()),

            AppException appException => new ExceptionDetails(
                    Message: appException.Message,
                    ErrorCode: appException.ErrorCode,
                    Errors: null),

            _ =>
                new ExceptionDetails(
                    Message: _environment.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred.",
                    ErrorCode: ErrorCodes.InternalError,
                    Errors: null)
        };
    }

    private void LogException(
        HttpContext context,
        Exception exception,
        string errorId)
    {
        var requestPath =
            context.Request.Path.Value ?? "N/A";

        var requestMethod =
            context.Request.Method;

        var userId =
            context.User.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? context.User.FindFirstValue("sub")
            ?? "anonymous";

        var traceId =
            context.TraceIdentifier;

        if (exception is ValidationException)
        {
            _logger.LogInformation(
                "Validation exception occurred. " +
                "ErrorId={ErrorId} " +
                "TraceId={TraceId} " +
                "Path={Path} " +
                "Method={Method} " +
                "UserId={UserId}",
                errorId,
                traceId,
                requestPath,
                requestMethod,
                userId);

            return;
        }

        if (exception is AppException)
        {
            _logger.LogWarning(
                exception,
                "Application exception occurred. " +
                "ErrorId={ErrorId} " +
                "TraceId={TraceId} " +
                "Path={Path} " +
                "Method={Method} " +
                "UserId={UserId}",
                errorId,
                traceId,
                requestPath,
                requestMethod,
                userId);

            return;
        }

        _logger.LogError(
            exception,
            "Unhandled exception occurred. " +
            "ErrorId={ErrorId} " +
            "TraceId={TraceId} " +
            "Path={Path} " +
            "Method={Method} " +
            "UserId={UserId}",
            errorId,
            traceId,
            requestPath,
            requestMethod,
            userId);
    }

    private static string GenerateErrorId()
    {
        var timestamp =
            DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss-fff");

        var randomId =
            Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpperInvariant();

        return $"ERR-{timestamp}-{randomId}";
    }

    private sealed record ExceptionDetails(
        string Message,
        string ErrorCode,
        IReadOnlyCollection<string>? Errors);
}