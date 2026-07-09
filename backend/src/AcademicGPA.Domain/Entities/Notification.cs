using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a notification sent to a student.
/// </summary>
public class Notification
{
    /// <summary>
    /// Unique identifier for the notification.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the recipient user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the recipient user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Title of the notification.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed text/message content of the notification.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Category/type of notification (e.g. System, Academic, Goal, GpaMilestone).
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the student has read the notification.
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Indicates whether the notification is a system-wide broadcast announcement.
    /// </summary>
    public bool IsBroadcast { get; set; } = false;

    /// <summary>
    /// Identifier of the administrator user who sent the notification, if applicable.
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// Display name of the recipient (used in admin notification history logs).
    /// </summary>
    public string? RecipientName { get; set; }

    /// <summary>
    /// Date and time when the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
