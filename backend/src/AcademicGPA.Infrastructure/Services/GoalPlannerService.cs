using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Goals.DTOs;
using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Services;

/// <summary>
/// Implements Goal Planner features: goal CRUD, required GPA analysis, and what-if simulation.
/// </summary>
public class GoalPlannerService : IGoalPlannerService
{
    private readonly IApplicationDbContext _context;
    private readonly IGpaCalculator _gpaCalculator;
    private readonly INotificationService _notificationService;

    public GoalPlannerService(
        IApplicationDbContext context,
        IGpaCalculator gpaCalculator,
        INotificationService notificationService)
    {
        _context = context;
        _gpaCalculator = gpaCalculator;
        _notificationService = notificationService;
    }

    /// <inheritdoc />
    public async Task<List<GoalDto>> GetGoalsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await GetStudentProfileAsync(userId, cancellationToken);

        var goals = await _context.AcademicGoals
            .Where(g => g.StudentProfileId == profile.Id)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new GoalDto(
                g.Id,
                g.TargetCumulativeGpa10,
                g.TargetCumulativeGpa4,
                g.Notes,
                g.IsAchieved,
                g.IsActive,
                g.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return goals;
    }

    /// <inheritdoc />
    public async Task<GoalDto> SetGoalAsync(Guid userId, decimal targetGpa10, string? notes, CancellationToken cancellationToken)
    {
        var profile = await GetStudentProfileAsync(userId, cancellationToken);

        // BR-GOAL-002: Deactivate all previous active goals
        var activeGoals = await _context.AcademicGoals
            .Where(g => g.StudentProfileId == profile.Id && g.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var existingGoal in activeGoals)
        {
            existingGoal.IsActive = false;
        }

        // Auto-compute GPA4 equivalent from GPA10
        var targetGpa4 = MapGpa10ToGpa4(targetGpa10);

        // Create new goal
        var newGoal = new AcademicGoal
        {
            StudentProfileId = profile.Id,
            TargetCumulativeGpa10 = targetGpa10,
            TargetCumulativeGpa4 = targetGpa4,
            Notes = notes,
            IsActive = true,
            IsAchieved = false,
            CreatedAt = DateTime.UtcNow
        };

        // BR-GOAL-003: Check if already achieved
        var currentCumulativeGpa10 = await CalculateCurrentCumulativeGpa10Async(profile, cancellationToken);
        if (currentCumulativeGpa10.HasValue && currentCumulativeGpa10.Value >= targetGpa10)
        {
            newGoal.IsAchieved = true;
        }

        _context.AcademicGoals.Add(newGoal);
        await _context.SaveChangesAsync(cancellationToken);

        // Send notifications
        await _notificationService.SendNotificationAsync(
            profile.UserId,
            "Target GPA Goal Activated",
            $"New target cumulative GPA goal of {targetGpa10:F2} has been set.",
            "Goal",
            cancellationToken);

        if (newGoal.IsAchieved)
        {
            await _notificationService.SendNotificationAsync(
                profile.UserId,
                "Goal Already Achieved",
                $"Your current cumulative GPA ({currentCumulativeGpa10:F2}) already meets or exceeds your target of {targetGpa10:F2}.",
                "Goal",
                cancellationToken);
        }

        return new GoalDto(
            newGoal.Id,
            newGoal.TargetCumulativeGpa10,
            newGoal.TargetCumulativeGpa4,
            newGoal.Notes,
            newGoal.IsAchieved,
            newGoal.IsActive,
            newGoal.CreatedAt
        );
    }

    /// <inheritdoc />
    public async Task<RequiredGpaDto> GetRequiredGpaAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await GetStudentProfileAsync(userId, cancellationToken);

        // Find active goal
        var activeGoal = await _context.AcademicGoals
            .FirstOrDefaultAsync(g => g.StudentProfileId == profile.Id && g.IsActive, cancellationToken);

        if (activeGoal == null)
        {
            throw new UnprocessableEntityException("No active goal defined. Please set a target GPA goal first.");
        }

