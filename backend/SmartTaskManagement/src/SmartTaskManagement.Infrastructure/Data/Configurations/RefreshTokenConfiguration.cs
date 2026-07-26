using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

namespace SmartTaskManagement.Infrastructure.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");

        // ── Primary Key ───────────────────────────────────────────────────
        b.HasKey(r => r.Id);

        // ── Properties ────────────────────────────────────────────────────
        b.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(r => r.JwtId)
            .IsRequired()
            .HasMaxLength(128); // Standard JWT JTI is a Guid string or short hash

        b.Property(r => r.ExpiresAtUtc)
            .IsRequired();

        b.Property(r => r.IsUsed)
            .IsRequired();

        b.Property(r => r.IsRevoked)
            .IsRequired();

        // Explicitly Ignore Computed Property (Does not exist in DB table)
        b.Ignore(r => r.IsActive);

        // ── Relationships ─────────────────────────────────────────────────
        b.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Database Indexes (Performance Optimization) ───────────────────

        // 1. Unique Index on Token string (Fast Lookup during Token Refresh)
        b.HasIndex(r => r.Token)
            .IsUnique();

        // 2. Fast Lookup for active tokens belonging to a specific user
        b.HasIndex(r => new { r.UserId, r.IsUsed, r.IsRevoked });

        // 3. Fast Cleanup of expired tokens (Cron/Background Jobs)
        b.HasIndex(r => r.ExpiresAtUtc);
    }
}