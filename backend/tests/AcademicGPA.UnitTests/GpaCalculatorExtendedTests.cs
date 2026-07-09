using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class GpaCalculatorExtendedTests
{
    private readonly GpaCalculator _calculator = new();

    [Theory]
    // 9.0 -> A+
    [InlineData(9.0, "A+", 4.0, true)]
    [InlineData(9.5, "A+", 4.0, true)]
    [InlineData(10.0, "A+", 4.0, true)]
    // 8.5 -> A
    [InlineData(8.5, "A", 3.7, true)]
    [InlineData(8.9, "A", 3.7, true)]
    // 8.0 -> B+
    [InlineData(8.0, "B+", 3.5, true)]
    [InlineData(8.4, "B+", 3.5, true)]
    // 7.0 -> B
    [InlineData(7.0, "B", 3.0, true)]
    [InlineData(7.9, "B", 3.0, true)]
    // 6.5 -> C+
    [InlineData(6.5, "C+", 2.5, true)]
    [InlineData(6.9, "C+", 2.5, true)]
    // 5.5 -> C
    [InlineData(5.5, "C", 2.0, true)]
    [InlineData(6.4, "C", 2.0, true)]
    // 5.0 -> D+
    [InlineData(5.0, "D+", 1.5, true)]
    [InlineData(5.4, "D+", 1.5, true)]
    // 4.0 -> D
    [InlineData(4.0, "D", 1.0, true)]
    [InlineData(4.9, "D", 1.0, true)]
    // < 4.0 -> F
    [InlineData(3.9, "F", 0.0, false)]
    [InlineData(0.0, "F", 0.0, false)]
    public void MapToGradeResult_ShouldMapCorrectlyBasedOnCourseScore(decimal score, string expectedLetter, decimal expectedGpa4, bool expectedPass)
    {
        // Act
        var result = _calculator.MapToGradeResult(score);

        // Assert
        result.LetterGrade.Should().Be(expectedLetter);
        result.Gpa4Value.Should().Be(expectedGpa4);
        result.IsPass.Should().Be(expectedPass);
    }

    [Theory]
    // Attendance * 0.1 + Continuous * 0.3 + Final * 0.6
    // Inputs are rounded to nearest half first.
    [InlineData(8.2, 7.8, 9.1, 8.6)] // 8.2 -> 8.0; 7.8 -> 8.0; 9.1 -> 9.0; Course score = 8.0 * 0.1 + 8.0 * 0.3 + 9.0 * 0.6 = 0.8 + 2.4 + 5.4 = 8.6
    [InlineData(10.0, 10.0, 10.0, 10.0)]
    [InlineData(0.0, 0.0, 0.0, 0.0)]
    [InlineData(7.25, 7.25, 7.25, 7.5)] // 7.25 -> 7.5; 7.5 * 0.1 + 7.5 * 0.3 + 7.5 * 0.6 = 7.5
    public void CalculateCourseScore_ShouldRoundToNearestHalfAndApplyFormula(decimal attendance, decimal continuous, decimal final, decimal expected)
    {
        // Act
        var result = _calculator.CalculateCourseScore(attendance, continuous, final);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CalculateCourseScore_WithNulls_ShouldReturnNull()
    {
        _calculator.CalculateCourseScore(null, 8.0m, 9.0m).Should().BeNull();
        _calculator.CalculateCourseScore(8.0m, null, 9.0m).Should().BeNull();
        _calculator.CalculateCourseScore(8.0m, 8.0m, null).Should().BeNull();
    }

    [Fact]
    public void CalculateGpa10_WithEmptyOrZeroCredits_ShouldReturnNull()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Credits = 0, Score = new Score { CourseScore = 10.0m } }
        };

        // Act
        var result = _calculator.CalculateGpa10(courses);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateGpa4_WithEmptyOrZeroCredits_ShouldReturnNull()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Credits = 0, Score = new Score { Gpa4Value = 4.0m } }
        };

        // Act
        var result = _calculator.CalculateGpa4(courses);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FilterBestAttempts_WithEmptyOrNullCourseCodes_ShouldNotThrowAndFilterCorrectly()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { CourseCode = "", CreatedAt = DateTime.UtcNow, Score = new Score { CourseScore = 5.0m } },
            new() { CourseCode = "", CreatedAt = DateTime.UtcNow.AddMinutes(5), Score = new Score { CourseScore = 8.0m } }
        };

        // Act
        var result = _calculator.FilterBestAttempts(courses).ToList();

        // Assert
        result.Should().ContainSingle();
        result.First().Score!.CourseScore.Should().Be(8.0m);
    }
}
