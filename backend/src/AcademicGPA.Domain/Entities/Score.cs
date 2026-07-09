using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Holds course-level component scores and calculated academic results.
/// </summary>
public class Score
{
    /// <summary>
    /// Unique identifier for the Score record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the parent Course.
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Attendance component score (10%). Range 0.0 - 10.0, rounded to nearest 0.5.
    /// </summary>
    public decimal? AttendanceScore { get; set; }

    /// <summary>
    /// Continuous Assessment component score (30%). Range 0.0 - 10.0, rounded to nearest 0.5.
    /// </summary>
    public decimal? ContinuousScore { get; set; }

    /// <summary>
    /// Final Exam component score (60%). Range 0.0 - 10.0, rounded to nearest 0.5.
    /// </summary>
    public decimal? FinalExamScore { get; set; }

    /// <summary>
    /// Total calculated course score. Formula: A*0.1 + C*0.3 + F*0.6. Rounded to 1 decimal place.
    /// </summary>
    public decimal? CourseScore { get; set; }

    /// <summary>
    /// Letter grade equivalent (A+, A, B+, B, C+, C, D+, D, F).
    /// </summary>
    public string? LetterGrade { get; set; }

    /// <summary>
    /// GPA 4 value translation (4.0, 3.7, 3.5, 3.0, 2.5, 2.0, 1.5, 1.0, 0.0).
    /// </summary>
    public decimal? Gpa4Value { get; set; }

    /// <summary>
    /// Academic Classification (Poor, Weak, Average, Average Good, Good, Very Good, Excellent, Outstanding).
    /// </summary>
    public string? AcademicClassification { get; set; }

    /// <summary>
    /// Pass/Fail Status (true if CourseScore >= 4.0, false otherwise).
    /// </summary>
    public bool? IsPass { get; set; }

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to parent Course.
    /// </summary>
    public Course Course { get; set; } = null!;
}
