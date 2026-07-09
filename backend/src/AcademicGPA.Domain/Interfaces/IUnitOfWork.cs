namespace AcademicGPA.Domain.Interfaces;

/// <summary>
/// coordinates transactions across multiple repositories.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IStudentRepository Students { get; }
    IAcademicYearRepository AcademicYears { get; }
    ISemesterRepository Semesters { get; }
    ICourseRepository Courses { get; }
    IScoreRepository Scores { get; }
    IScoreAuditLogRepository ScoreAuditLogs { get; }
    IImportBatchRepository ImportBatches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
