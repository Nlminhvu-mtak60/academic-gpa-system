using System;
using AcademicGPA.Application.Features.Scores.Commands.UpdateScores;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class ScoreTests
{
    private readonly GpaCalculator _calculator = new();

    [Theory]
    // Rounding of components to nearest 0.5 first, then weighted calculation and rounding to 1 decimal place.
    [InlineData(7.0, 7.0, 7.0, 7.0)]    // 7.0*0.1 + 7.0*0.3 + 7.0*0.6 = 7.0
    [InlineData(7.1, 7.0, 7.0, 7.0)]    // 7.1 rounds to 7.0 -> 7.0
    [InlineData(7.2, 7.0, 7.0, 7.0)]    // 7.2 rounds to 7.0 -> 7.0
    [InlineData(7.24, 7.0, 7.0, 7.0)]   // 7.24 rounds to 7.0 -> 7.0
    [InlineData(7.25, 7.0, 7.0, 7.1)]   // 7.25 rounds to 7.5 -> 7.5*0.1 + 7.0*0.3 + 7.0*0.6 = 7.05 -> rounds to 7.1
    [InlineData(7.3, 7.0, 7.0, 7.1)]    // 7.3 rounds to 7.5 -> 7.05 -> rounds to 7.1
    [InlineData(7.7, 7.0, 7.0, 7.1)]    // 7.7 rounds to 7.5 -> 7.05 -> rounds to 7.1
    [InlineData(7.75, 7.0, 7.0, 7.1)]   // 7.75 rounds to 8.0 -> 8.0*0.1 + 7.0*0.3 + 7.0*0.6 = 7.1 -> 7.1
    public void CalculateCourseScore_ShouldRoundComponentsAndWeightsCorrectly(
        double attendance,
        double continuous,
        double finalExam,
        double expectedScore)
    {
        // Act
        var result = _calculator.CalculateCourseScore(
            (decimal)attendance,
            (decimal)continuous,
            (decimal)finalExam
        );

        // Assert
        result.Should().Be((decimal)expectedScore);
    }

    [Theory]
    // Standard rounding to 1 decimal place check using component scores that are already multiples of 0.5.
    // This allows isolated testing of the final course score rounding.
    [InlineData(7.5, 7.0, 7.0, 7.1)]  // raw: 7.5*0.1 + 7.0*0.3 + 7.0*0.6 = 0.75 + 2.1 + 4.2 = 7.05 -> rounds to 7.1
    [InlineData(6.5, 7.0, 7.0, 7.0)]  // raw: 6.5*0.1 + 7.0*0.3 + 7.0*0.6 = 0.65 + 2.1 + 4.2 = 6.95 -> rounds to 7.0
    [InlineData(8.5, 7.0, 7.0, 7.2)]  // raw: 8.5*0.1 + 7.0*0.3 + 7.0*0.6 = 0.85 + 2.1 + 4.2 = 7.15 -> rounds to 7.2
    [InlineData(5.5, 7.0, 7.0, 6.9)]  // raw: 5.5*0.1 + 7.0*0.3 + 7.0*0.6 = 0.55 + 2.1 + 4.2 = 6.85 -> rounds to 6.9
    public void CalculateCourseScore_ShouldRoundFinalResultToOneDecimalPlace(
        double attendance,
        double continuous,
        double finalExam,
        double expectedScore)
    {
        // Act
        var result = _calculator.CalculateCourseScore(
            (decimal)attendance,
            (decimal)continuous,
            (decimal)finalExam
        );

        // Assert
        result.Should().Be((decimal)expectedScore);
    }

    [Theory]
    [InlineData(9.0, "A+", 4.0, "Outstanding", true)]
    [InlineData(9.5, "A+", 4.0, "Outstanding", true)]
    [InlineData(10.0, "A+", 4.0, "Outstanding", true)]
    [InlineData(8.5, "A", 3.7, "Excellent", true)]
    [InlineData(8.9, "A", 3.7, "Excellent", true)]
    [InlineData(8.0, "B+", 3.5, "Very Good", true)]
    [InlineData(8.4, "B+", 3.5, "Very Good", true)]
    [InlineData(7.0, "B", 3.0, "Good", true)]
    [InlineData(7.9, "B", 3.0, "Good", true)]
    [InlineData(6.5, "C+", 2.5, "Average Good", true)]
    [InlineData(6.9, "C+", 2.5, "Average Good", true)]
    [InlineData(5.5, "C", 2.0, "Average", true)]
    [InlineData(6.4, "C", 2.0, "Average", true)]
    [InlineData(5.0, "D+", 1.5, "Average", true)]
    [InlineData(5.4, "D+", 1.5, "Average", true)]
    [InlineData(4.0, "D", 1.0, "Weak", true)]
    [InlineData(4.9, "D", 1.0, "Weak", true)]
    [InlineData(0.0, "F", 0.0, "Poor", false)]
    [InlineData(3.9, "F", 0.0, "Poor", false)]
    public void MapToGradeResult_ShouldConvertCorrectly(
        double score,
        string expectedLetter,
        double expectedGpa4,
        string expectedClassification,
        bool expectedPass)
    {
        // Act
        var result = _calculator.MapToGradeResult((decimal)score);

        // Assert
        result.LetterGrade.Should().Be(expectedLetter);
        result.Gpa4Value.Should().Be((decimal)expectedGpa4);
        result.AcademicClassification.Should().Be(expectedClassification);
        result.IsPass.Should().Be(expectedPass);
    }

    [Theory]
    [InlineData(8.0, 7.5, 6.5, true)]  // Valid scores
    [InlineData(null, null, null, true)] // Optional scores (allowed)
    [InlineData(10.0, null, null, true)] // Partial score (allowed)
    [InlineData(-0.5, 5.0, 5.0, false)] // Negative value
    [InlineData(10.5, 5.0, 5.0, false)] // Exceeds 10.0
    public void UpdateScoresCommandValidator_ShouldValidateCorrectly(
        double? attendance,
        double? continuous,
        double? finalExam,
        bool expectedValid)
    {
        // Arrange
        var validator = new UpdateScoresCommandValidator();
        var command = new UpdateScoresCommand(
            Guid.NewGuid(),
            attendance.HasValue ? (decimal)attendance.Value : null,
            continuous.HasValue ? (decimal)continuous.Value : null,
            finalExam.HasValue ? (decimal)finalExam.Value : null
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}
