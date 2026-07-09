using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Statistics.DTOs;
using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Services;

/// <summary>
/// Service providing student statistics, trends, progress, and performance analytics.
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IApplicationDbContext _context;
    private readonly IGpaCalculator _gpaCalculator;

    public StatisticsService(IApplicationDbContext context, IGpaCalculator gpaCalculator)
    {
        _context = context;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<IReadOnlyList<GpaTrendDto>> GetGpaTrendAsync(Guid userId, CancellationToken cancellationToken)
    {
        // 1. Fetch Student Profile
        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("StudentProfile", userId);
        }

        // 2. Fetch all academic years and semesters chronologically
        var academicYears = await _context.AcademicYears
            .Include(ay => ay.Semesters.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Score)
            .Where(ay => ay.StudentProfileId == profile.Id && !ay.IsDeleted)
            .OrderBy(ay => ay.SortOrder)
            .ToListAsync(cancellationToken);

        // 3. Flatten semesters chronologically (including imported ones)
        var sortedSemesters = academicYears
            .SelectMany(ay => ay.Semesters.OrderBy(s => s.SortOrder))
            .ToList();

        var trendList = new List<GpaTrendDto>();
        var processedSemesters = new List<Semester>();

        // 4. Loop semesters, computing semester GPA and rolling cumulative GPA
        foreach (var semester in sortedSemesters)
        {
            processedSemesters.Add(semester);

            decimal? semGpa10 = null;
            decimal? semGpa4 = null;

            if (semester.IsImported)
            {
                semGpa10 = semester.ImportedGpa10;
                semGpa4 = semester.ImportedGpa4;
            }
            else
            {
                var semCourses = semester.Courses.Where(c => !c.IsDeleted).ToList();
                var gradedSemCourses = semCourses.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
                semGpa10 = _gpaCalculator.CalculateGpa10(gradedSemCourses);
                semGpa4 = _gpaCalculator.CalculateGpa4(gradedSemCourses);
            }

            var detailedCoursesAcc = processedSemesters
                .Where(s => !s.IsImported && !s.AcademicYear.IsImported)
                .SelectMany(s => s.Courses.Where(c => !c.IsDeleted))
                .ToList();

            var bestAttemptsAcc = _gpaCalculator.FilterBestAttempts(detailedCoursesAcc).ToList();
            var gradedBestAttemptsAcc = bestAttemptsAcc.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();

            decimal totalGpa10Weight = 0m;
            decimal totalGpa4Weight = 0m;
            int totalGradedCredits = 0;

            foreach (var c in gradedBestAttemptsAcc)
            {
                totalGpa10Weight += c.Score!.CourseScore!.Value * c.Credits;
                totalGpa4Weight += (c.Score!.Gpa4Value ?? 0.00m) * c.Credits;
                totalGradedCredits += c.Credits;
            }

            var activeYearStart = semester.AcademicYear.StartYear;
            var priorImportedYears = academicYears
                .Where(ay => ay.IsImported && ay.StartYear <= activeYearStart)
                .ToList();

            foreach (var ay in priorImportedYears)
            {
                totalGpa10Weight += ay.ImportedGpa10 * ay.ImportedCredits;
                totalGpa4Weight += ay.ImportedGpa4 * ay.ImportedCredits;
                totalGradedCredits += ay.ImportedCredits;
            }

            var processedImportedSemesters = processedSemesters
                .Where(s => s.IsImported && !s.AcademicYear.IsImported)
                .ToList();

            foreach (var s in processedImportedSemesters)
            {
                totalGpa10Weight += s.ImportedGpa10 * s.ImportedCredits;
                totalGpa4Weight += s.ImportedGpa4 * s.ImportedCredits;
                totalGradedCredits += s.ImportedCredits;
            }

            decimal? rollingCumGpa10 = totalGradedCredits > 0 ? Math.Round(totalGpa10Weight / totalGradedCredits, 2, MidpointRounding.AwayFromZero) : null;
            decimal? rollingCumGpa4 = totalGradedCredits > 0 ? Math.Round(totalGpa4Weight / totalGradedCredits, 2, MidpointRounding.AwayFromZero) : null;

            trendList.Add(new GpaTrendDto(
                semester.Id,
                semester.SemesterName,
                semester.AcademicYear.YearName,
                semGpa10,
                semGpa4,
                rollingCumGpa10,
                rollingCumGpa4
            ));
        }

        return trendList;
    }

    public async Task<GradeDistributionDto> GetGradeDistributionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("StudentProfile", userId);
        }

        var academicYears = await _context.AcademicYears
            .Include(ay => ay.Semesters.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Score)
            .Where(ay => ay.StudentProfileId == profile.Id && !ay.IsDeleted)
            .ToListAsync(cancellationToken);

        var allCourses = academicYears
            .SelectMany(ay => ay.Semesters)
            .SelectMany(s => s.Courses)
            .ToList();

        var bestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();

        int aPlus = 0, a = 0, bPlus = 0, b = 0, cPlus = 0, c = 0, dPlus = 0, d = 0, f = 0;

        foreach (var course in bestAttempts)
        {
            if (course.Score != null && !string.IsNullOrEmpty(course.Score.LetterGrade))
            {
                switch (course.Score.LetterGrade.ToUpperInvariant())
                {
                    case "A+": aPlus++; break;
                    case "A": a++; break;
                    case "B+": bPlus++; break;
                    case "B": b++; break;
                    case "C+": cPlus++; break;
                    case "C": c++; break;
                    case "D+": dPlus++; break;
                    case "D": d++; break;
                    case "F": f++; break;
                }
            }
        }

        return new GradeDistributionDto(aPlus, a, bPlus, b, cPlus, c, dPlus, d, f);
    }

    public async Task<CreditProgressDto> GetCreditProgressAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("StudentProfile", userId);
        }

        var academicYears = await _context.AcademicYears
            .Include(ay => ay.Semesters.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Score)
            .Where(ay => ay.StudentProfileId == profile.Id && !ay.IsDeleted)
            .ToListAsync(cancellationToken);

        var allCourses = academicYears
            .Where(ay => !ay.IsImported)
            .SelectMany(ay => ay.Semesters)
            .Where(s => !s.IsImported)
            .SelectMany(s => s.Courses)
            .ToList();

        var bestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();

        int completedCredits = bestAttempts
            .Where(c => c.Score != null && c.Score.IsPass == true)
            .Sum(c => c.Credits);

        int failedCredits = bestAttempts
            .Where(c => c.Score != null && c.Score.IsPass == false)
            .Sum(c => c.Credits);

        int inProgressCredits = allCourses
            .Where(c => c.Score == null || !c.Score.CourseScore.HasValue)
            .Sum(c => c.Credits);

        // Add imported summary credits
        foreach (var ay in academicYears.Where(ay => ay.IsImported))
        {
            completedCredits += ay.ImportedCredits;
        }

        foreach (var s in academicYears.Where(ay => !ay.IsImported).SelectMany(ay => ay.Semesters).Where(s => s.IsImported))
        {
            completedCredits += s.ImportedCredits;
        }

        int totalRequiredCredits = profile.TotalRequiredCredits;
        int remainingCredits = Math.Max(0, totalRequiredCredits - completedCredits);

        return new CreditProgressDto(
            completedCredits,
            failedCredits,
            inProgressCredits,
            totalRequiredCredits,
            remainingCredits
        );
    }

    public async Task<StrengthsWeaknessesDto> GetStrengthsWeaknessesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("StudentProfile", userId);
        }

        var academicYears = await _context.AcademicYears
            .Include(ay => ay.Semesters.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Score)
            .Where(ay => ay.StudentProfileId == profile.Id && !ay.IsDeleted)
            .ToListAsync(cancellationToken);

        var allCourses = academicYears
            .SelectMany(ay => ay.Semesters)
            .SelectMany(s => s.Courses)
            .ToList();

        var bestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();
        var gradedAttempts = bestAttempts
            .Where(c => c.Score != null && c.Score.CourseScore.HasValue)
            .ToList();

        var strongest = gradedAttempts
            .OrderByDescending(c => c.Score!.CourseScore!.Value)
            .ThenByDescending(c => c.Credits)
            .ThenBy(c => c.CourseCode)
            .Take(5)
            .Select(c => new StrengthWeaknessCourseDto(
                c.CourseCode,
                c.CourseName,
                c.Score!.CourseScore!.Value,
                c.Score.LetterGrade ?? string.Empty
            ))
            .ToList();

        var weakest = gradedAttempts
            .OrderBy(c => c.Score!.CourseScore!.Value)
            .ThenBy(c => c.Credits)
            .ThenBy(c => c.CourseCode)
            .Take(5)
            .Select(c => new StrengthWeaknessCourseDto(
                c.CourseCode,
                c.CourseName,
                c.Score!.CourseScore!.Value,
                c.Score.LetterGrade ?? string.Empty
            ))
            .ToList();

        return new StrengthsWeaknessesDto(strongest, weakest);
    }
}