        // Calculate current cumulative GPA and completed credits
        var (currentGpa10, completedCredits) = await CalculateCurrentGpaAndCreditsAsync(profile, cancellationToken);
        int totalCreditsRequired = profile.TotalRequiredCredits;
        int creditsRemaining = Math.Max(0, totalCreditsRequired - completedCredits);

        decimal targetGpa10 = activeGoal.TargetCumulativeGpa10;
        decimal currentGpa = currentGpa10 ?? 0m;

        // BR-GOAL-001: Feasibility calculation
        // requiredRemainingGpa10 = (targetGpa10 * totalCredits - currentGpa10 * completedCredits) / remainingCredits
        string feasibility;
        decimal requiredRemainingGpa10;
        string message;

        if (creditsRemaining <= 0)
        {
            // No remaining credits
            requiredRemainingGpa10 = 0m;
            if (currentGpa >= targetGpa10)
            {
                feasibility = "Already Achieved";
                message = "Congratulations! Your current cumulative GPA already meets or exceeds your goal.";
            }
            else
            {
                feasibility = "Not Achievable";
                message = "You have completed all required credits. Your goal cannot be achieved with no remaining courses.";
            }
        }
        else
        {
            requiredRemainingGpa10 = Math.Round(
                (targetGpa10 * totalCreditsRequired - currentGpa * completedCredits) / creditsRemaining,
                2, MidpointRounding.AwayFromZero);

            if (currentGpa >= targetGpa10)
            {
                feasibility = "Already Achieved";
                message = "Congratulations! Your current cumulative GPA already meets or exceeds your goal.";
            }
            else if (requiredRemainingGpa10 > 10.0m)
            {
                feasibility = "Not Achievable";
                message = $"Goal is not achievable. You would need an average GPA of {requiredRemainingGpa10:F2} in remaining credits, which exceeds the maximum 10.00.";
            }
            else if (requiredRemainingGpa10 <= 0m)
            {
                feasibility = "Already Achieved";
                message = "Your goal is already achieved based on current performance.";
            }
            else
            {
                feasibility = "Achievable";
                message = $"You need an average GPA of {requiredRemainingGpa10:F2} in your remaining {creditsRemaining} credits to achieve your goal.";
            }
        }

