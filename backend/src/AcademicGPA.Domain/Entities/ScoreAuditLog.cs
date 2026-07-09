using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Audit log entry tracking score component adjustments for a course.
/// </summary>
public class ScoreAuditLog
{
    /// <summary>
    /// Unique identifier for the audit log record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the Course whose scores were changed.
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Name of the field changed (e.g. "AttendanceScore", "ContinuousScore", "FinalExamScore").
    /// </summary>
    public string FieldChanged { get; set; } = string.Empty;

    /// <summary>
    /// String representation of the value prior to modification (null if not set previously).
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// String representation of the new value after modification.
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Timestamp of when the change occurred (UTC).
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to Course.
    /// </summary>
    public Course Course { get; set; } = null!;
}
