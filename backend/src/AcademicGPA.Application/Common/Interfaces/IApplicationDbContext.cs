using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Interface exposing the database context to the Application layer.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<StudentProfile> StudentProfiles { get; }
    DbSet<AcademicYear> AcademicYears { get; }
    DbSet<Semester> Semesters { get; }
    DbSet<Course> Courses { get; }
    DbSet<Score> Scores { get; }
    DbSet<ScoreAuditLog> ScoreAuditLogs { get; }
    DbSet<AcademicGoal> AcademicGoals { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<UserSettings> UserSettings { get; }
    DbSet<UserActivityLog> UserActivityLogs { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMessage> ConversationMessages { get; }
    DbSet<ImportBatch> ImportBatches { get; }
    DbSet<ImportBatchCourse> ImportBatchCourses { get; }
    DbSet<GradeScale> GradeScales { get; }
    DbSet<GradeScaleEntry> GradeScaleEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
