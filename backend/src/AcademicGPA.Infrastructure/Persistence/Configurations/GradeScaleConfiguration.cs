using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

public class GradeScaleConfiguration : IEntityTypeConfiguration<GradeScale>
{
    public void Configure(EntityTypeBuilder<GradeScale> builder)
    {
        builder.ToTable("GradeScales");

        builder.HasKey(gs => gs.Id);

        builder.Property(gs => gs.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(gs => gs.GradeScaleEntries)
            .WithOne(e => e.GradeScale)
            .HasForeignKey(e => e.GradeScaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed default GradeScale
        builder.HasData(new GradeScale
        {
            Id = System.Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Default 10-point scale",
            IsDefault = true
        });
    }
}
