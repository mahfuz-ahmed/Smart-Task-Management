using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");

        // ── Primary Key ───────────────────────────────────────────────────
        b.HasKey(u => u.Id);

        // ── Properties ────────────────────────────────────────────────────
        b.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        b.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500); // Standard length for hashed values (e.g. BCrypt, Argon2, PBKDF2)

        b.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        b.Property(u => u.IsActive)
            .IsRequired();

        // Ignore domain getter computed properties
        b.Ignore(u => u.FullName);

        // ── Query Filter ──────────────────────────────────────────────────
        b.HasQueryFilter(u => !u.IsDeleted);

        // ── Database Indexes (Performance Optimization) ───────────────────

        // 1. Unique index on Email for fast login and duplicate checks
        b.HasIndex(u => u.Email)
            .IsUnique();

        // 2. Fast lookup for active/deactivated users
        b.HasIndex(u => u.IsActive);
    }
}