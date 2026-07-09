using AcademicGPA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace AcademicGPA.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private IDbContextTransaction? _transaction;
    private IStudentRepository? _studentRepository;
    private IAcademicYearRepository? _academicYearRepository;
    private ISemesterRepository? _semesterRepository;
    private ICourseRepository? _courseRepository;
    private IScoreRepository? _scoreRepository;
    private IScoreAuditLogRepository? _scoreAuditLogRepository;
    private IImportBatchRepository? _importBatchRepository;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IStudentRepository Students => _studentRepository ??= new StudentRepository(_dbContext);
    public IAcademicYearRepository AcademicYears => _academicYearRepository ??= new AcademicYearRepository(_dbContext);
    public ISemesterRepository Semesters => _semesterRepository ??= new SemesterRepository(_dbContext);
    public ICourseRepository Courses => _courseRepository ??= new CourseRepository(_dbContext);
    public IScoreRepository Scores => _scoreRepository ??= new ScoreRepository(_dbContext);
    public IScoreAuditLogRepository ScoreAuditLogs => _scoreAuditLogRepository ??= new ScoreAuditLogRepository(_dbContext);
    public IImportBatchRepository ImportBatches => _importBatchRepository ??= new ImportBatchRepository(_dbContext);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
}
