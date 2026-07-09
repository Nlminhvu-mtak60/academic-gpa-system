using AcademicGPA.Domain.Entities;

namespace AcademicGPA.Domain.Interfaces;

/// <summary>
/// Custom repository contract for StudentProfile specific aggregator queries.
/// </summary>
public interface IStudentRepository : IGenericRepository<StudentProfile>
{
    /// <summary>
    /// Loads a student profile along with the full academic history (years, semesters, courses, scores) in a single transaction.
    /// </summary>
    Task<StudentProfile?> GetProfileWithAcademicHistoryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a student profile by UserId.
    /// </summary>
    Task<StudentProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a StudentCode is unique.
    /// </summary>
    Task<bool> IsStudentCodeUniqueAsync(string studentCode, Guid? excludeProfileId = null, CancellationToken cancellationToken = default);
}
