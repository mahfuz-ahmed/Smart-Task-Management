using System.Text.Json;
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

        // Tuple to hold Status Code, Message, and Optional Errors Array
        var (status, message, errors) = ex switch
        {
            NotFoundException e => (e.StatusCode, e.Message, null),
            ForbiddenException e => (e.StatusCode, e.Message, null),
            ConflictException e => (e.StatusCode, e.Message, null),
            BusinessException e => (e.StatusCode, e.Message, null),
            UnauthorizedException e => (e.StatusCode, e.Message, null),
            AppException e => (e.StatusCode, e.Message, null),

            // Optional: If you are using FluentValidation and throwing ValidationException
            FluentValidation.ValidationException e =>
                (400, "Validation failed", e.Errors.Select(x => x.ErrorMessage)),

            _ => (
                500,
                _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.",
                _env.IsDevelopment() ? new[] { ex.StackTrace ?? string.Empty } : null
            )
        };

        context.Response.StatusCode = status;

        var response = errors != null && errors.Any()
            ? ApiResponse.Fail(message, errors)
            : ApiResponse.Fail(message);

        var body = JsonSerializer.Serialize(response, _jsonOptions);

        await context.Response.WriteAsync(body);
    }
}
