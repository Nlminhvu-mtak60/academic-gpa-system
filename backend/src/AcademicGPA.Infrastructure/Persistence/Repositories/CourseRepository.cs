using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Persistence.Repositories;

public class CourseRepository : GenericRepository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Course>> GetBySemesterIdAsync(Guid semesterId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Courses
            .Include(c => c.Score)
            .Where(c => c.SemesterId == semesterId && !c.IsDeleted)
            .OrderBy(c => c.CourseName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Course?> GetByIdWithOwnerAsync(Guid id, Guid studentProfileId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Courses
            .FirstOrDefaultAsync(c => c.Id == id 
                && c.Semester.AcademicYear.StudentProfileId == studentProfileId 
                && !c.IsDeleted 
                && !c.Semester.IsDeleted 
                && !c.Semester.AcademicYear.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsCourseNameUniqueAsync(Guid semesterId, string courseName, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Courses
            .Where(c => c.SemesterId == semesterId 
                && c.CourseName.ToLower() == courseName.ToLower() 
                && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        var exists = await query.AnyAsync(cancellationToken);
        return !exists;
    }

    public async Task<bool> HasGradesAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Scores
            .AnyAsync(s => s.CourseId == courseId 
                && (s.AttendanceScore != null || s.ContinuousScore != null || s.FinalExamScore != null), cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetEligibleOriginalCoursesAsync(Guid studentProfileId, string courseCode, CancellationToken cancellationToken = default)
    {
        return await DbContext.Courses
            .Where(c => c.Semester.AcademicYear.StudentProfileId == studentProfileId 
                && c.CourseCode.ToLower() == courseCode.ToLower() 
                && !c.IsRetake 
                && !c.IsDeleted 
                && !c.Semester.IsDeleted 
                && !c.Semester.AcademicYear.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsValidOriginalCourseAsync(Guid originalCourseId, Guid studentProfileId, string currentCourseCode, CancellationToken cancellationToken = default)
    {
        return await DbContext.Courses
            .AnyAsync(c => c.Id == originalCourseId 
                && c.Semester.AcademicYear.StudentProfileId == studentProfileId 
                && c.CourseCode.ToLower() == currentCourseCode.ToLower() 
                && !c.IsRetake 
                && !c.IsDeleted 
                && !c.Semester.IsDeleted 
                && !c.Semester.AcademicYear.IsDeleted, cancellationToken);
    }
}
