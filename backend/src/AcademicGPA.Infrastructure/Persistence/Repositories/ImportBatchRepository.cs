using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using AcademicGPA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Persistence.Repositories;

public class ImportBatchRepository : GenericRepository<ImportBatch>, IImportBatchRepository
{
    public ImportBatchRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ImportBatch?> GetByIdWithCoursesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<ImportBatch>()
            .Include(ib => ib.ImportBatchCourses)
            .ThenInclude(ibc => ibc.Course)
            .FirstOrDefaultAsync(ib => ib.Id == id, cancellationToken);
    }
}
