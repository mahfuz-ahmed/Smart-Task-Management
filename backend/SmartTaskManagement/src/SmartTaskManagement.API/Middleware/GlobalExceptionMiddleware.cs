using System;
using System.Text.Json;
using System.Linq;
using SmartTaskManagement.Application.Common;
using SmartTaskManagement.Application.Exceptions;

namespace SmartTaskManagement.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    // Cache the options to prevent heavy memory allocation on every exception
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Unhandled Exception: {Message}", ex.Message);

        // Prevent crash if the response has already started streaming to the client
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("The response has already started. The error handler will not be executed.");
            return;
        }

        context.Response.ContentType = "application/json";

        // Tuple to hold Status Code, Message, ErrorCode, and Optional Errors Array
        var (status, message, errorCode, errors) = ex switch
        {
            NotFoundException e => (e.StatusCode, e.Message, e.ErrorCode, null),
            ForbiddenException e => (e.StatusCode, e.Message, e.ErrorCode, null),
            ConflictException e => (e.StatusCode, e.Message, e.ErrorCode, null),
            BusinessException e => (e.StatusCode, e.Message, e.ErrorCode, null),
            UnauthorizedException e => (e.StatusCode, e.Message, e.ErrorCode, null),
            AppException e => (e.StatusCode, e.UserMessage ?? e.Message, e.ErrorCode, null),
            FluentValidation.ValidationException e => (400, "Validation failed", "VALIDATION_ERROR", e.Errors.Select(x => x.ErrorMessage)),
            _ => (500, _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.", "INTERNAL_ERROR", null)
        };

        context.Response.StatusCode = status;

        // Generate a formatted ErrorId for this error instance
        var errorId = $"ERR-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{Guid.NewGuid().ToString("N").Substring(0,8).ToUpper()}";
        var response = errors != null && errors.Any()
            ? ApiResponse.Fail(message, errors, errorId, errorCode)
            : ApiResponse.Fail(message, null, errorId, errorCode);

        var body = JsonSerializer.Serialize(response, _jsonOptions);

        await context.Response.WriteAsync(body);
    }
}
