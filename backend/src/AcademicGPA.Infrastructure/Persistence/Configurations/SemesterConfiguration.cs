using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Semesters table.
/// </summary>
public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.ToTable("Semesters");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.AcademicYearId)
            .IsRequired();

        builder.Property(s => s.SemesterName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(s => s.IsImported)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.ImportedCredits)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(s => s.ImportedGpa10)
            .IsRequired()
            .HasPrecision(4, 2)
            .HasColumnType("decimal(4,2)")
            .HasDefaultValue(0.00m);

        builder.Property(s => s.ImportedGpa4)
            .IsRequired()
            .HasPrecision(3, 2)
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0.00m);

        // 1:N Relationship with AcademicYear
        builder.HasOne(s => s.AcademicYear)
            .WithMany(ay => ay.Semesters)
            .HasForeignKey(s => s.AcademicYearId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indices
        builder.HasIndex(s => new { s.AcademicYearId, s.IsDeleted })
            .HasDatabaseName("IX_Semesters_AcademicYearId");

        // Unique constraint on SemesterName per AcademicYear for active (non-deleted) semesters
        builder.HasIndex(s => new { s.AcademicYearId, s.SemesterName })
            .HasFilter("\"IsDeleted\" = false")
            .IsUnique()
            .HasDatabaseName("UX_Semesters_AcademicYearId_SemesterName");
    }
}
