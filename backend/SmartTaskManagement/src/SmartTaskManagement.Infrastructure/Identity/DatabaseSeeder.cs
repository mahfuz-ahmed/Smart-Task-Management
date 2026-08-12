using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartTaskManagement.Domain.Entities;
using SmartTaskManagement.Domain.Enums;
using SmartTaskManagement.Infrastructure.Data;

namespace SmartTaskManagement.Infrastructure.Identity;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // 1. Apply pending migrations automatically
            await context.Database.MigrateAsync();

            // 2. Prevent duplicate seeding
            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already seeded. Skipping seeder.");
                return;
            }

            // 3. Default Admin User
            var admin = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Smart Task",
                LastName = "Management",
                Email = "admin@smarttask.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin1234!"),
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            // 4. Default Standard User (useful for testing access roles)
            var member = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Mahfuz",
                LastName = "Ahmed",
                Email = "user@smarttask.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User1234!"),
                Role = UserRole.TeamMember,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            await context.Users.AddRangeAsync(admin, member);
            await context.SaveChangesAsync();

            logger.LogInformation("Database users seeded successfully.");
            logger.LogInformation("Admin : admin@smarttask.com / Admin1234!");
            logger.LogInformation("Member: user@smarttask.com / User1234!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}