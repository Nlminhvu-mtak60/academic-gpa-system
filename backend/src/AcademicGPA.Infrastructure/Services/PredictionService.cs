using System;
using System.Collections.Generic;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Prediction.DTOs;

namespace AcademicGPA.Infrastructure.Services;

/// <summary>
/// Implements Final Exam score prediction using BR-PREDICT-001 and BR-PREDICT-002 business rules.
/// </summary>
public class PredictionService : IPredictionService
{
    /// <summary>
    /// Grade thresholds per BR-PREDICT-002.
    /// </summary>
    private static readonly (string Grade, decimal MinScore)[] GradeThresholds =
    {
        ("A+", 9.0m),
        ("A",  8.5m),
        ("B+", 8.0m),
        ("B",  7.0m),
        ("C+", 6.5m),
        ("C",  5.5m),
        ("D+", 5.0m),
        ("D",  4.0m)
    };

    /// <inheritdoc />
    public FinalScorePredictionDto PredictFinalScore(decimal attendanceScore, decimal continuousScore, string targetGrade)
    {
        // Find the target score threshold for the requested grade
        decimal targetScoreThreshold = 0m;
        foreach (var (grade, minScore) in GradeThresholds)
        {
            if (string.Equals(grade, targetGrade, StringComparison.OrdinalIgnoreCase))
            {
                targetScoreThreshold = minScore;
                break;
            }
        }

        var (requiredFinal, feasibility) = CalculateRequiredFinal(attendanceScore, continuousScore, targetScoreThreshold);

        string advice = feasibility switch
        {
            "Impossible" => $"It is not possible to achieve {targetGrade} with your current component scores.",
            "Guaranteed" => $"You are guaranteed to achieve {targetGrade} regardless of your Final Exam score.",
            _ => $"You need to score at least {requiredFinal:F1} on the Final Exam to secure a {targetGrade}."
        };

        return new FinalScorePredictionDto(
            attendanceScore,
            continuousScore,
            targetGrade,
            targetScoreThreshold,
            requiredFinal,
            feasibility,
            advice
        );
    }

    /// <inheritdoc />
    public List<ScenarioPredictionDto> PredictAllScenarios(decimal attendanceScore, decimal continuousScore)
    {
        var results = new List<ScenarioPredictionDto>();

        foreach (var (grade, minScore) in GradeThresholds)
        {
            var (requiredFinal, feasibility) = CalculateRequiredFinal(attendanceScore, continuousScore, minScore);
            results.Add(new ScenarioPredictionDto(grade, requiredFinal, feasibility));
        }

        return results;
    }

    /// <summary>
    /// Calculates required Final Exam score using BR-PREDICT-001:
    /// RequiredFinal = (TargetScore - RoundedA×0.1 - RoundedC×0.3) / 0.6
    /// Result rounded to nearest 0.5 per BR-CALC-003.
    /// </summary>
    private static (decimal RequiredFinal, string Feasibility) CalculateRequiredFinal(
        decimal attendance, decimal continuous, decimal targetScore)
    {
        var roundedA = RoundToNearestHalf(attendance);
        var roundedC = RoundToNearestHalf(continuous);

        var rawRequired = (targetScore - roundedA * 0.1m - roundedC * 0.3m) / 0.6m;
        var roundedRequired = RoundToNearestHalf(rawRequired);

        if (roundedRequired > 10.0m)
        {
            return (roundedRequired, "Impossible");
        }

        if (roundedRequired <= 0.0m)
        {
            return (0.0m, "Guaranteed");
        }

        return (roundedRequired, "Achievable");
    }

    /// <summary>
    /// Rounds a value to the nearest 0.5 (same as BR-CALC-003).
    /// </summary>
    private static decimal RoundToNearestHalf(decimal value)
    {
        return Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2;
    }
}
