using System;
using System.Collections.Generic;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents an academic course belonging to a semester.
/// </summary>
public class Course
{
    /// <summary>
    /// Unique identifier for the course.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key referencing the parent Semester.
    /// </summary>
    public Guid SemesterId { get; set; }

    /// <summary>
    /// Unique identifier code of the course (e.g. "CS101").
    /// </summary>
    public string CourseCode { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the course.
    /// </summary>
    public string CourseName { get; set; } = string.Empty;

    /// <summary>
    /// Number of credits assigned to this course (1 to 6).
    /// </summary>
    public int Credits { get; set; } = 3;

    /// <summary>
    /// Flag indicating if this course is a retake of a previous attempt.
    /// </summary>
    public bool IsRetake { get; set; }

    /// <summary>
    /// Foreign key pointing to the original attempt course if this is a retake.
    /// </summary>
    public Guid? OriginalCourseId { get; set; }

    /// <summary>
    /// Soft-delete indicator.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Update timestamp (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the parent Semester.
    /// </summary>
    public Semester Semester { get; set; } = null!;

    /// <summary>
    /// Self-referencing navigation property to the original course attempt.
    /// </summary>
    public Course? OriginalCourse { get; set; }

    /// <summary>
    /// Collection of child retake attempts of this course.
    /// </summary>
    public ICollection<Course> Retakes { get; set; } = new List<Course>();

    /// <summary>
    /// Navigation property to the associated Score record.
    /// </summary>
    public Score? Score { get; set; }
}
