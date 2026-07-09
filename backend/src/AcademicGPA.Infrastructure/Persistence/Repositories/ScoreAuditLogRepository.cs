using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core repository implementation for managing ScoreAuditLogs.
/// </summary>
public class ScoreAuditLogRepository : GenericRepository<ScoreAuditLog>, IScoreAuditLogRepository
{
    public ScoreAuditLogRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<ScoreAuditLog>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await DbContext.ScoreAuditLogs
            .Where(sal => sal.CourseId == courseId)
            .OrderByDescending(sal => sal.ChangedAt)
            .ToListAsync(cancellationToken);
    }
}
