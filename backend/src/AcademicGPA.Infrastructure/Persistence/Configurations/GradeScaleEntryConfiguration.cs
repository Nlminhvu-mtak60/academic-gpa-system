using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

public class GradeScaleEntryConfiguration : IEntityTypeConfiguration<GradeScaleEntry>
{
    public void Configure(EntityTypeBuilder<GradeScaleEntry> builder)
    {
        builder.ToTable("GradeScaleEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.LetterGrade)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(e => e.Classification)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.MinScore)
            .HasPrecision(4, 2);

        builder.Property(e => e.MaxScore)
            .HasPrecision(4, 2);

        builder.Property(e => e.Gpa4Value)
            .HasPrecision(3, 2);

        // Seed entries
        var defaultScaleId = System.Guid.Parse("00000000-0000-0000-0000-000000000001");
        builder.HasData(
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000011"), GradeScaleId = defaultScaleId, LetterGrade = "A+", MinScore = 9.0m, MaxScore = 10.0m, Gpa4Value = 4.0m, Classification = "Outstanding", IsPass = true, SortOrder = 1 },
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000012"), GradeScaleId = defaultScaleId, LetterGrade = "A", MinScore = 8.5m, MaxScore = 8.99m, Gpa4Value = 3.7m, Classification = "Excellent", IsPass = true, SortOrder = 2 },
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000013"), GradeScaleId = defaultScaleId, LetterGrade = "B+", MinScore = 8.0m, MaxScore = 8.49m, Gpa4Value = 3.5m, Classification = "Very Good", IsPass = true, SortOrder = 3 },
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000014"), GradeScaleId = defaultScaleId, LetterGrade = "B", MinScore = 7.0m, MaxScore = 7.99m, Gpa4Value = 3.0m, Classification = "Good", IsPass = true, SortOrder = 4 },
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000015"), GradeScaleId = defaultScaleId, LetterGrade = "C+", MinScore = 6.5m, MaxScore = 6.99m, Gpa4Value = 2.5m, Classification = "Average Good", IsPass = true, SortOrder = 5 },
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000016"), GradeScaleId = defaultScaleId, LetterGrade = "C", MinScore = 5.5m, MaxScore = 6.49m, Gpa4Value = 2.0m, Classification = "Average", IsPass = true, SortOrder = 6 },
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000017"), GradeScaleId = defaultScaleId, LetterGrade = "D+", MinScore = 5.0m, MaxScore = 5.49m, Gpa4Value = 1.5m, Classification = "Average", IsPass = true, SortOrder = 7 },
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000018"), GradeScaleId = defaultScaleId, LetterGrade = "D", MinScore = 4.0m, MaxScore = 4.99m, Gpa4Value = 1.0m, Classification = "Weak", IsPass = true, SortOrder = 8 },
            new GradeScaleEntry { Id = System.Guid.Parse("00000000-0000-0000-0000-000000000019"), GradeScaleId = defaultScaleId, LetterGrade = "F", MinScore = 0.0m, MaxScore = 3.99m, Gpa4Value = 0.0m, Classification = "Poor", IsPass = false, SortOrder = 9 }
        );
    }
}
