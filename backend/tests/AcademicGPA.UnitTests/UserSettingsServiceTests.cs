using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Features.Settings.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcademicGPA.UnitTests;

public class UserSettingsServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public UserSettingsServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    private async Task<User> SeedUserAsync(ApplicationDbContext context, string email)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = email,
            PasswordHash = "hash",
            PreferredLanguage = "vi",
            PreferredTheme = "light"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task GetSettingsAsync_ShouldCreateSettingsOnDemand_IfNull()
    {
        using var context = CreateContext();
        var user = await SeedUserAsync(context, "john.doe@example.com");

        var service = new UserSettingsService(context);

        // Get settings when they don't exist yet
        var settings = await service.GetSettingsAsync(user.Id, CancellationToken.None);
        settings.Should().NotBeNull();
        settings.PreferredLanguage.Should().Be("vi");
        settings.PreferredTheme.Should().Be("light");

        // Verify it was saved to DB
        var dbSettings = await context.UserSettings.FirstOrDefaultAsync(us => us.UserId == user.Id);
        dbSettings.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateSettingsAsync_ShouldSaveSettings_AndSyncToUser()
    {
        using var context = CreateContext();
        var user = await SeedUserAsync(context, "john.doe@example.com");

        var service = new UserSettingsService(context);
        var dto = new UserSettingsDto("en", "dark", false, false, false, false);

        await service.UpdateSettingsAsync(user.Id, dto, CancellationToken.None);

        // Verify visual settings were synced back to User entity
        var updatedUser = await context.Users.FindAsync(user.Id);
        updatedUser!.PreferredLanguage.Should().Be("en");
        updatedUser!.PreferredTheme.Should().Be("dark");

        // Verify UserSettings table was updated
        var updatedSettings = await context.UserSettings.FirstOrDefaultAsync(us => us.UserId == user.Id);
        updatedSettings!.PreferredLanguage.Should().Be("en");
        updatedSettings!.PreferredTheme.Should().Be("dark");
        updatedSettings.ReceiveSystem.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateEmailAsync_ShouldThrowValidationException_OnDuplicateEmail()
    {
        using var context = CreateContext();
        var user1 = await SeedUserAsync(context, "user1@example.com");
        var user2 = await SeedUserAsync(context, "user2@example.com");

        var service = new UserSettingsService(context);

        // Try updating user1's email to user2's email
        Func<Task> act = async () => await service.UpdateEmailAsync(user1.Id, "user2@example.com", CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>();

        // Try updating user1's email to a new, unique email
        await service.UpdateEmailAsync(user1.Id, "unique@example.com", CancellationToken.None);
        var updatedUser = await context.Users.FindAsync(user1.Id);
        updatedUser!.Email.Should().Be("unique@example.com");
    }
}
