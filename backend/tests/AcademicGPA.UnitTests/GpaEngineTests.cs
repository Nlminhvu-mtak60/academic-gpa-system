using System;
using System.Collections.Generic;
using System.Linq;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class GpaEngineTests
{
    private readonly GpaCalculator _calculator = new();

    [Fact]
    public void CalculateGpa10_ShouldCalculateCorrectly_WhenAllScoresArePresent()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Credits = 3, Score = new Score { CourseScore = 8.0m } },
            new() { Credits = 2, Score = new Score { CourseScore = 7.5m } }
        };

        // Act
        var result = _calculator.CalculateGpa10(courses);

        // Assert
        // (8.0 * 3 + 7.5 * 2) / 5 = 7.8
        result.Should().Be(7.8m);
    }

    [Fact]
    public void CalculateGpa10_ShouldIgnoreCoursesWithMissingScores()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Credits = 3, Score = new Score { CourseScore = 8.0m } },
            new() { Credits = 2, Score = null }, // Missing Score entity
            new() { Credits = 3, Score = new Score { CourseScore = null } } // Missing score value
        };

        // Act
        var result = _calculator.CalculateGpa10(courses);

        // Assert
        // Only first course counts: (8.0 * 3) / 3 = 8.0
        result.Should().Be(8.0m);
    }

    [Fact]
    public void CalculateGpa10_ShouldReturnNull_WhenNoGradedCoursesExist()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Credits = 3, Score = null },
            new() { Credits = 2, Score = new Score { CourseScore = null } }
        };

        // Act
        var result = _calculator.CalculateGpa10(courses);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateGpa4_ShouldCalculateCorrectly_WhenAllScoresArePresent()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { Credits = 3, Score = new Score { Gpa4Value = 3.5m } },
            new() { Credits = 2, Score = new Score { Gpa4Value = 3.0m } }
        };

        // Act
        var result = _calculator.CalculateGpa4(courses);

        // Assert
        // (3.5 * 3 + 3.0 * 2) / 5 = 3.3
        result.Should().Be(3.3m);
    }

    [Fact]
    public void FilterBestAttempts_ShouldKeepOnlyHighestGradedAttempt_WhenRetakesExist()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { CourseCode = "CS101", CreatedAt = DateTime.UtcNow.AddDays(-10), Score = new Score { CourseScore = 4.0m } }, // Original
            new() { CourseCode = "CS101", CreatedAt = DateTime.UtcNow.AddDays(-1), Score = new Score { CourseScore = 8.0m } },  // Retake (Better)
            new() { CourseCode = "MATH201", CreatedAt = DateTime.UtcNow.AddDays(-2), Score = new Score { CourseScore = 7.0m } } // Single attempt
        };

        // Act
        var result = _calculator.FilterBestAttempts(courses).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.CourseCode == "CS101" && c.Score!.CourseScore == 8.0m);
        result.Should().Contain(c => c.CourseCode == "MATH201" && c.Score!.CourseScore == 7.0m);
    }

    [Fact]
    public void FilterBestAttempts_ShouldBeCaseInsensitive()
    {
        // Arrange
        var courses = new List<Course>
        {
            new() { CourseCode = "cs101", CreatedAt = DateTime.UtcNow.AddDays(-5), Score = new Score { CourseScore = 4.0m } },
            new() { CourseCode = "CS101", CreatedAt = DateTime.UtcNow.AddDays(-1), Score = new Score { CourseScore = 5.5m } }
        };

        // Act
        var result = _calculator.FilterBestAttempts(courses).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().Score!.CourseScore.Should().Be(5.5m);
    }

    [Fact]
    public void FilterBestAttempts_ShouldFallbackToLatestAttempt_WhenNoAttemptsAreGraded()
    {
        // Arrange
        var date1 = DateTime.UtcNow.AddDays(-5);
        var date2 = DateTime.UtcNow.AddDays(-1);
        var courses = new List<Course>
        {
            new() { CourseCode = "CS101", CreatedAt = date1, Score = null },
            new() { CourseCode = "CS101", CreatedAt = date2, Score = null }
        };

        // Act
        var result = _calculator.FilterBestAttempts(courses).ToList();

        // Assert
        result.Should().HaveCount(1);
        result.First().CreatedAt.Should().Be(date2);
    }
}
