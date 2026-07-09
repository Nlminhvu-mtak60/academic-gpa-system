using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Persistence.Repositories;

public class AcademicYearRepository : GenericRepository<AcademicYear>, IAcademicYearRepository
{
    public AcademicYearRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<AcademicYear>> GetByStudentProfileIdAsync(Guid studentProfileId, CancellationToken cancellationToken = default)
    {
        return await DbContext.AcademicYears
            .Include(ay => ay.Semesters.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Score)
            .Where(ay => ay.StudentProfileId == studentProfileId && !ay.IsDeleted)
            .OrderBy(ay => ay.SortOrder)
            .ThenBy(ay => ay.StartYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<AcademicYear?> GetByIdWithOwnerAsync(Guid id, Guid studentProfileId, CancellationToken cancellationToken = default)
    {
        return await DbContext.AcademicYears
            .FirstOrDefaultAsync(ay => ay.Id == id && ay.StudentProfileId == studentProfileId && !ay.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsYearNameUniqueAsync(Guid studentProfileId, string yearName, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbContext.AcademicYears
            .Where(ay => ay.StudentProfileId == studentProfileId && ay.YearName.ToLower() == yearName.ToLower() && !ay.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(ay => ay.Id != excludeId.Value);
        }

        var exists = await query.AnyAsync(cancellationToken);
        return !exists;
    }

    public async Task<AcademicYear?> GetCurrentYearAsync(Guid studentProfileId, CancellationToken cancellationToken = default)
    {
        return await DbContext.AcademicYears
            .FirstOrDefaultAsync(ay => ay.StudentProfileId == studentProfileId && ay.IsCurrent && !ay.IsDeleted, cancellationToken);
    }

    public async Task<bool> HasSemestersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Semesters.AnyAsync(s => s.AcademicYearId == id && !s.IsDeleted, cancellationToken);
    }
}
