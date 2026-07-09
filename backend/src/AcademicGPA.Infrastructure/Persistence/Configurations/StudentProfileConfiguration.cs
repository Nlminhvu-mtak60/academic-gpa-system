using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.ToTable("StudentProfiles");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.StudentCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(sp => sp.StudentCode)
            .IsUnique();

        builder.Property(sp => sp.UniversityName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sp => sp.MajorName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(sp => sp.EnrollmentYear)
            .IsRequired();

        builder.Property(sp => sp.TotalRequiredCredits)
            .IsRequired();

        // 1:1 Relationship with User
        builder.HasOne(sp => sp.User)
            .WithOne(u => u.StudentProfile)
            .HasForeignKey<StudentProfile>(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
