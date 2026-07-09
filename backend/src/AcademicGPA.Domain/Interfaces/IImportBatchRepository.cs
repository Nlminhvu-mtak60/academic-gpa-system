using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;

namespace AcademicGPA.Domain.Interfaces;

public interface IImportBatchRepository : IGenericRepository<ImportBatch>
{
    Task<ImportBatch?> GetByIdWithCoursesAsync(Guid id, CancellationToken cancellationToken = default);
}
