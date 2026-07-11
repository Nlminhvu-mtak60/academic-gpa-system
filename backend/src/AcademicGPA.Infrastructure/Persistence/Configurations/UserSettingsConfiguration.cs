using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the UserSettings entity.
/// </summary>
public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("UserSettings");

        builder.HasKey(us => us.Id);

        builder.Property(us => us.PreferredLanguage)
            .HasMaxLength(10)
            .HasDefaultValue("vi")
            .IsRequired();

        builder.Property(us => us.PreferredTheme)
            .HasMaxLength(20)
            .HasDefaultValue("light")
            .IsRequired();

        builder.Property(us => us.ReceiveSystem)
            .HasDefaultValue(true);

        builder.Property(us => us.ReceiveAcademic)
            .HasDefaultValue(true);

        builder.Property(us => us.ReceiveGoal)
            .HasDefaultValue(true);

        builder.Property(us => us.ReceiveGpaMilestone)
            .HasDefaultValue(true);

        builder.Property(us => us.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(us => us.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // One-to-one unique index on UserId
        builder.HasIndex(us => us.UserId)
            .IsUnique()
            .HasDatabaseName("UX_UserSettings_UserId");

        // Relationships
        builder.HasOne(us => us.User)
            .WithOne(u => u.UserSettings)
            .HasForeignKey<UserSettings>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
