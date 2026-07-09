using System;
using AcademicGPA.Application.Features.AcademicYears.Commands.CreateAcademicYear;
using AcademicGPA.Application.Features.AcademicYears.Commands.UpdateAcademicYear;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class AcademicYearTests
{
    [Theory]
    [InlineData("2024-2025", 2024, 2025, true)]
    [InlineData("2024-2024", 2024, 2024, true)]
    [InlineData("", 2024, 2025, false)] // Empty name
    [InlineData("2024-2025", 1999, 2000, false)] // Start year too early
    [InlineData("2024-2025", 2101, 2102, false)] // Start year too late
    [InlineData("2024-2025", 2024, 2026, false)] // End year > start + 1
    [InlineData("2024-2025", 2024, 2023, false)] // End year < start
    public void CreateAcademicYearCommandValidator_ShouldValidateCorrectly(
        string yearName,
        int startYear,
        int endYear,
        bool expectedValid)
    {
        // Arrange
        var validator = new CreateAcademicYearCommandValidator();
        var command = new CreateAcademicYearCommand(yearName, startYear, endYear);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void UpdateAcademicYearCommandValidator_ShouldBeInvalid_WhenIdIsEmpty()
    {
        // Arrange
        var validator = new UpdateAcademicYearCommandValidator();
        var command = new UpdateAcademicYearCommand(
            Guid.Empty,
            "2024-2025",
            2024,
            2025
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void UpdateAcademicYearCommandValidator_ShouldBeInvalid_WhenEndDateBeforeStartDate()
    {
        // Arrange
        var validator = new UpdateAcademicYearCommandValidator();
        var command = new UpdateAcademicYearCommand(
            Guid.NewGuid(),
            "2024-2025",
            2024,
            2025,
            new DateTime(2024, 9, 1),
            new DateTime(2024, 8, 31) // End date before start date
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EndDate");
    }

    [Fact]
    public void UpdateAcademicYearCommandValidator_ShouldBeValid_WhenDatesAreCorrect()
    {
        // Arrange
        var validator = new UpdateAcademicYearCommandValidator();
        var command = new UpdateAcademicYearCommand(
            Guid.NewGuid(),
            "2024-2025",
            2024,
            2025,
            new DateTime(2024, 9, 1),
            new DateTime(2025, 6, 30)
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
