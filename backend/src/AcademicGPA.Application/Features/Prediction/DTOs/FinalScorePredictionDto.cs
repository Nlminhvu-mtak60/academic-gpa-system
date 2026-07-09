namespace AcademicGPA.Application.Features.Prediction.DTOs;

/// <summary>
/// Result of predicting the required Final Exam score for a specific target grade.
/// </summary>
public record FinalScorePredictionDto(
    decimal AttendanceScore,
    decimal ContinuousScore,
    string TargetGrade,
    decimal TargetScoreThreshold,
    decimal RequiredFinalExamScore,
    string Feasibility,
    string Advice
);
