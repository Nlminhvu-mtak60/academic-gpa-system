using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;

namespace AcademicGPA.Domain.Interfaces;

/// <summary>
/// Custom repository contract for managing Semesters.
/// </summary>
public interface ISemesterRepository : IGenericRepository<Semester>
{
    /// <summary>
    /// Fetches all active semesters belonging to a specific academic year.
    /// </summary>
    Task<IReadOnlyList<Semester>> GetByAcademicYearIdAsync(Guid academicYearId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a semester by ID, verifying that it belongs to a specific academic year owned by a student profile.
    /// </summary>
    Task<Semester?> GetByIdWithOwnerAsync(Guid id, Guid studentProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a semester name is unique within the same academic year.
    /// </summary>
    Task<bool> IsSemesterNameUniqueAsync(Guid academicYearId, string semesterName, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the count of active semesters within an academic year.
    /// </summary>
    Task<int> CountByAcademicYearIdAsync(Guid academicYearId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the semester contains any courses.
    /// </summary>
    Task<bool> HasCoursesAsync(Guid semesterId, CancellationToken cancellationToken = default);
}
