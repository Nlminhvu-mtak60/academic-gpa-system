using System;
using System.Collections.Generic;
using AcademicGPA.Application.Features.Prediction.DTOs;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class PredictionServiceTests
{
    private readonly PredictionService _predictionService = new();

    [Theory]
    [InlineData(10.0, 10.0, "A+", 9.0, 8.5, "Achievable")] // (9.0 - 10*0.1 - 10*0.3)/0.6 = 5.0/0.6 = 8.33 -> rounds to 8.5
    [InlineData(8.0, 8.0, "B+", 8.0, 8.0, "Achievable")] // (8.0 - 8*0.1 - 8*0.3)/0.6 = 4.8/0.6 = 8.0 -> rounds to 8.0
    [InlineData(9.0, 9.0, "A", 8.5, 8.0, "Achievable")] // (8.5 - 9*0.1 - 9*0.3)/0.6 = 4.9/0.6 = 8.16 -> rounds to 8.0
    public void PredictFinalScore_ShouldCalculateRequiredFinalAndFeasibilityCorrectly(
        double attendance,
        double continuous,
        string targetGrade,
        double expectedThreshold,
        double expectedFinal,
        string expectedFeasibility)
    {
        // Act
        var result = _predictionService.PredictFinalScore(
            (decimal)attendance,
            (decimal)continuous,
            targetGrade
        );

        // Assert
        result.Should().NotBeNull();
        result.AttendanceScore.Should().Be((decimal)attendance);
        result.ContinuousScore.Should().Be((decimal)continuous);
        result.TargetGrade.Should().Be(targetGrade);
        result.TargetScoreThreshold.Should().Be((decimal)expectedThreshold);
        result.RequiredFinalExamScore.Should().Be((decimal)expectedFinal);
        result.Feasibility.Should().Be(expectedFeasibility);
        result.Advice.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void PredictFinalScore_ShouldReturnImpossible_WhenTargetCannotBeReached()
    {
        // Arrange
        // Even with a 10.0 in Final, the max score is 0.0*0.1 + 0.0*0.3 + 10.0*0.6 = 6.0
        // Target is A+ (9.0), which is impossible
        decimal attendance = 0.0m;
        decimal continuous = 0.0m;
        string targetGrade = "A+";

        // Act
        var result = _predictionService.PredictFinalScore(attendance, continuous, targetGrade);

        // Assert
        result.Feasibility.Should().Be("Impossible");
        result.RequiredFinalExamScore.Should().BeGreaterThan(10.0m);
        result.Advice.Should().Contain("not possible");
    }

    [Fact]
    public void PredictFinalScore_ShouldReturnGuaranteed_WhenTargetIsAlreadyMet()
    {
        // Arrange
        // With 10.0 in attendance and continuous, we have 10*0.1 + 10*0.3 = 4.0 points.
        // Target is D (4.0), which is already met even with 0 on Final Exam.
        decimal attendance = 10.0m;
        decimal continuous = 10.0m;
        string targetGrade = "D";

        // Act
        var result = _predictionService.PredictFinalScore(attendance, continuous, targetGrade);

        // Assert
        result.Feasibility.Should().Be("Guaranteed");
        result.RequiredFinalExamScore.Should().Be(0.0m);
        result.Advice.Should().Contain("guaranteed");
    }

    [Fact]
    public void PredictAllScenarios_ShouldReturnEightThresholds()
    {
        // Arrange
        decimal attendance = 8.5m;
        decimal continuous = 8.0m;

        // Act
        var result = _predictionService.PredictAllScenarios(attendance, continuous);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(8);

        // Verify some specific targets
        var aPlus = result.Find(r => r.TargetGrade == "A+");
        aPlus.Should().NotBeNull();
        // A+ target = 9.0. (9.0 - 8.5*0.1 - 8.0*0.3)/0.6 = (9.0 - 0.85 - 2.4)/0.6 = 5.75/0.6 = 9.58 -> rounds to 9.5
        aPlus.RequiredScore.Should().Be(9.5m);
        aPlus.Feasibility.Should().Be("Achievable");

        var d = result.Find(r => r.TargetGrade == "D");
        d.Should().NotBeNull();
        // D target = 4.0. (4.0 - 0.85 - 2.4)/0.6 = 0.75/0.6 = 1.25 -> rounds to 1.5. Wait, rounds 8.5 to nearest 0.5 is 8.5, 8.0 is 8.0.
        // (4.0 - 8.5*0.1 - 8.0*0.3)/0.6 = (4.0 - 3.25)/0.6 = 0.75/0.6 = 1.25 -> rounds to 1.5. Wait, 1.25 * 2 = 2.5. Round(2.5) = 2 -> 2/2 = 1.0. Let's see:
        // Math.Round(1.25 * 2, MidpointRounding.AwayFromZero) / 2 = Math.Round(2.5) / 2 = 3 / 2 = 1.5.
        // Wait, yes, 1.25 * 2 = 2.5. Round(2.5) = 3 (with MidpointRounding.AwayFromZero). So 3 / 2 = 1.5.
        d.RequiredScore.Should().Be(1.5m);
        d.Feasibility.Should().Be("Achievable");
    }
}
