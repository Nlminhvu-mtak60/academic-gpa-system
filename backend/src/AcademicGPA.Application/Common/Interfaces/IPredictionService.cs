using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Prediction.DTOs;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service contract for Final Exam score prediction calculations.
/// </summary>
public interface IPredictionService
{
    /// <summary>
    /// Predicts the required Final Exam score to achieve a specific target grade.
    /// Uses BR-PREDICT-001 formula with nearest 0.5 rounding.
    /// </summary>
    FinalScorePredictionDto PredictFinalScore(decimal attendanceScore, decimal continuousScore, string targetGrade);

    /// <summary>
    /// Generates required Final Exam scores for all possible passing letter grades.
    /// Uses BR-PREDICT-002 target score boundaries.
    /// </summary>
    List<ScenarioPredictionDto> PredictAllScenarios(decimal attendanceScore, decimal continuousScore);
}
