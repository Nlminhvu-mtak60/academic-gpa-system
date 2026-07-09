using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Dashboard.DTOs;
using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Services;

/// <summary>
/// Service providing student summary aggregates for the dashboard.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _context;
    private readonly IGpaCalculator _gpaCalculator;

    public DashboardService(IApplicationDbContext context, IGpaCalculator gpaCalculator)
    {
        _context = context;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken)
    {
        // 1. Fetch Student Profile
        var profile = await _context.StudentProfiles
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User does not exist.");
            }

            profile = new StudentProfile
            {
                UserId = userId,
                StudentCode = "ST" + DateTime.UtcNow.ToString("yy") + Guid.NewGuid().ToString("N")[..6].ToUpper(),
                UniversityName = "Chưa cập nhật",
                MajorName = "Chưa cập nhật",
                EnrollmentYear = DateTime.UtcNow.Year,
                TotalRequiredCredits = 130
            };

            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync(cancellationToken);

            profile = await _context.StudentProfiles
                .Include(sp => sp.User)
                .FirstOrDefaultAsync(sp => sp.Id == profile.Id, cancellationToken);
        }

        // 2. Fetch all academic years, semesters, courses, and scores
        var academicYears = await _context.AcademicYears
            .Include(ay => ay.Semesters.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Score)
            .Where(ay => ay.StudentProfileId == profile.Id && !ay.IsDeleted)
            .ToListAsync(cancellationToken);

        // 3. Extract and filter overall courses (excluding imported years and semesters)
        var allCourses = academicYears
            .Where(ay => !ay.IsImported)
            .SelectMany(ay => ay.Semesters)
            .Where(s => !s.IsImported)
            .SelectMany(s => s.Courses)
            .ToList();

        decimal totalGpa10Weight = 0m;
        decimal totalGpa4Weight = 0m;
        int totalCreditsCompleted = 0;

        // Add detailed courses contribution
        var bestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();
        var gradedBestAttempts = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
        
        foreach (var c in gradedBestAttempts)
        {
            totalGpa10Weight += c.Score!.CourseScore!.Value * c.Credits;
            totalGpa4Weight += (c.Score!.Gpa4Value ?? 0.00m) * c.Credits;
        }
        totalCreditsCompleted += bestAttempts
            .Where(c => c.Score != null && c.Score.IsPass == true)
            .Sum(c => c.Credits);
        
        int totalGradedCredits = gradedBestAttempts.Sum(c => c.Credits);

        // Add imported years contribution (where IsImported == true)
        foreach (var ay in academicYears.Where(ay => ay.IsImported))
        {
            totalGpa10Weight += ay.ImportedGpa10 * ay.ImportedCredits;
            totalGpa4Weight += ay.ImportedGpa4 * ay.ImportedCredits;
            totalCreditsCompleted += ay.ImportedCredits;
            totalGradedCredits += ay.ImportedCredits;
        }

        // Add imported semesters contribution (where IsImported == true but their year is NOT imported to prevent double counting)
        foreach (var s in academicYears.Where(ay => !ay.IsImported).SelectMany(ay => ay.Semesters).Where(s => s.IsImported))
        {
            totalGpa10Weight += s.ImportedGpa10 * s.ImportedCredits;
            totalGpa4Weight += s.ImportedGpa4 * s.ImportedCredits;
            totalCreditsCompleted += s.ImportedCredits;
            totalGradedCredits += s.ImportedCredits;
        }

        decimal? cumulativeGpa10 = totalGradedCredits > 0 ? Math.Round(totalGpa10Weight / totalGradedCredits, 2, MidpointRounding.AwayFromZero) : null;
        decimal? cumulativeGpa4 = totalGradedCredits > 0 ? Math.Round(totalGpa4Weight / totalGradedCredits, 2, MidpointRounding.AwayFromZero) : null;

        int totalCreditsRequired = profile.TotalRequiredCredits;

        // Determine Vietnamese Classification for Performance Summary
        string classificationVn = "Kém";
        if (cumulativeGpa10.HasValue)
        {
            var gpa = cumulativeGpa10.Value;
            if (gpa >= 9.0m) classificationVn = "Xuất sắc";
            else if (gpa >= 8.0m) classificationVn = "Giỏi";
            else if (gpa >= 7.0m) classificationVn = "Khá";
            else if (gpa >= 6.5m) classificationVn = "Trung bình khá";
            else if (gpa >= 5.0m) classificationVn = "Trung bình";
            else if (gpa >= 4.0m) classificationVn = "Yếu";
            else classificationVn = "Kém";
        }

        // 5. Determine active year/semester status
        var currentYear = academicYears.FirstOrDefault(ay => ay.IsCurrent);
        var currentSemester = currentYear?.Semesters.OrderByDescending(s => s.SortOrder).FirstOrDefault();

        // Calculate current semester GPAs
        decimal? currentSemesterGpa10 = null;
        decimal? currentSemesterGpa4 = null;
        if (currentSemester != null)
        {
            if (currentSemester.IsImported)
            {
                currentSemesterGpa10 = currentSemester.ImportedGpa10;
                currentSemesterGpa4 = currentSemester.ImportedGpa4;
            }
            else
            {
                var gradedSemesterCourses = currentSemester.Courses
                    .Where(c => !c.IsDeleted && c.Score != null && c.Score.CourseScore.HasValue)
                    .ToList();
                currentSemesterGpa10 = _gpaCalculator.CalculateGpa10(gradedSemesterCourses);
                currentSemesterGpa4 = _gpaCalculator.CalculateGpa4(gradedSemesterCourses);
            }
        }

        // Calculate current academic year GPAs
        decimal? currentAcademicYearGpa10 = null;
        decimal? currentAcademicYearGpa4 = null;
        if (currentYear != null)
        {
            if (currentYear.IsImported)
            {
                currentAcademicYearGpa10 = currentYear.ImportedGpa10;
                currentAcademicYearGpa4 = currentYear.ImportedGpa4;
            }
            else
            {
                decimal yearGpa10Weight = 0m;
                decimal yearGpa4Weight = 0m;
                int yearGradedCredits = 0;

                foreach (var sem in currentYear.Semesters)
                {
                    if (sem.IsImported)
                    {
                        yearGpa10Weight += sem.ImportedGpa10 * sem.ImportedCredits;
                        yearGpa4Weight += sem.ImportedGpa4 * sem.ImportedCredits;
                        yearGradedCredits += sem.ImportedCredits;
                    }
                    else
                    {
                        var gradedSemCourses = sem.Courses
                            .Where(c => !c.IsDeleted && c.Score != null && c.Score.CourseScore.HasValue)
                            .ToList();
                        foreach (var c in gradedSemCourses)
                        {
                            yearGpa10Weight += c.Score!.CourseScore!.Value * c.Credits;
                            yearGpa4Weight += (c.Score!.Gpa4Value ?? 0m) * c.Credits;
                            yearGradedCredits += c.Credits;
                        }
                    }
                }

                if (yearGradedCredits > 0)
                {
                    currentAcademicYearGpa10 = Math.Round(yearGpa10Weight / yearGradedCredits, 2, MidpointRounding.AwayFromZero);
                    currentAcademicYearGpa4 = Math.Round(yearGpa4Weight / yearGradedCredits, 2, MidpointRounding.AwayFromZero);
                }
            }
        }

        // Aggregate total/passed/failed credits across all semesters
        int totalCredits = allCourses.Sum(c => c.Credits);
        int passedCredits = allCourses.Where(c => c.Score != null && c.Score.IsPass == true).Sum(c => c.Credits);
        int failedCredits = allCourses.Where(c => c.Score != null && c.Score.IsPass == false).Sum(c => c.Credits);

        foreach (var ay in academicYears.Where(ay => ay.IsImported))
        {
            totalCredits += ay.ImportedCredits;
            passedCredits += ay.ImportedCredits;
        }

        foreach (var s in academicYears.Where(ay => !ay.IsImported).SelectMany(ay => ay.Semesters).Where(s => s.IsImported))
        {
            totalCredits += s.ImportedCredits;
            passedCredits += s.ImportedCredits;
        }

        var studentDto = new DashboardStudentDto(
            profile.User?.FirstName ?? string.Empty,
            profile.User?.LastName ?? string.Empty,
            profile.StudentCode
        );

        var performanceDto = new DashboardPerformanceDto(
            currentSemesterGpa10,
            currentSemesterGpa4,
            currentAcademicYearGpa10,
            currentAcademicYearGpa4,
            cumulativeGpa10,
            cumulativeGpa4,
            totalCredits,
            passedCredits,
            failedCredits,
            currentYear?.YearName,
            currentSemester?.SemesterName,
            classificationVn,
            totalCreditsCompleted,
            totalCreditsRequired
        );

        // Goal progress: query active goal and compute status
        DashboardGoalProgressDto goalProgressDto;
        var activeGoal = await _context.AcademicGoals
            .FirstOrDefaultAsync(g => g.StudentProfileId == profile.Id && g.IsActive, cancellationToken);

        if (activeGoal != null)
        {
            decimal? requiredRemaining = null;
            int creditsRemaining = Math.Max(0, totalCreditsRequired - totalCreditsCompleted);

            if (creditsRemaining > 0 && cumulativeGpa10.HasValue)
            {
                requiredRemaining = Math.Round(
                    (activeGoal.TargetCumulativeGpa10 * totalCreditsRequired - cumulativeGpa10.Value * totalCreditsCompleted) / creditsRemaining,
                    2, MidpointRounding.AwayFromZero);
            }

            bool isAchieved = cumulativeGpa10.HasValue && cumulativeGpa10.Value >= activeGoal.TargetCumulativeGpa10;
            if (isAchieved && !activeGoal.IsAchieved)
            {
                activeGoal.IsAchieved = true;
                await _context.SaveChangesAsync(cancellationToken);
            }

            goalProgressDto = new DashboardGoalProgressDto(
                activeGoal.TargetCumulativeGpa10,
                activeGoal.TargetCumulativeGpa4,
                isAchieved,
                requiredRemaining
            );
        }
        else
        {
            goalProgressDto = new DashboardGoalProgressDto(null, null, false, null);
        }

        // Fetch top 5 recently updated courses
        var recentCoursesList = allCourses
            .OrderByDescending(c => c.UpdatedAt)
            .Take(5)
            .Select(c => new DashboardRecentCourseDto(
                c.Id,
                c.CourseCode,
                c.CourseName,
                c.Credits,
                c.Score?.CourseScore,
                c.Score?.LetterGrade
            ))
            .ToList();

        return new DashboardSummaryDto(
            studentDto,
            performanceDto,
            goalProgressDto,
            recentCoursesList,
            0 // Unread notifications count placeholder
        );
    }
}
