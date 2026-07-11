using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

public class AcademicYearConfiguration : IEntityTypeConfiguration<AcademicYear>
{
    public void Configure(EntityTypeBuilder<AcademicYear> builder)
    {
        builder.ToTable("AcademicYears");

        builder.HasKey(ay => ay.Id);

        builder.Property(ay => ay.StudentProfileId)
            .IsRequired();

        builder.Property(ay => ay.YearName)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(ay => ay.StartYear)
            .IsRequired();

        builder.Property(ay => ay.EndYear)
            .IsRequired();

        builder.Property(ay => ay.StartDate)
            .IsRequired();

        builder.Property(ay => ay.EndDate)
            .IsRequired();

        builder.Property(ay => ay.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Completed");

        builder.Property(ay => ay.IsCurrent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ay => ay.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ay => ay.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ay => ay.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ay => ay.IsImported)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ay => ay.ImportedCredits)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ay => ay.ImportedGpa10)
            .IsRequired()
            .HasPrecision(4, 2)
            .HasColumnType("decimal(4,2)")
            .HasDefaultValue(0.00m);

        builder.Property(ay => ay.ImportedGpa4)
            .IsRequired()
            .HasPrecision(3, 2)
            .HasColumnType("decimal(3,2)")
            .HasDefaultValue(0.00m);

        // 1:N Relationship with StudentProfile
        builder.HasOne(ay => ay.StudentProfile)
            .WithMany(sp => sp.AcademicYears)
            .HasForeignKey(ay => ay.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indices
        builder.HasIndex(ay => new { ay.StudentProfileId, ay.IsDeleted })
            .HasDatabaseName("IX_AcademicYears_StudentProfileId");

        // Unique constraint on YearName per StudentProfile for active (non-deleted) years
        builder.HasIndex(ay => new { ay.StudentProfileId, ay.YearName })
            .HasFilter("\"IsDeleted\" = false")
            .IsUnique()
            .HasDatabaseName("UX_AcademicYears_StudentProfileId_YearName");

        // Check constraint
        builder.ToTable(t => t.HasCheckConstraint("CK_AcademicYears_Years", "\"StartYear\" <= \"EndYear\" AND \"EndYear\" <= \"StartYear\" + 1"));
    }
}
