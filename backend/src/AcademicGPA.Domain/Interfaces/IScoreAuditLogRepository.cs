using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;

namespace AcademicGPA.Domain.Interfaces;

/// <summary>
/// Custom repository contract for managing ScoreAuditLogs.
/// </summary>
public interface IScoreAuditLogRepository : IGenericRepository<ScoreAuditLog>
{
    /// <summary>
    /// Fetches all audit logs for a specific course.
    /// </summary>
    Task<IReadOnlyList<ScoreAuditLog>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
}
