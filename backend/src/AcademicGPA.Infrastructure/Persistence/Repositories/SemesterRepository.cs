using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Persistence.Repositories;

public class SemesterRepository : GenericRepository<Semester>, ISemesterRepository
{
    public SemesterRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Semester>> GetByAcademicYearIdAsync(Guid academicYearId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Semesters
            .Include(s => s.Courses.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Score)
            .Where(s => s.AcademicYearId == academicYearId && !s.IsDeleted)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.SemesterName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Semester?> GetByIdWithOwnerAsync(Guid id, Guid studentProfileId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Semesters
            .FirstOrDefaultAsync(s => s.Id == id 
                && s.AcademicYear.StudentProfileId == studentProfileId 
                && !s.IsDeleted 
                && !s.AcademicYear.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsSemesterNameUniqueAsync(Guid academicYearId, string semesterName, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Semesters
            .Where(s => s.AcademicYearId == academicYearId 
                && s.SemesterName.ToLower() == semesterName.ToLower() 
                && !s.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        var exists = await query.AnyAsync(cancellationToken);
        return !exists;
    }

    public async Task<int> CountByAcademicYearIdAsync(Guid academicYearId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Semesters
            .CountAsync(s => s.AcademicYearId == academicYearId && !s.IsDeleted, cancellationToken);
    }

    public async Task<bool> HasCoursesAsync(Guid semesterId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Courses.AnyAsync(c => c.SemesterId == semesterId && !c.IsDeleted, cancellationToken);
    }
}
