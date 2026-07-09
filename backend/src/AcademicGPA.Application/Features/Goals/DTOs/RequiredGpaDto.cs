namespace AcademicGPA.Application.Features.Goals.DTOs;

/// <summary>
/// Analysis of required GPA in remaining credits to achieve the active goal.
/// </summary>
public record RequiredGpaDto(
    decimal TargetCumulativeGpa10,
    decimal CurrentCumulativeGpa10,
    int CreditsCompleted,
    int CreditsRemaining,
    decimal RequiredRemainingGpa10,
    string Feasibility,
    string Message
);
