using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;

namespace AcademicGPA.Domain.Interfaces;

/// <summary>
/// Custom repository contract for managing Courses.
/// </summary>
public interface ICourseRepository : IGenericRepository<Course>
{
    /// <summary>
    /// Fetches all active courses belonging to a specific semester.
    /// </summary>
    Task<IReadOnlyList<Course>> GetBySemesterIdAsync(Guid semesterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a course by ID, verifying that it belongs to a semester/academic year owned by a student profile.
    /// </summary>
    Task<Course?> GetByIdWithOwnerAsync(Guid id, Guid studentProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a course name is unique within the same semester.
    /// </summary>
    Task<bool> IsCourseNameUniqueAsync(Guid semesterId, string courseName, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the course has any grade records associated.
    /// </summary>
    Task<bool> HasGradesAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all courses with the given code that belong to the student, excluding any that are already marked as retakes.
    /// Used for listing candidates that can be selected as the "Original Attempt".
    /// </summary>
    Task<IReadOnlyList<Course>> GetEligibleOriginalCoursesAsync(Guid studentProfileId, string courseCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a course ID is a valid original attempt candidates (must belong to student, match course code, not be a retake itself).
    /// </summary>
    Task<bool> IsValidOriginalCourseAsync(Guid originalCourseId, Guid studentProfileId, string currentCourseCode, CancellationToken cancellationToken = default);
}
