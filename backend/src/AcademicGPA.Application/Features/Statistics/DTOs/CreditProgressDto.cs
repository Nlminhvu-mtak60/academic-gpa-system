namespace AcademicGPA.Application.Features.Statistics.DTOs;

/// <summary>
/// DTO representing credit completion progress.
/// </summary>
public record CreditProgressDto(
    int CompletedCredits,
    int FailedCredits,
    int InProgressCredits,
    int TotalRequiredCredits,
    int RemainingCredits
);
