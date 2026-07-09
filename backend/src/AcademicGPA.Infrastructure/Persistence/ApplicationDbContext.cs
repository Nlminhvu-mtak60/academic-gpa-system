using System.Reflection;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Persistence;

/// <summary>
/// Database context for the Academic GPA Management System implementing IApplicationDbContext.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<ScoreAuditLog> ScoreAuditLogs => Set<ScoreAuditLog>();
    public DbSet<AcademicGoal> AcademicGoals => Set<AcademicGoal>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();
    public DbSet<ImportBatchCourse> ImportBatchCourses => Set<ImportBatchCourse>();
    public DbSet<GradeScale> GradeScales => Set<GradeScale>();
    public DbSet<GradeScaleEntry> GradeScaleEntries => Set<GradeScaleEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applies all configurations from the current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
