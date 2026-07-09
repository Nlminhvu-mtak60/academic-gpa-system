using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Notifications.DTOs;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service managing student notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Retrieves a paginated list of notifications for a student.
    /// </summary>
    Task<List<NotificationDto>> GetNotificationsAsync(
        Guid userId, int page, int pageSize, bool? unreadOnly, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves total count of notifications for a student.
    /// </summary>
    Task<int> GetTotalNotificationsCountAsync(
        Guid userId, bool? unreadOnly, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);

    /// <summary>
    /// Marks all unread notifications for a student as read.
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the count of unread notifications for a student.
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a specific notification.
    /// </summary>
    Task<bool> DeleteNotificationAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);

    /// <summary>
    /// dispatches a notification checking user preference before saving.
    /// </summary>
    Task SendNotificationAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken, bool saveChanges = true);
}
