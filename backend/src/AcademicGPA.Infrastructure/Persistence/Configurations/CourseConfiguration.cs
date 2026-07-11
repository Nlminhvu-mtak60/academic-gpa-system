using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Courses table.
/// </summary>
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SemesterId)
            .IsRequired();

        builder.Property(c => c.CourseCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.CourseName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Credits)
            .IsRequired()
            .HasDefaultValue(3);

        builder.Property(c => c.IsRetake)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.OriginalCourseId)
            .IsRequired(false);

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // 1:N Relationship with Semester
        builder.HasOne(c => c.Semester)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SemesterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing FK relationship for original course attempt
        builder.HasOne(c => c.OriginalCourse)
            .WithMany(c => c.Retakes)
            .HasForeignKey(c => c.OriginalCourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indices
        builder.HasIndex(c => new { c.SemesterId, c.IsDeleted })
            .HasDatabaseName("IX_Courses_SemesterId");

        builder.HasIndex(c => c.OriginalCourseId)
            .HasFilter("\"OriginalCourseId\" IS NOT NULL")
            .HasDatabaseName("IX_Courses_OriginalCourseId");

        // Unique constraint on CourseName per Semester for active (non-deleted) courses
        builder.HasIndex(c => new { c.SemesterId, c.CourseName })
            .HasFilter("\"IsDeleted\" = false")
            .IsUnique()
            .HasDatabaseName("UX_Courses_SemesterId_CourseName");
    }
}
