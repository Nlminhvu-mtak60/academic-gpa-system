using System;
using System.Collections.Generic;

namespace AcademicGPA.Application.Features.Dashboard.DTOs;

/// <summary>
/// DTO representing the student details on the dashboard.
/// </summary>
public record DashboardStudentDto(
    string FirstName,
    string LastName,
    string StudentCode
);

/// <summary>
/// DTO representing academic performance statistics on the dashboard.
/// </summary>
public record DashboardPerformanceDto(
    decimal? CurrentSemesterGpa10,
    decimal? CurrentSemesterGpa4,
    decimal? CurrentAcademicYearGpa10,
    decimal? CurrentAcademicYearGpa4,
    decimal? CumulativeGpa10,
    decimal? CumulativeGpa4,
    int TotalCredits,
    int PassedCredits,
    int FailedCredits,
    string? CurrentAcademicYearName,
    string? CurrentSemesterName,
    string ClassificationVn,
    int TotalCreditsCompleted,
    int TotalCreditsRequired
);

/// <summary>
/// DTO representing goal progress on the dashboard (out of scope, kept for compatibility).
/// </summary>
public record DashboardGoalProgressDto(
    decimal? TargetGpa10,
    decimal? TargetGpa4,
    bool IsAchieved,
    decimal? RequiredRemainingGpa
);

/// <summary>
/// DTO representing recently graded/updated courses.
/// </summary>
public record DashboardRecentCourseDto(
    Guid Id,
    string CourseCode,
    string CourseName,
    int Credits,
    decimal? CourseScore,
    string? LetterGrade
);

/// <summary>
/// Consolidated DTO representing all data needed to render the student dashboard.
/// </summary>
public record DashboardSummaryDto(
    DashboardStudentDto Student,
    DashboardPerformanceDto PerformanceSummary,
    DashboardGoalProgressDto GoalProgress,
    IReadOnlyList<DashboardRecentCourseDto> RecentCourses,
    int UnreadNotificationsCount
);
