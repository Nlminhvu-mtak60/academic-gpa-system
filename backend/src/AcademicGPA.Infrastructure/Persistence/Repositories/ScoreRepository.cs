using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Persistence.Repositories;

/// <summary>
/// Entity Framework Core repository implementation for managing Scores.
/// </summary>
public class ScoreRepository : GenericRepository<Score>, IScoreRepository
{
    public ScoreRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<Score?> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Scores
            .FirstOrDefaultAsync(s => s.CourseId == courseId, cancellationToken);
    }
}
