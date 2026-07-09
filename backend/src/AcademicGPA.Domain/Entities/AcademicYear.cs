using System;
using System.Collections.Generic;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents an academic year belonging to a student, grouping semesters.
/// </summary>
public class AcademicYear
{
    /// <summary>
    /// Unique identifier for the academic year.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key referencing the associated StudentProfile.
    /// </summary>
    public Guid StudentProfileId { get; set; }

    /// <summary>
    /// Name of the academic year (e.g. "2024-2025").
    /// </summary>
    public string YearName { get; set; } = string.Empty;

    /// <summary>
    /// The starting calendar year (e.g. 2024).
    /// </summary>
    public int StartYear { get; set; }

    /// <summary>
    /// The ending calendar year (e.g. 2025).
    /// </summary>
    public int EndYear { get; set; }

    /// <summary>
    /// The start date of the academic year.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// The end date of the academic year.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// The status of the academic year ("Current" or "Completed").
    /// </summary>
    public string Status { get; set; } = "Completed";

    /// <summary>
    /// Flag indicating if this is the active current year.
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Sorting index order.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Soft-delete indicator.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Flag indicating if this is an imported historical summary record.
    /// </summary>
    public bool IsImported { get; set; }

    /// <summary>
    /// Total completed credits imported for this historical year.
    /// </summary>
    public int ImportedCredits { get; set; }

    /// <summary>
    /// Average GPA on 10-scale imported for this historical year.
    /// </summary>
    public decimal ImportedGpa10 { get; set; }

    /// <summary>
    /// Average GPA on 4-scale imported for this historical year.
    /// </summary>
    public decimal ImportedGpa4 { get; set; }

    /// <summary>
    /// Navigation property to the parent StudentProfile.
    /// </summary>
    public StudentProfile StudentProfile { get; set; } = null!;

    /// <summary>
    /// Navigation collection of child semesters.
    /// </summary>
    public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
}
