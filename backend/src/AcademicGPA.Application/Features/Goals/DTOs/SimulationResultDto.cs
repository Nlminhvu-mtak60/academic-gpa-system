namespace AcademicGPA.Application.Features.Goals.DTOs;

/// <summary>
/// Result of a "what-if" scenario simulation showing projected GPAs.
/// </summary>
public record SimulationResultDto(
    decimal? SimulatedSemesterGpa10,
    decimal? SimulatedCumulativeGpa10,
    decimal? TargetVariance
);
