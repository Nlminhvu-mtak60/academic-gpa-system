using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents an audit log of user or admin activities within the system.
/// </summary>
public class UserActivityLog
{
    /// <summary>
    /// Unique identifier for the audit log entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the user associated with this activity.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the associated user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Description of the activity performed (e.g., "Login", "Lock Account", "Reset Password").
    /// </summary>
    public string Activity { get; set; } = string.Empty;

    /// <summary>
    /// IP address from which the activity originated.
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the activity was logged.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
