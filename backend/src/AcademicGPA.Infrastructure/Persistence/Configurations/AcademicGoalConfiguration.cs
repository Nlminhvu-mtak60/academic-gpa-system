using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the AcademicGoal entity.
/// </summary>
public class AcademicGoalConfiguration : IEntityTypeConfiguration<AcademicGoal>
{
    public void Configure(EntityTypeBuilder<AcademicGoal> builder)
    {
        builder.ToTable("AcademicGoals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.TargetCumulativeGpa10)
            .HasColumnType("decimal(4,2)")
            .IsRequired();

        builder.Property(g => g.TargetCumulativeGpa4)
            .HasColumnType("decimal(3,2)")
            .IsRequired();

        builder.Property(g => g.Notes)
            .HasMaxLength(500);

        builder.Property(g => g.IsAchieved)
            .HasDefaultValue(false);

        builder.Property(g => g.IsActive)
            .HasDefaultValue(true);

        builder.Property(g => g.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Filtered unique index: only one active goal per student
        builder.HasIndex(g => new { g.StudentProfileId, g.IsActive })
            .HasFilter("[IsActive] = 1")
            .IsUnique()
            .HasDatabaseName("UX_AcademicGoals_StudentProfileId_Active");

        // General index for querying all goals by student
        builder.HasIndex(g => g.StudentProfileId)
            .HasDatabaseName("IX_AcademicGoals_StudentProfileId");

        // Relationship
        builder.HasOne(g => g.StudentProfile)
            .WithMany(sp => sp.AcademicGoals)
            .HasForeignKey(g => g.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
