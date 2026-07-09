using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the Scores table.
/// </summary>
public class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.ToTable("Scores");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CourseId)
            .IsRequired();

        builder.Property(s => s.AttendanceScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(s => s.ContinuousScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(s => s.FinalExamScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(s => s.CourseScore)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(s => s.LetterGrade)
            .HasMaxLength(5)
            .IsRequired(false);

        builder.Property(s => s.Gpa4Value)
            .HasColumnType("decimal(5,2)")
            .IsRequired(false);

        builder.Property(s => s.AcademicClassification)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(s => s.IsPass)
            .IsRequired(false);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(s => s.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // 1:1 relationship with Course, cascade delete
        builder.HasOne(s => s.Course)
            .WithOne(c => c.Score)
            .HasForeignKey<Score>(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on CourseId
        builder.HasIndex(s => s.CourseId)
            .IsUnique()
            .HasDatabaseName("UX_Scores_CourseId");
    }
}
