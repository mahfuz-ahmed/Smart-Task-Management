using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManagement.Domain.Entities;

public sealed class ProjectConfiguration
    : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> b)
    {
        b.ToTable("Projects");

        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(100);

        b.Property(x => x.Description).HasMaxLength(500);

        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

        b.Property(x => x.Priority).HasConversion<string>().HasMaxLength(30);

        b.HasQueryFilter(x => !x.IsDeleted);


        // Creator
        b.HasOne(x => x.CreatedByUser)
            .WithMany(x => x.Projects)
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Project -> Tasks
        b.HasMany(x => x.Tasks)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project -> Members
        b.HasMany(x => x.Members)
            .WithOne(x => x.Project)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.CreatedByUserId);
    }
}