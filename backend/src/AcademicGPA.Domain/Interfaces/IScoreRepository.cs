using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;

namespace AcademicGPA.Domain.Interfaces;

/// <summary>
/// Custom repository contract for managing Scores.
/// </summary>
public interface IScoreRepository : IGenericRepository<Score>
{
    /// <summary>
    /// Fetches the Score record for a specific course.
    /// </summary>
    Task<Score?> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
}
