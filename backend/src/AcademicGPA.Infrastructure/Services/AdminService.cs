using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Admin.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Services;

/// <summary>
/// Implements administration controls, student management, system statistics, and notification broadcasts.
/// </summary>
public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public AdminService(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc />
    public async Task<AdminStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        // 1. User stats (students only)
        var totalStudents = await _context.Users
            .CountAsync(u => u.Role == UserRole.Student, cancellationToken);

        var activeStudents = await _context.Users
            .CountAsync(u => u.Role == UserRole.Student && u.IsActive, cancellationToken);

        var lockedAccounts = totalStudents - activeStudents;

        // 2. Load all profiles with courses to compute GPAs
        var profiles = await _context.StudentProfiles
            .Include(sp => sp.AcademicYears.Where(ay => !ay.IsDeleted))
                .ThenInclude(ay => ay.Semesters.Where(s => !s.IsDeleted))
                    .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.Score)
            .ToListAsync(cancellationToken);

        var studentCumulativeGpas = new List<decimal>();
        var studentCumulativeGpa4s = new List<decimal>();

        foreach (var profile in profiles)
        {
            var allCourses = profile.AcademicYears
                .SelectMany(ay => ay.Semesters)
                .SelectMany(s => s.Courses)
                .ToList();

            // Retake deduplication: group by course name or code
            var bestAttempts = allCourses
                .GroupBy(c => c.CourseName.ToLower().Trim())
                .Select(g => g.OrderByDescending(c => c.Score?.CourseScore ?? 0).First())
                .ToList();

            var graded = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
            if (graded.Any())
            {
                var totalCredits = graded.Sum(c => c.Credits);
                if (totalCredits > 0)
                {
                    var sum10 = graded.Sum(c => c.Score!.CourseScore!.Value * c.Credits);
                    var sum4 = graded.Sum(c => c.Score!.Gpa4Value!.Value * c.Credits);

                    studentCumulativeGpas.Add(Math.Round(sum10 / totalCredits, 2, MidpointRounding.AwayFromZero));
                    studentCumulativeGpa4s.Add(Math.Round(sum4 / totalCredits, 2, MidpointRounding.AwayFromZero));
                }
            }
        }

        decimal? sysAvgGpa10 = studentCumulativeGpas.Any() ? studentCumulativeGpas.Average() : (decimal?)null;
        decimal? sysAvgGpa4 = studentCumulativeGpa4s.Any() ? studentCumulativeGpa4s.Average() : (decimal?)null;

        if (sysAvgGpa10.HasValue) sysAvgGpa10 = Math.Round(sysAvgGpa10.Value, 2, MidpointRounding.AwayFromZero);
        if (sysAvgGpa4.HasValue) sysAvgGpa4 = Math.Round(sysAvgGpa4.Value, 2, MidpointRounding.AwayFromZero);

        // GPA distributions
        int excellent = 0, veryGood = 0, good = 0, average = 0, belowAverage = 0, fail = 0;
        foreach (var gpa in studentCumulativeGpas)
        {
            if (gpa >= 9.0m) excellent++;
            else if (gpa >= 8.0m) veryGood++;
            else if (gpa >= 7.0m) good++;
            else if (gpa >= 5.0m) average++;
            else if (gpa >= 4.0m) belowAverage++;
            else fail++;
        }

        // Total credits earned across the system
        var totalCreditsEarned = await _context.Scores
            .Where(s => s.IsPass == true)
            .Join(_context.Courses, s => s.CourseId, c => c.Id, (s, c) => c.Credits)
            .SumAsync(cancellationToken);

        return new AdminStatisticsDto(
            new UserStatsDto(totalStudents, activeStudents, lockedAccounts),
            new AcademicOverviewDto(sysAvgGpa10, sysAvgGpa4, totalCreditsEarned),
            new GpaDistributionDto(excellent, veryGood, good, average, belowAverage, fail)
        );
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<AdminStudentDto> Items, int TotalCount)> GetStudentsAsync(
        int page, int pageSize, string? search, bool? isActive, string? sortBy, string? sortOrder, CancellationToken cancellationToken)
    {
        var query = _context.Users
            .Include(u => u.StudentProfile)
                .ThenInclude(sp => sp!.AcademicYears.Where(ay => !ay.IsDeleted))
                    .ThenInclude(ay => ay.Semesters.Where(s => !s.IsDeleted))
                        .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                            .ThenInclude(c => c.Score)
            .Where(u => u.Role == UserRole.Student);

        // Filters
        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            var cleanSearch = search.ToLower().Trim();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(cleanSearch) ||
                u.LastName.ToLower().Contains(cleanSearch) ||
                u.Email.ToLower().Contains(cleanSearch) ||
                (u.StudentProfile != null && u.StudentProfile.StudentCode.ToLower().Contains(cleanSearch))
            );
        }

        var allUsers = await query.ToListAsync(cancellationToken);

        // Map and compute GPAs in memory
        var studentDtos = allUsers.Select(u =>
        {
            decimal? gpa10 = null;
            decimal? gpa4 = null;

            if (u.StudentProfile != null)
            {
                var allCourses = u.StudentProfile.AcademicYears
                    .SelectMany(ay => ay.Semesters)
                    .SelectMany(s => s.Courses)
                    .ToList();

                var bestAttempts = allCourses
                    .GroupBy(c => c.CourseName.ToLower().Trim())
                    .Select(g => g.OrderByDescending(c => c.Score?.CourseScore ?? 0).First())
                    .ToList();

                var graded = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
                if (graded.Any())
                {
                    var totalCredits = graded.Sum(c => c.Credits);
                    if (totalCredits > 0)
                    {
                        var sum10 = graded.Sum(c => c.Score!.CourseScore!.Value * c.Credits);
                        var sum4 = graded.Sum(c => c.Score!.Gpa4Value!.Value * c.Credits);
                        gpa10 = Math.Round(sum10 / totalCredits, 2, MidpointRounding.AwayFromZero);
                        gpa4 = Math.Round(sum4 / totalCredits, 2, MidpointRounding.AwayFromZero);
                    }
                }
            }

            return new AdminStudentDto(
                u.Id,
                u.StudentProfile?.StudentCode ?? string.Empty,
                u.FirstName,
                u.LastName,
                u.Email,
                u.StudentProfile?.UniversityName ?? string.Empty,
                u.StudentProfile?.MajorName ?? string.Empty,
                u.IsActive,
                gpa10,
                gpa4,
                u.CreatedAt,
                u.LastLoginAt
            );
        }).ToList();

        // Sorting
        bool desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        if (!string.IsNullOrEmpty(sortBy))
        {
            studentDtos = sortBy.ToLower() switch
            {
                "studentcode" => desc ? studentDtos.OrderByDescending(x => x.StudentCode).ToList() : studentDtos.OrderBy(x => x.StudentCode).ToList(),
                "lastname" => desc ? studentDtos.OrderByDescending(x => x.LastName).ToList() : studentDtos.OrderBy(x => x.LastName).ToList(),
                "email" => desc ? studentDtos.OrderByDescending(x => x.Email).ToList() : studentDtos.OrderBy(x => x.Email).ToList(),
                "cumulativegpa10" => desc ? studentDtos.OrderByDescending(x => x.CumulativeGpa10 ?? 0).ToList() : studentDtos.OrderBy(x => x.CumulativeGpa10 ?? 0).ToList(),
                _ => desc ? studentDtos.OrderByDescending(x => x.RegistrationDate).ToList() : studentDtos.OrderBy(x => x.RegistrationDate).ToList()
            };
        }
        else
        {
            studentDtos = studentDtos.OrderByDescending(x => x.RegistrationDate).ToList();
        }

        var totalCount = studentDtos.Count;
        var paginated = studentDtos.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return (paginated, totalCount);
    }

    /// <inheritdoc />
    public async Task<AdminStudentDetailDto> GetStudentDetailsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var u = await _context.Users
            .Include(u => u.StudentProfile)
                .ThenInclude(sp => sp!.AcademicYears.Where(ay => !ay.IsDeleted))
                    .ThenInclude(ay => ay.Semesters.Where(s => !s.IsDeleted))
                        .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                            .ThenInclude(c => c.Score)
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student, cancellationToken);

        if (u == null)
        {
            throw new NotFoundException("Student", studentId);
        }

        decimal? gpa10 = null;
        decimal? gpa4 = null;

        if (u.StudentProfile != null)
        {
            var allCourses = u.StudentProfile.AcademicYears
                .SelectMany(ay => ay.Semesters)
                .SelectMany(s => s.Courses)
                .ToList();

            var bestAttempts = allCourses
                .GroupBy(c => c.CourseName.ToLower().Trim())
                .Select(g => g.OrderByDescending(c => c.Score?.CourseScore ?? 0).First())
                .ToList();

            var graded = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
            if (graded.Any())
            {
                var totalCredits = graded.Sum(c => c.Credits);
                if (totalCredits > 0)
                {
                    var sum10 = graded.Sum(c => c.Score!.CourseScore!.Value * c.Credits);
                    var sum4 = graded.Sum(c => c.Score!.Gpa4Value!.Value * c.Credits);
                    gpa10 = Math.Round(sum10 / totalCredits, 2, MidpointRounding.AwayFromZero);
                    gpa4 = Math.Round(sum4 / totalCredits, 2, MidpointRounding.AwayFromZero);
                }
            }
        }

        return new AdminStudentDetailDto(
            u.Id,
            u.Email,
            u.FirstName,
            u.LastName,
            u.IsActive,
            u.StudentProfile?.StudentCode,
            u.StudentProfile?.UniversityName,
            u.StudentProfile?.MajorName,
            u.StudentProfile?.EnrollmentYear,
            u.StudentProfile?.TotalRequiredCredits,
            gpa10,
            gpa4
        );
    }

    /// <inheritdoc />
    public async Task LockStudentAsync(Guid studentId, string reason, string adminIpAddress, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Student", studentId);
        }

        user.IsActive = false;
        user.LockedAt = DateTime.UtcNow;
        user.LockReason = reason;

        // Revoke active sessions
        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await LogActivityAsync(studentId, $"Locked Account: {reason}", adminIpAddress, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UnlockStudentAsync(Guid studentId, string adminIpAddress, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Student", studentId);
        }

        user.IsActive = true;
        user.LockedAt = null;
        user.LockReason = null;

        await _context.SaveChangesAsync(cancellationToken);
        await LogActivityAsync(studentId, "Unlocked Account", adminIpAddress, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> ResetStudentPasswordAsync(Guid studentId, string adminIpAddress, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Student", studentId);
        }

        var tempPassword = GenerateTemporaryPassword();
        user.PasswordHash = _passwordHasher.HashPassword(tempPassword);
        user.ForcePasswordChange = true;

        // Revoke active sessions
        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await LogActivityAsync(studentId, "Reset Password to Temporary Key", adminIpAddress, cancellationToken);

        return tempPassword;
    }

    /// <inheritdoc />
    public async Task DeleteStudentAsync(Guid studentId, string adminIpAddress, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("Student", studentId);
        }

        user.IsDeleted = true;
        user.IsActive = false;

        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await LogActivityAsync(studentId, "Soft Deleted Account", adminIpAddress, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SendDirectNotificationAsync(Guid adminUserId, Guid recipientId, string title, string message, string type, CancellationToken cancellationToken)
    {
        var recipient = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == recipientId && u.Role == UserRole.Student, cancellationToken);

        if (recipient == null)
        {
            throw new NotFoundException("Student", recipientId);
        }

        var notif = new Notification
        {
            UserId = recipientId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            IsBroadcast = false,
            SenderId = adminUserId,
            RecipientName = $"{recipient.LastName} {recipient.FirstName}"
        };

        _context.Notifications.Add(notif);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task BroadcastNotificationAsync(Guid adminUserId, string title, string message, CancellationToken cancellationToken)
    {
        // 1. Create broadcast history tracker under AdminUserId
        var historyNotif = new Notification
        {
            UserId = adminUserId, // Link to admin just to store history
            Title = title,
            Message = message,
            Type = "System",
            IsRead = true,
            IsBroadcast = true,
            SenderId = adminUserId,
            RecipientName = "All Students"
        };
        _context.Notifications.Add(historyNotif);

        // 2. Fetch all active student IDs
        var studentIds = await _context.Users
            .Where(u => u.Role == UserRole.Student && u.IsActive && !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        // 3. Create individual notifications for each active student
        foreach (var sId in studentIds)
        {
            var notif = new Notification
            {
                UserId = sId,
                Title = title,
                Message = message,
                Type = "System",
                IsRead = false,
                IsBroadcast = false,
                SenderId = adminUserId
            };
            _context.Notifications.Add(notif);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<AdminNotificationHistoryDto> Items, int TotalCount)> GetNotificationHistoryAsync(
        int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Notifications
            .Where(n => n.SenderId != null && (n.IsBroadcast || n.RecipientName != null));

        var list = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new AdminNotificationHistoryDto(
                n.Id,
                n.Title,
                n.Message,
                n.IsBroadcast,
                n.RecipientName,
                n.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        return (list, totalCount);
    }

    /// <inheritdoc />
    public async Task EditStudentInfoAsync(Guid studentId, EditStudentInfoDto dto, string adminIpAddress, CancellationToken cancellationToken)
    {
        var u = await _context.Users
            .Include(u => u.StudentProfile)
            .FirstOrDefaultAsync(u => u.Id == studentId && u.Role == UserRole.Student, cancellationToken);

        if (u == null)
        {
            throw new NotFoundException("Student", studentId);
        }

        u.FirstName = dto.FirstName;
        u.LastName = dto.LastName;
        u.UpdatedAt = DateTime.UtcNow;

        if (u.StudentProfile == null)
        {
            u.StudentProfile = new StudentProfile
            {
                UserId = studentId,
                StudentCode = dto.StudentCode,
                UniversityName = dto.UniversityName,
                MajorName = dto.MajorName,
                EnrollmentYear = dto.EnrollmentYear,
                TotalRequiredCredits = dto.TotalRequiredCredits
            };
        }
        else
        {
            u.StudentProfile.StudentCode = dto.StudentCode;
            u.StudentProfile.UniversityName = dto.UniversityName;
            u.StudentProfile.MajorName = dto.MajorName;
            u.StudentProfile.EnrollmentYear = dto.EnrollmentYear;
            u.StudentProfile.TotalRequiredCredits = dto.TotalRequiredCredits;
        }

        _context.Users.Update(u);
        await _context.SaveChangesAsync(cancellationToken);
        await LogActivityAsync(studentId, "Updated Student Information", adminIpAddress, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<AdminUserDto> Items, int TotalCount)> GetAllUsersAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var cleanSearch = search.ToLower().Trim();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(cleanSearch) ||
                u.LastName.ToLower().Contains(cleanSearch) ||
                u.Email.ToLower().Contains(cleanSearch)
            );
        }

        var list = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role.ToString(),
                u.IsActive,
                u.LastLoginAt
            ))
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        return (list, totalCount);
    }

    /// <inheritdoc />
    public async Task<(IReadOnlyList<UserActivityLogDto> Items, int TotalCount)> GetActivityLogsAsync(
        int page, int pageSize, Guid? userId, CancellationToken cancellationToken)
    {
        var query = _context.UserActivityLogs
            .Include(x => x.User)
            .AsQueryable();

        if (userId.HasValue)
        {
            query = query.Where(x => x.UserId == userId.Value);
        }

        var list = await query
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserActivityLogDto(
                x.Id,
                x.UserId,
                x.User.Email,
                x.Activity,
                x.IpAddress,
                x.Timestamp
            ))
            .ToListAsync(cancellationToken);

        var totalCount = await query.CountAsync(cancellationToken);

        return (list, totalCount);
    }

    /// <inheritdoc />
    public async Task LogActivityAsync(Guid userId, string activity, string ipAddress, CancellationToken cancellationToken)
    {
        var log = new UserActivityLog
        {
            UserId = userId,
            Activity = activity,
            IpAddress = ipAddress ?? "127.0.0.1",
            Timestamp = DateTime.UtcNow
        };
        _context.UserActivityLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private string GenerateTemporaryPassword()
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()_+";

        var random = new Random();
        var chars = new char[12];
        chars[0] = lowercase[random.Next(lowercase.Length)];
        chars[1] = uppercase[random.Next(uppercase.Length)];
        chars[2] = digits[random.Next(digits.Length)];
        chars[3] = special[random.Next(special.Length)];

        const string allChars = lowercase + uppercase + digits + special;
        for (int i = 4; i < 12; i++)
        {
            chars[i] = allChars[random.Next(allChars.Length)];
        }

        return new string(chars.OrderBy(_ => random.Next()).ToArray());
    }
}
