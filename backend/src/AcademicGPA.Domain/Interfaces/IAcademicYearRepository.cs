using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;

namespace AcademicGPA.Domain.Interfaces;

/// <summary>
/// Custom repository contract for managing AcademicYears.
/// </summary>
public interface IAcademicYearRepository : IGenericRepository<AcademicYear>
{
    /// <summary>
    /// Fetches all active academic years belonging to a specific student profile.
    /// </summary>
    Task<IReadOnlyList<AcademicYear>> GetByStudentProfileIdAsync(Guid studentProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches an academic year by ID, verifying that it belongs to a student profile.
    /// </summary>
    Task<AcademicYear?> GetByIdWithOwnerAsync(Guid id, Guid studentProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an academic year name is unique for the student.
    /// </summary>
    Task<bool> IsYearNameUniqueAsync(Guid studentProfileId, string yearName, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches the current active academic year for a student, if any.
    /// </summary>
    Task<AcademicYear?> GetCurrentYearAsync(Guid studentProfileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the academic year contains any semesters.
    /// </summary>
    Task<bool> HasSemestersAsync(Guid id, CancellationToken cancellationToken = default);
}
