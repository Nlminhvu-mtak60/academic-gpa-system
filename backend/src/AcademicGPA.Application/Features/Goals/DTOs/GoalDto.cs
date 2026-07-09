using System;

namespace AcademicGPA.Application.Features.Goals.DTOs;

/// <summary>
/// Represents a student's GPA goal with achievement status.
/// </summary>
public record GoalDto(
    Guid Id,
    decimal TargetCumulativeGpa10,
    decimal TargetCumulativeGpa4,
    string? Notes,
    bool IsAchieved,
    bool IsActive,
    DateTime CreatedAt
);
