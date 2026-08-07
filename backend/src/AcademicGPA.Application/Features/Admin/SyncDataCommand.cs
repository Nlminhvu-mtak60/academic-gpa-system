using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Admin;

public record SyncUserDto(
    Guid Id,
    string Email,
    string PasswordHash,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    bool IsEmailVerified,
    string? GoogleId,
    string? AvatarUrl,
    string PreferredLanguage,
    string PreferredTheme,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastLoginAt,
    DateTime? LockedAt,
    string? LockReason,
    bool IsDeleted,
    bool ForcePasswordChange
);

public record SyncStudentProfileDto(
    Guid Id,
    Guid UserId,
    string StudentCode,
    string UniversityName,
    string MajorName,
    int EnrollmentYear,
    int TotalRequiredCredits
);

public record SyncAcademicYearDto(
    Guid Id,
    Guid StudentProfileId,
    string YearName,
    int StartYear,
    int EndYear,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    bool IsCurrent,
    int SortOrder,
    bool IsDeleted,
    DateTime CreatedAt
);

public record SyncSemesterDto(
    Guid Id,
    Guid AcademicYearId,
    string SemesterName,
    int SortOrder,
    bool IsDeleted,
    bool IsImported,
    int ImportedCredits,
    decimal ImportedGpa10,
    decimal ImportedGpa4,
    DateTime CreatedAt
);

