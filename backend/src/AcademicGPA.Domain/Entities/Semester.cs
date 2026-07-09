using System;
using System.Collections.Generic;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a semester within an academic year.
/// </summary>
public class Semester
{
    /// <summary>
    /// Unique identifier for the semester.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key referencing the parent AcademicYear.
    /// </summary>
    public Guid AcademicYearId { get; set; }

    /// <summary>
    /// Display name of the semester (e.g. "Semester 1", "Summer Semester").
    /// </summary>
    public string SemesterName { get; set; } = string.Empty;

    /// <summary>
    /// Ordering position within the academic year.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Soft-delete indicator.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Flag indicating if this is an imported historical summary record.
    /// </summary>
    public bool IsImported { get; set; }

    /// <summary>
    /// Total completed credits imported for this historical semester.
    /// </summary>
    public int ImportedCredits { get; set; }

    /// <summary>
    /// Average GPA on 10-scale imported for this historical semester.
    /// </summary>
    public decimal ImportedGpa10 { get; set; }

    /// <summary>
    /// Average GPA on 4-scale imported for this historical semester.
    /// </summary>
    public decimal ImportedGpa4 { get; set; }

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the parent AcademicYear.
    /// </summary>
    public AcademicYear AcademicYear { get; set; } = null!;

    /// <summary>
    /// Navigation collection of child courses.
    /// </summary>
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
