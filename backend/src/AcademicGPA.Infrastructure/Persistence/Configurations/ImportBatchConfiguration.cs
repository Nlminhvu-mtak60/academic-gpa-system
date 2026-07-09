using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

public class ImportBatchConfiguration : IEntityTypeConfiguration<ImportBatch>
{
    public void Configure(EntityTypeBuilder<ImportBatch> builder)
    {
        builder.ToTable("ImportBatches");

        builder.HasKey(ib => ib.Id);

        builder.Property(ib => ib.SourceType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(ib => ib.StudentProfile)
            .WithMany()
            .HasForeignKey(ib => ib.StudentProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ib => ib.Semester)
            .WithMany()
            .HasForeignKey(ib => ib.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(ib => ib.ImportBatchCourses)
            .WithOne(ibc => ibc.ImportBatch)
            .HasForeignKey(ibc => ibc.ImportBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ImportBatchCourseConfiguration : IEntityTypeConfiguration<ImportBatchCourse>
{
    public void Configure(EntityTypeBuilder<ImportBatchCourse> builder)
    {
        builder.ToTable("ImportBatchCourses");

        builder.HasKey(ibc => new { ibc.ImportBatchId, ibc.CourseId });

        builder.HasOne(ibc => ibc.Course)
            .WithMany()
            .HasForeignKey(ibc => ibc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