        return new RequiredGpaDto(
            targetGpa10,
            currentGpa,
            completedCredits,
            creditsRemaining,
            requiredRemainingGpa10,
            feasibility,
            message
        );
    }

    /// <inheritdoc />
    public async Task<SimulationResultDto> SimulateScenariosAsync(Guid userId, List<SimulatedCourseInput> simulatedCourses, CancellationToken cancellationToken)
    {
        var profile = await GetStudentProfileAsync(userId, cancellationToken);

        // Load all academic data
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

        // Build a lookup of simulated overrides
        var overrides = simulatedCourses.ToDictionary(sc => sc.CourseId);

        // Clone courses with simulated scores (without persisting)
        var simulatedCourseList = new List<Course>();
        foreach (var course in allCourses)
        {
            if (overrides.TryGetValue(course.Id, out var simInput))
            {
                // Create a virtual copy with simulated scores
                var simulatedScore = _gpaCalculator.CalculateCourseScore(
                    simInput.AttendanceScore, simInput.ContinuousScore, simInput.FinalExamScore);

                var virtualCourse = new Course
                {
                    Id = course.Id,
                    SemesterId = course.SemesterId,
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    Credits = course.Credits,
                    IsRetake = course.IsRetake,
                    OriginalCourseId = course.OriginalCourseId,
                    CreatedAt = course.CreatedAt,
                    Score = new Score
                    {
                        CourseId = course.Id,
                        CourseScore = simulatedScore,
                        Gpa4Value = simulatedScore.HasValue ? _gpaCalculator.MapToGradeResult(simulatedScore.Value).Gpa4Value : null,
                        IsPass = simulatedScore.HasValue ? simulatedScore.Value >= 4.0m : null
                    }
                };
                simulatedCourseList.Add(virtualCourse);
            }
            else
            {
                // Use existing data as-is
                simulatedCourseList.Add(course);
            }
        }

        // Calculate simulated GPAs using best-attempt logic
        var bestAttempts = _gpaCalculator.FilterBestAttempts(simulatedCourseList).ToList();
        var gradedBest = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
        var simulatedCumulativeGpa10 = _gpaCalculator.CalculateGpa10(gradedBest);

        // Calculate simulated semester GPA (courses in the current semester)
        var currentYear = academicYears.FirstOrDefault(ay => ay.IsCurrent);
        var currentSemester = currentYear?.Semesters.OrderByDescending(s => s.SortOrder).FirstOrDefault();
        decimal? simulatedSemesterGpa10 = null;

        if (currentSemester != null)
        {
            var semesterCourses = simulatedCourseList
                .Where(c => c.SemesterId == currentSemester.Id && c.Score != null && c.Score.CourseScore.HasValue)
                .ToList();
            simulatedSemesterGpa10 = _gpaCalculator.CalculateGpa10(semesterCourses);
        }

        // Calculate target variance (if there's an active goal)
        decimal? targetVariance = null;
        var activeGoal = await _context.AcademicGoals
            .FirstOrDefaultAsync(g => g.StudentProfileId == profile.Id && g.IsActive, cancellationToken);

        if (activeGoal != null && simulatedCumulativeGpa10.HasValue)
        {
            targetVariance = Math.Round(
                simulatedCumulativeGpa10.Value - activeGoal.TargetCumulativeGpa10,
                2, MidpointRounding.AwayFromZero);
        }

        return new SimulationResultDto(
            simulatedSemesterGpa10,
            simulatedCumulativeGpa10,
            targetVariance
        );
    }

    // ──────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────

    private async Task<StudentProfile> GetStudentProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("StudentProfile", userId);
        }

        return profile;
    }

    private async Task<decimal?> CalculateCurrentCumulativeGpa10Async(StudentProfile profile, CancellationToken cancellationToken)
    {
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
        var gradedBest = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
        return _gpaCalculator.CalculateGpa10(gradedBest);
    }

    private async Task<(decimal? Gpa10, int CompletedCredits)> CalculateCurrentGpaAndCreditsAsync(
        StudentProfile profile, CancellationToken cancellationToken)
    {
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

        decimal totalGpa10Weight = 0m;
        int completedCredits = 0;

        var bestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();
        var gradedBestAttempts = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
        
        foreach (var c in gradedBestAttempts)
        {
            totalGpa10Weight += c.Score!.CourseScore!.Value * c.Credits;
        }
        completedCredits += bestAttempts
            .Where(c => c.Score != null && c.Score.IsPass == true)
            .Sum(c => c.Credits);
        
        int totalGradedCredits = gradedBestAttempts.Sum(c => c.Credits);

        foreach (var ay in academicYears.Where(ay => ay.IsImported))
        {
            totalGpa10Weight += ay.ImportedGpa10 * ay.ImportedCredits;
            completedCredits += ay.ImportedCredits;
            totalGradedCredits += ay.ImportedCredits;
        }

        foreach (var s in academicYears.Where(ay => !ay.IsImported).SelectMany(ay => ay.Semesters).Where(s => s.IsImported))
        {
            totalGpa10Weight += s.ImportedGpa10 * s.ImportedCredits;
            completedCredits += s.ImportedCredits;
            totalGradedCredits += s.ImportedCredits;
        }

        decimal? gpa10 = totalGradedCredits > 0 ? Math.Round(totalGpa10Weight / totalGradedCredits, 2, MidpointRounding.AwayFromZero) : null;

        return (gpa10, completedCredits);
    }

    /// <summary>
    /// Maps a GPA 10-scale value to an approximate GPA 4-scale value.
    /// </summary>
    private static decimal MapGpa10ToGpa4(decimal gpa10)
    {
        if (gpa10 >= 9.0m) return 4.0m;
        if (gpa10 >= 8.5m) return 3.7m;
        if (gpa10 >= 8.0m) return 3.5m;
        if (gpa10 >= 7.0m) return 3.0m;
        if (gpa10 >= 6.5m) return 2.5m;
        if (gpa10 >= 5.5m) return 2.0m;
        if (gpa10 >= 5.0m) return 1.5m;
        if (gpa10 >= 4.0m) return 1.0m;
        return 0.0m;
    }
}
