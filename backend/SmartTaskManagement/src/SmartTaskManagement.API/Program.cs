using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartTaskManagement.API.Middleware;
using SmartTaskManagement.Application.Validators.Auth;
using SmartTaskManagement.Infrastructure;
using SmartTaskManagement.Infrastructure.Data;
using SmartTaskManagement.Infrastructure.Identity;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ─────────────────────────────────────────────────────────────
    // Serilog
    // ─────────────────────────────────────────────────────────────

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    // ─────────────────────────────────────────────────────────────
    // Infrastructure
    // EF Core, JWT, Repositories, Application Services, etc.
    // ─────────────────────────────────────────────────────────────

    builder.Services.AddInfrastructure(builder.Configuration);

    // ─────────────────────────────────────────────────────────────
    // SignalR
    // ─────────────────────────────────────────────────────────────

    builder.Services.AddSignalR();

    // ─────────────────────────────────────────────────────────────
    // FluentValidation
    // ─────────────────────────────────────────────────────────────

    builder.Services.AddValidatorsFromAssemblyContaining<RegisterValidator>();

    // ─────────────────────────────────────────────────────────────
    // Rate Limiting
    // ─────────────────────────────────────────────────────────────

    builder.Services.AddMemoryCache();

    builder.Services.Configure<IpRateLimitOptions>(
        builder.Configuration.GetSection("RateLimiting"));

    builder.Services.AddInMemoryRateLimiting();

    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // ─────────────────────────────────────────────────────────────
    // CORS
    // ─────────────────────────────────────────────────────────────

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:4200",
                    "https://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // ─────────────────────────────────────────────────────────────
    // Controllers
    // ─────────────────────────────────────────────────────────────

    builder.Services
        .AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;

            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // ─────────────────────────────────────────────────────────────
    // Health Checks
    // ─────────────────────────────────────────────────────────────

    builder.Services
        .AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database");

    // ─────────────────────────────────────────────────────────────
    // Swagger
    // ─────────────────────────────────────────────────────────────

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Smart Task Management API",
            Version = "v1",
            Description =
                "Clean Architecture API for Smart Task Management."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token."
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                []
            }
        });
    });

    // ─────────────────────────────────────────────────────────────
    // Build
    // ─────────────────────────────────────────────────────────────

    var app = builder.Build();

    // ─────────────────────────────────────────────────────────────
    // Database Initialization
    // ─────────────────────────────────────────────────────────────

    try
    {
        await DatabaseSeeder.SeedAsync(app.Services);
    }
    catch (Exception exception)
    {
        Log.Error(
            exception,
            "Database migration/seeding failed during startup.");
    }

    // ─────────────────────────────────────────────────────────────
    // Middleware Pipeline
    // ─────────────────────────────────────────────────────────────

    app.UseSerilogRequestLogging();

    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseHttpsRedirection();

    app.UseDefaultFiles();
    app.UseStaticFiles();

    // ─────────────────────────────────────────────────────────────
    // Swagger
    // ─────────────────────────────────────────────────────────────

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Smart Task Management API v1");

        options.RoutePrefix = "swagger";
    });

    // ─────────────────────────────────────────────────────────────
    // Rate Limiting
    // ─────────────────────────────────────────────────────────────

    app.UseIpRateLimiting();

    // ─────────────────────────────────────────────────────────────
    // CORS
    // ─────────────────────────────────────────────────────────────

    app.UseCors("AllowAngular");

    // ─────────────────────────────────────────────────────────────
    // Authentication & Authorization
    // ─────────────────────────────────────────────────────────────

    app.UseAuthentication();
    app.UseAuthorization();

    // ─────────────────────────────────────────────────────────────
    // Endpoints
    // ─────────────────────────────────────────────────────────────

    app.MapControllers();

    app.MapHealthChecks("/health");

    // Example:
    // app.MapHub<NotificationHub>("/hubs/notifications");

    // Use only if ASP.NET Core is also serving the Angular SPA.
    app.MapFallbackToFile("index.html");

    Log.Information(
        "Smart Task Management API started successfully.");

    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(
        exception,
        "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}