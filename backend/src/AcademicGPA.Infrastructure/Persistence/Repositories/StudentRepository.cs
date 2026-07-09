using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Persistence.Repositories;

public class StudentRepository : GenericRepository<StudentProfile>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<StudentProfile?> GetProfileWithAcademicHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Currently loads StudentProfile and User account details.
        // Child academic collections (AcademicYears, Semesters, Courses) will be joined here as they are implemented in future modules.
        return await DbContext.StudentProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);
    }

    public async Task<StudentProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsStudentCodeUniqueAsync(string studentCode, Guid? excludeProfileId = null, CancellationToken cancellationToken = default)
    {
        var query = DbContext.StudentProfiles.AsQueryable();

        if (excludeProfileId.HasValue)
        {
            query = query.Where(sp => sp.Id != excludeProfileId.Value);
        }

        var exists = await query.AnyAsync(sp => sp.StudentCode.ToLower() == studentCode.ToLower(), cancellationToken);
        return !exists;
    }
}
