using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Notifications.DTOs;
using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Services;

/// <summary>
/// Implements notification CRUD, pagination, read receipts, and preference-filtered dispatch.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;

    public NotificationService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<List<NotificationDto>> GetNotificationsAsync(
        Guid userId, int page, int pageSize, bool? unreadOnly, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        var list = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return list;
    }

    /// <inheritdoc />
    public async Task<int> GetTotalNotificationsCountAsync(
        Guid userId, bool? unreadOnly, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        return await query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken)
    {
        var notif = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notif == null) return false;

        if (!notif.IsRead)
        {
            notif.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    /// <inheritdoc />
    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var n in unread)
        {
            n.IsRead = true;
        }

        if (unread.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteNotificationAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken)
    {
        var notif = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notif == null) return false;

        _context.Notifications.Remove(notif);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task SendNotificationAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken, bool saveChanges = true)
    {
        // 1. Check user notification preferences
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId, cancellationToken);

        // If settings don't exist yet, we assume they are true by default
        bool receive = true;
        if (settings != null)
        {
            receive = type.ToLower() switch
            {
                "system" => settings.ReceiveSystem,
                "academic" => settings.ReceiveAcademic,
                "goal" => settings.ReceiveGoal,
                "gpamilestone" => settings.ReceiveGpaMilestone,
                _ => true
            };
        }

        if (!receive) return;

        // 2. Create and save notification
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
