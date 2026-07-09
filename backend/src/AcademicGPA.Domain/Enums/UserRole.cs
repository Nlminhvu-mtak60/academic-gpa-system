namespace AcademicGPA.Domain.Enums;

/// <summary>
/// Defines the system roles for users.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Student role with standard grade tracking capabilities.
    /// </summary>
    Student = 1,

    /// <summary>
    /// Administrator role with student lock, password reset, and announcement broadcast tools.
    /// </summary>
    Admin = 2
}
