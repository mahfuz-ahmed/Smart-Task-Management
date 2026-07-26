using AspNetCoreRateLimit;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;
using SmartTaskManagement.API.Middleware;
using SmartTaskManagement.Application.DTOs.AI;
using SmartTaskManagement.Application.DTOs.Auth;
using SmartTaskManagement.Application.DTOs.Comments;
using SmartTaskManagement.Application.DTOs.Projects;
using SmartTaskManagement.Application.DTOs.Tasks;
using SmartTaskManagement.Application.Validators.AI;
using SmartTaskManagement.Application.Validators.Auth;
using SmartTaskManagement.Application.Validators.Comments;
using SmartTaskManagement.Application.Validators.Projects;
using SmartTaskManagement.Application.Validators.Tasks;
using SmartTaskManagement.Infrastructure;
using SmartTaskManagement.Infrastructure.Data;
using SmartTaskManagement.Infrastructure.Identity;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, svc, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(svc));

    // ── Infrastructure (EF, JWT, Repos, Services) ─────────────────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── SignalR ───────────────────────────────────────────────────────────────
    builder.Services.AddSignalR();

    // ── FluentValidation ─────────────────────────────────────────────────────
    builder.Services.AddScoped<IValidator<RegisterDto>, RegisterValidator>();
    builder.Services.AddScoped<IValidator<LoginDto>, LoginValidator>();
    builder.Services.AddScoped<IValidator<RefreshTokenRequestDto>, RefreshTokenRequestValidator>();
    builder.Services.AddScoped<IValidator<CreateProjectDto>, CreateProjectValidator>();
    builder.Services.AddScoped<IValidator<UpdateProjectDto>, UpdateProjectValidator>();
    builder.Services.AddScoped<IValidator<CreateTaskDto>, CreateTaskValidator>();
    builder.Services.AddScoped<IValidator<UpdateTaskDto>, UpdateTaskValidator>();
    builder.Services.AddScoped<IValidator<CreateCommentDto>, CreateCommentValidator>();
    builder.Services.AddScoped<IValidator<ImproveDescriptionDto>, ImproveDescriptionValidator>();

    // ── Rate Limiting ─────────────────────────────────────────────────────────
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
        options.AddPolicy("AllowAngular", policy =>
            policy.WithOrigins("http://localhost:4200", "https://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));   // Required for SignalR

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers()
        .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            o.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    // ── Health Checks ─────────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database");

    // ── Swagger with JWT Bearer ───────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title       = "Smart Task Management API",
            Version     = "v1",
            Description = "Clean Architecture API with SignalR notifications and AI description enhancement."
        });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name        = "Authorization",
            Type        = SecuritySchemeType.ApiKey,
            Scheme      = "Bearer",
            BearerFormat = "JWT",
            In          = ParameterLocation.Header,
            Description = "Enter: Bearer {your token}"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                []
            }
        });
    });

    // ─────────────────────────────────────────────────────────────────────────
    var app = builder.Build();
    // ─────────────────────────────────────────────────────────────────────────

    await DatabaseSeeder.SeedAsync(app.Services);

    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Task Management API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseHttpsRedirection();
    app.UseIpRateLimiting();
    app.UseCors("AllowAngular");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Smart Task Management API starting on https://localhost:7125");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
