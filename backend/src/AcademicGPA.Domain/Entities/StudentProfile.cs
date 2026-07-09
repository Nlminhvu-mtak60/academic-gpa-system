namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents the university-specific academic profile of a student.
/// </summary>
public class StudentProfile
{
    /// <summary>
    /// Unique identifier for the student profile.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key referencing the associated User.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Student Code (MSSV) assigned by the university. Acts as unique identifier.
    /// </summary>
    public string StudentCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the university.
    /// </summary>
    public string UniversityName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the student's major.
    /// </summary>
    public string MajorName { get; set; } = string.Empty;

    /// <summary>
    /// Year of enrollment (e.g. 2024).
    /// </summary>
    public int EnrollmentYear { get; set; }

    /// <summary>
    /// Total credits required for graduation in this major.
    /// </summary>
    public int TotalRequiredCredits { get; set; }

    /// <summary>
    /// Navigation property to the owner user account.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Collection of academic years owned by this student.
    /// </summary>
    public ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();

    /// <summary>
    /// Collection of GPA goals set by this student.
    /// </summary>
    public ICollection<AcademicGoal> AcademicGoals { get; set; } = new List<AcademicGoal>();
}
