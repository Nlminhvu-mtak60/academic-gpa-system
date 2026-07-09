namespace AcademicGPA.Application.Features.Prediction.DTOs;

/// <summary>
/// Result of a multi-scenario prediction showing required Final Exam score for a specific grade target.
/// </summary>
public record ScenarioPredictionDto(
    string TargetGrade,
    decimal RequiredScore,
    string Feasibility
);
