using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicGPA.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Notification entity.
/// </summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.IsBroadcast)
            .HasDefaultValue(false);

        builder.Property(n => n.RecipientName)
            .HasMaxLength(200);

        builder.Property(n => n.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Indexes for performance
        builder.HasIndex(n => n.UserId)
            .HasDatabaseName("IX_Notifications_UserId");

        builder.HasIndex(n => new { n.UserId, n.IsRead })
            .HasDatabaseName("IX_Notifications_UserId_IsRead");

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