public record SyncCourseDto(
    Guid Id,
    Guid SemesterId,
    string CourseCode,
    string CourseName,
    int Credits,
    bool IsRetake,
    Guid? OriginalCourseId,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record SyncScoreDto(
    Guid Id,
    Guid CourseId,
    decimal? AttendanceScore,
    decimal? ContinuousScore,
    decimal? FinalExamScore,
    decimal? CourseScore,
    string? LetterGrade,
    decimal? Gpa4Value,
    string? AcademicClassification,
    bool? IsPass,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record SyncScoreAuditLogDto(
    Guid Id,
    Guid CourseId,
    string FieldChanged,
    string? OldValue,
    string? NewValue,
    DateTime ChangedAt
);

public record SyncUserSettingDto(
    Guid Id,
    Guid UserId,
    string PreferredLanguage,
    string PreferredTheme,
    bool ReceiveSystem,
    bool ReceiveAcademic,
    bool ReceiveGoal,
    bool ReceiveGpaMilestone,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record SyncAcademicGoalDto(
    Guid Id,
    Guid StudentProfileId,
    decimal TargetCumulativeGpa10,
    decimal TargetCumulativeGpa4,
    string? Notes,
    bool IsAchieved,
    bool IsActive,
    DateTime CreatedAt
);

public record SyncNotificationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    bool IsBroadcast,
    Guid? SenderId,
    string? RecipientName,
    DateTime CreatedAt
);

public record SyncDataCommand(
    List<SyncUserDto> Users,
    List<SyncStudentProfileDto> StudentProfiles,
    List<SyncAcademicYearDto> AcademicYears,
    List<SyncSemesterDto> Semesters,
    List<SyncCourseDto> Courses,
    List<SyncScoreDto> Scores,
    List<SyncScoreAuditLogDto> ScoreAuditLogs,
    List<SyncUserSettingDto> UserSettings,
    List<SyncAcademicGoalDto> AcademicGoals,
    List<SyncNotificationDto> Notifications
) : IRequest<SyncDataResultDto>;

public record SyncDataResultDto(
    int UsersSynced,
    int StudentProfilesSynced,
    int AcademicYearsSynced,
    int SemestersSynced,
    int CoursesSynced,
    int ScoresSynced,
    int AuditLogsSynced,
    int SettingsSynced,
    int GoalsSynced,
    int NotificationsSynced
);

public class SyncDataCommandHandler : IRequestHandler<SyncDataCommand, SyncDataResultDto>
{
    private readonly IApplicationDbContext _context;

    public SyncDataCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SyncDataResultDto> Handle(SyncDataCommand request, CancellationToken cancellationToken)
    {
        // 1. Sync Users
        int usersCount = 0;
        foreach (var uDto in request.Users)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == uDto.Id || x.Email.ToLower() == uDto.Email.ToLower(), cancellationToken);

            var roleEnum = Enum.TryParse<UserRole>(uDto.Role, true, out var r) ? r : UserRole.Student;

            if (user == null)
            {
                user = new User
                {
                    Id = uDto.Id,
                    Email = uDto.Email.ToLower(),
                    PasswordHash = uDto.PasswordHash,
                    FirstName = uDto.FirstName,
                    LastName = uDto.LastName,
                    Role = roleEnum,
                    IsActive = uDto.IsActive,
                    IsEmailVerified = uDto.IsEmailVerified,
                    GoogleId = uDto.GoogleId,
                    AvatarUrl = uDto.AvatarUrl,
                    PreferredLanguage = uDto.PreferredLanguage,
                    PreferredTheme = uDto.PreferredTheme,
                    CreatedAt = uDto.CreatedAt,
                    UpdatedAt = uDto.UpdatedAt,
                    LastLoginAt = uDto.LastLoginAt,
                    LockedAt = uDto.LockedAt,
                    LockReason = uDto.LockReason,
                    IsDeleted = uDto.IsDeleted,
                    ForcePasswordChange = uDto.ForcePasswordChange
                };
                _context.Users.Add(user);
            }
            else
            {
                user.PasswordHash = uDto.PasswordHash;
                user.FirstName = uDto.FirstName;
                user.LastName = uDto.LastName;
                user.Role = roleEnum;
                user.IsActive = uDto.IsActive;
                user.IsEmailVerified = uDto.IsEmailVerified;
                if (!string.IsNullOrEmpty(uDto.GoogleId)) user.GoogleId = uDto.GoogleId;
                if (!string.IsNullOrEmpty(uDto.AvatarUrl)) user.AvatarUrl = uDto.AvatarUrl;
                user.PreferredLanguage = uDto.PreferredLanguage;
                user.PreferredTheme = uDto.PreferredTheme;
                user.UpdatedAt = uDto.UpdatedAt;
            }
            usersCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Sync StudentProfiles
        int profilesCount = 0;
        foreach (var spDto in request.StudentProfiles)
        {
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(x => x.Id == spDto.Id || x.UserId == spDto.UserId, cancellationToken);

            if (profile == null)
            {
                profile = new StudentProfile
                {
                    Id = spDto.Id,
                    UserId = spDto.UserId,
                    StudentCode = spDto.StudentCode,
                    UniversityName = spDto.UniversityName,
                    MajorName = spDto.MajorName,
                    EnrollmentYear = spDto.EnrollmentYear,
                    TotalRequiredCredits = spDto.TotalRequiredCredits
                };
                _context.StudentProfiles.Add(profile);
            }
            else
            {
                profile.StudentCode = spDto.StudentCode;
                profile.UniversityName = spDto.UniversityName;
                profile.MajorName = spDto.MajorName;
                profile.EnrollmentYear = spDto.EnrollmentYear;
                profile.TotalRequiredCredits = spDto.TotalRequiredCredits;
            }
            profilesCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Sync AcademicYears
        int yearsCount = 0;
        foreach (var ayDto in request.AcademicYears)
        {
            var ay = await _context.AcademicYears
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == ayDto.Id, cancellationToken);

            if (ay == null)
            {
                ay = new AcademicYear
                {
                    Id = ayDto.Id,
                    StudentProfileId = ayDto.StudentProfileId,
                    YearName = ayDto.YearName,
                    StartYear = ayDto.StartYear,
                    EndYear = ayDto.EndYear,
                    StartDate = ayDto.StartDate,
                    EndDate = ayDto.EndDate,
                    Status = ayDto.Status,
                    IsCurrent = ayDto.IsCurrent,
                    SortOrder = ayDto.SortOrder,
                    IsDeleted = ayDto.IsDeleted,
                    CreatedAt = ayDto.CreatedAt
                };
                _context.AcademicYears.Add(ay);
            }
            else
            {
                ay.YearName = ayDto.YearName;
                ay.StartYear = ayDto.StartYear;
                ay.EndYear = ayDto.EndYear;
                ay.StartDate = ayDto.StartDate;
                ay.EndDate = ayDto.EndDate;
                ay.Status = ayDto.Status;
                ay.IsCurrent = ayDto.IsCurrent;
                ay.SortOrder = ayDto.SortOrder;
                ay.IsDeleted = ayDto.IsDeleted;
            }
            yearsCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Sync Semesters
        int semestersCount = 0;
        foreach (var sDto in request.Semesters)
        {
            var sem = await _context.Semesters
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == sDto.Id, cancellationToken);

            if (sem == null)
            {
                sem = new Semester
                {
                    Id = sDto.Id,
                    AcademicYearId = sDto.AcademicYearId,
                    SemesterName = sDto.SemesterName,
                    SortOrder = sDto.SortOrder,
                    IsDeleted = sDto.IsDeleted,
                    IsImported = sDto.IsImported,
                    ImportedCredits = sDto.ImportedCredits,
                    ImportedGpa10 = sDto.ImportedGpa10,
                    ImportedGpa4 = sDto.ImportedGpa4,
                    CreatedAt = sDto.CreatedAt
                };
                _context.Semesters.Add(sem);
            }
            else
            {
                sem.SemesterName = sDto.SemesterName;
                sem.SortOrder = sDto.SortOrder;
                sem.IsDeleted = sDto.IsDeleted;
                sem.IsImported = sDto.IsImported;
                sem.ImportedCredits = sDto.ImportedCredits;
                sem.ImportedGpa10 = sDto.ImportedGpa10;
                sem.ImportedGpa4 = sDto.ImportedGpa4;
            }
            semestersCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Sync Courses
        int coursesCount = 0;
        foreach (var cDto in request.Courses)
        {
            var c = await _context.Courses
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == cDto.Id, cancellationToken);

            if (c == null)
            {
                c = new Course
                {
                    Id = cDto.Id,
                    SemesterId = cDto.SemesterId,
                    CourseCode = cDto.CourseCode,
                    CourseName = cDto.CourseName,
                    Credits = cDto.Credits,
                    IsRetake = cDto.IsRetake,
                    OriginalCourseId = cDto.OriginalCourseId,
                    IsDeleted = cDto.IsDeleted,
                    CreatedAt = cDto.CreatedAt,
                    UpdatedAt = cDto.UpdatedAt
                };
                _context.Courses.Add(c);
            }
            else
            {
                c.CourseCode = cDto.CourseCode;
                c.CourseName = cDto.CourseName;
                c.Credits = cDto.Credits;
                c.IsRetake = cDto.IsRetake;
                c.OriginalCourseId = cDto.OriginalCourseId;
                c.IsDeleted = cDto.IsDeleted;
                c.UpdatedAt = cDto.UpdatedAt;
            }
            coursesCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Sync Scores
        int scoresCount = 0;
        foreach (var scDto in request.Scores)
        {
            var sc = await _context.Scores
                .FirstOrDefaultAsync(x => x.Id == scDto.Id || x.CourseId == scDto.CourseId, cancellationToken);

            if (sc == null)
            {
                sc = new Score
                {
                    Id = scDto.Id,
                    CourseId = scDto.CourseId,
                    AttendanceScore = scDto.AttendanceScore,
                    ContinuousScore = scDto.ContinuousScore,
                    FinalExamScore = scDto.FinalExamScore,
                    CourseScore = scDto.CourseScore,
                    LetterGrade = scDto.LetterGrade,
                    Gpa4Value = scDto.Gpa4Value,
                    AcademicClassification = scDto.AcademicClassification,
                    IsPass = scDto.IsPass,
                    CreatedAt = scDto.CreatedAt,
                    UpdatedAt = scDto.UpdatedAt
                };
                _context.Scores.Add(sc);
            }
            else
            {
                sc.AttendanceScore = scDto.AttendanceScore;
                sc.ContinuousScore = scDto.ContinuousScore;
                sc.FinalExamScore = scDto.FinalExamScore;
                sc.CourseScore = scDto.CourseScore;
                sc.LetterGrade = scDto.LetterGrade;
                sc.Gpa4Value = scDto.Gpa4Value;
                sc.AcademicClassification = scDto.AcademicClassification;
                sc.IsPass = scDto.IsPass;
                sc.UpdatedAt = scDto.UpdatedAt;
            }
            scoresCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Sync ScoreAuditLogs
        int auditCount = 0;
        foreach (var logDto in request.ScoreAuditLogs)
        {
            var log = await _context.ScoreAuditLogs
                .FirstOrDefaultAsync(x => x.Id == logDto.Id, cancellationToken);

            if (log == null)
            {
                log = new ScoreAuditLog
                {
                    Id = logDto.Id,
                    CourseId = logDto.CourseId,
                    FieldChanged = logDto.FieldChanged,
                    OldValue = logDto.OldValue,
                    NewValue = logDto.NewValue,
                    ChangedAt = logDto.ChangedAt
                };
                _context.ScoreAuditLogs.Add(log);
            }
            auditCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 8. Sync UserSettings
        int settingsCount = 0;
        foreach (var setDto in request.UserSettings)
        {
            var us = await _context.UserSettings
                .FirstOrDefaultAsync(x => x.Id == setDto.Id || x.UserId == setDto.UserId, cancellationToken);

            if (us == null)
            {
                us = new UserSettings
                {
                    Id = setDto.Id,
                    UserId = setDto.UserId,
                    PreferredLanguage = setDto.PreferredLanguage,
                    PreferredTheme = setDto.PreferredTheme,
                    ReceiveSystem = setDto.ReceiveSystem,
                    ReceiveAcademic = setDto.ReceiveAcademic,
                    ReceiveGoal = setDto.ReceiveGoal,
                    ReceiveGpaMilestone = setDto.ReceiveGpaMilestone,
                    CreatedAt = setDto.CreatedAt,
                    UpdatedAt = setDto.UpdatedAt
                };
                _context.UserSettings.Add(us);
            }
            settingsCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 9. Sync AcademicGoals
        int goalsCount = 0;
        foreach (var gDto in request.AcademicGoals)
        {
            var goal = await _context.AcademicGoals
                .FirstOrDefaultAsync(x => x.Id == gDto.Id, cancellationToken);

            if (goal == null)
            {
                goal = new AcademicGoal
                {
                    Id = gDto.Id,
                    StudentProfileId = gDto.StudentProfileId,
                    TargetCumulativeGpa10 = gDto.TargetCumulativeGpa10,
                    TargetCumulativeGpa4 = gDto.TargetCumulativeGpa4,
                    Notes = gDto.Notes,
                    IsAchieved = gDto.IsAchieved,
                    IsActive = gDto.IsActive,
                    CreatedAt = gDto.CreatedAt
                };
                _context.AcademicGoals.Add(goal);
            }
            goalsCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        // 10. Sync Notifications
        int notifCount = 0;
        foreach (var nDto in request.Notifications)
        {
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == nDto.Id, cancellationToken);

            if (notif == null)
            {
                notif = new Notification
                {
                    Id = nDto.Id,
                    UserId = nDto.UserId,
                    Title = nDto.Title,
                    Message = nDto.Message,
                    Type = nDto.Type,
                    IsRead = nDto.IsRead,
                    IsBroadcast = nDto.IsBroadcast,
                    SenderId = nDto.SenderId,
                    RecipientName = nDto.RecipientName,
                    CreatedAt = nDto.CreatedAt
                };
                _context.Notifications.Add(notif);
            }
            notifCount++;
        }
        await _context.SaveChangesAsync(cancellationToken);

        return new SyncDataResultDto(
            usersCount, profilesCount, yearsCount, semestersCount, coursesCount,
            scoresCount, auditCount, settingsCount, goalsCount, notifCount
        );
    }
}
