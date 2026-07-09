using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Features.Notifications.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcademicGPA.UnitTests;

public class NotificationServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public NotificationServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    private async Task<User> SeedUserAsync(ApplicationDbContext context)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "hash"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task GetNotificationsAsync_ShouldReturnPaginatedAndFiltered()
    {
        using var context = CreateContext();
        var user = await SeedUserAsync(context);

        // Add 3 notifications: 2 unread, 1 read
        context.Notifications.AddRange(
            new Notification { UserId = user.Id, Title = "N1", Message = "M1", Type = "Academic", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new Notification { UserId = user.Id, Title = "N2", Message = "M2", Type = "Goal", IsRead = true, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new Notification { UserId = user.Id, Title = "N3", Message = "M3", Type = "System", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-3) }
        );
        await context.SaveChangesAsync();

        var service = new NotificationService(context);

        // Get unread only, page 1, size 10
        var unreadList = await service.GetNotificationsAsync(user.Id, 1, 10, true, CancellationToken.None);
        unreadList.Should().HaveCount(2);
        unreadList.Select(n => n.Title).Should().ContainInOrder("N3", "N1"); // Ordered by CreatedAt desc

        // Get all, page 1, size 2 (pagination check)
        var allList = await service.GetNotificationsAsync(user.Id, 1, 2, null, CancellationToken.None);
        allList.Should().HaveCount(2);
        allList.Select(n => n.Title).Should().ContainInOrder("N3", "N2"); // Ordered by CreatedAt desc
    }

    [Fact]
    public async Task MarkAsReadAsync_ShouldUpdateStateCorrectly()
    {
        using var context = CreateContext();
        var user = await SeedUserAsync(context);
        var notif = new Notification { UserId = user.Id, Title = "N1", Message = "M1", Type = "Academic", IsRead = false };
        context.Notifications.Add(notif);
        await context.SaveChangesAsync();

        var service = new NotificationService(context);
        var success = await service.MarkAsReadAsync(user.Id, notif.Id, CancellationToken.None);
        success.Should().BeTrue();

        var updatedNotif = await context.Notifications.FindAsync(notif.Id);
        updatedNotif!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ShouldMarkAllUnread()
    {
        using var context = CreateContext();
        var user = await SeedUserAsync(context);
        context.Notifications.AddRange(
            new Notification { UserId = user.Id, Title = "N1", IsRead = false },
            new Notification { UserId = user.Id, Title = "N2", IsRead = false },
            new Notification { UserId = user.Id, Title = "N3", IsRead = true }
        );
        await context.SaveChangesAsync();

        var service = new NotificationService(context);
        await service.MarkAllAsReadAsync(user.Id, CancellationToken.None);

        var allRead = await context.Notifications.AllAsync(n => n.IsRead);
        allRead.Should().BeTrue();
    }

    [Fact]
    public async Task SendNotificationAsync_ShouldRespectUserPreferences()
    {
        using var context = CreateContext();
        var user = await SeedUserAsync(context);

        // Turn off Academic, keep Goal on
        var settings = new UserSettings
        {
            UserId = user.Id,
            ReceiveAcademic = false,
            ReceiveGoal = true
        };
        context.UserSettings.Add(settings);
        await context.SaveChangesAsync();

        var service = new NotificationService(context);

        // Try dispatching Academic notification
        await service.SendNotificationAsync(user.Id, "Academic Title", "Academic Msg", "Academic", CancellationToken.None);
        // Try dispatching Goal notification
        await service.SendNotificationAsync(user.Id, "Goal Title", "Goal Msg", "Goal", CancellationToken.None);

        var notifications = await context.Notifications.ToListAsync();
        notifications.Should().HaveCount(1);
        notifications[0].Type.Should().Be("Goal");
    }

    [Fact]
    public async Task GetUnreadCountAsync_ShouldReturnUnreadCountOnly()
    {
        using var context = CreateContext();
        var user = await SeedUserAsync(context);
        context.Notifications.AddRange(
            new Notification { UserId = user.Id, Title = "N1", IsRead = false },
            new Notification { UserId = user.Id, Title = "N2", IsRead = true }
        );
        await context.SaveChangesAsync();

        var service = new NotificationService(context);
        var unreadCount = await service.GetUnreadCountAsync(user.Id, CancellationToken.None);
        unreadCount.Should().Be(1);
    }
}
