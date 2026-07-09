using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the ScoreAuditLogs table.
/// </summary>
public class ScoreAuditLogConfiguration : IEntityTypeConfiguration<ScoreAuditLog>
{
    public void Configure(EntityTypeBuilder<ScoreAuditLog> builder)
    {
        builder.ToTable("ScoreAuditLogs");

        builder.HasKey(sal => sal.Id);

        builder.Property(sal => sal.CourseId)
            .IsRequired();

        builder.Property(sal => sal.FieldChanged)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sal => sal.OldValue)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(sal => sal.NewValue)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(sal => sal.ChangedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // N:1 relationship with Course, cascade delete on course removal
        builder.HasOne(sal => sal.Course)
            .WithMany()
            .HasForeignKey(sal => sal.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on CourseId for performance
        builder.HasIndex(sal => sal.CourseId)
            .HasDatabaseName("IX_ScoreAuditLogs_CourseId");
    }
}
