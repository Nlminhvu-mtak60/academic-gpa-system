using System;

namespace AcademicGPA.Application.Features.Admin.DTOs;

public record AdminStudentDto(
    Guid Id,
    string StudentCode,
    string FirstName,
    string LastName,
    string Email,
    string UniversityName,
    string MajorName,
    bool IsActive,
    decimal? CumulativeGpa10,
    decimal? CumulativeGpa4,
    DateTime RegistrationDate,
    DateTime? LastLoginAt
);

public record AdminStudentDetailDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    string? StudentCode,
    string? UniversityName,
    string? MajorName,
    int? EnrollmentYear,
    int? TotalRequiredCredits,
    decimal? CumulativeGpa10,
    decimal? CumulativeGpa4
);

public record GpaDistributionDto(
    int Excellent,
    int VeryGood,
    int Good,
    int Average,
    int BelowAverage,
    int Fail
);

public record AdminStatisticsDto(
    UserStatsDto UserStats,
    AcademicOverviewDto AcademicOverview,
    GpaDistributionDto GpaDistribution
);

public record UserStatsDto(
    int TotalStudents,
    int ActiveStudents,
    int LockedAccounts
);

public record AcademicOverviewDto(
    decimal? SystemAverageGpa10,
    decimal? SystemAverageGpa4,
    int TotalCreditsEarned
);

public record AdminNotificationHistoryDto(
    Guid Id,
    string Title,
    string Message,
    bool IsBroadcast,
    string? RecipientName,
    DateTime CreatedAt
);

public record AdminUserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTime? LastLoginAt
);

public record UserActivityLogDto(
    Guid Id,
    Guid UserId,
    string UserEmail,
    string Activity,
    string IpAddress,
    DateTime Timestamp
);

public record EditStudentInfoDto(
    string FirstName,
    string LastName,
    string StudentCode,
    string UniversityName,
    string MajorName,
    int EnrollmentYear,
    int TotalRequiredCredits
);
