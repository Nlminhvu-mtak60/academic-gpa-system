using System;
using AcademicGPA.Application.Features.Courses.Commands.CreateCourse;
using AcademicGPA.Application.Features.Courses.Commands.UpdateCourse;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class CourseTests
{
    [Theory]
    [InlineData("CS101", "Introduction to Computer Science", 3, false, null, true)]
    [InlineData("MATH201", "Calculus II", 4, false, null, true)]
    [InlineData("PE11", "Physical Education", 1, false, null, true)]
    [InlineData("", "Calculus II", 4, false, null, false)] // Empty code
    [InlineData("MATH-201", "Calculus II", 4, false, null, false)] // Special characters in code
    [InlineData("MATH201", "", 4, false, null, false)] // Empty name
    [InlineData("MATH201", "Calculus II", 0, false, null, false)] // Credits too low
    [InlineData("MATH201", "Calculus II", 7, false, null, false)] // Credits too high
    [InlineData("CS101", "Intro CS", 3, true, "e32fb33f-8461-460d-85fa-7c961e67fa8b", true)] // Retake with valid original ID
    [InlineData("CS101", "Intro CS", 3, true, null, false)] // Retake without original ID
    public void CreateCourseCommandValidator_ShouldValidateCorrectly(
        string courseCode,
        string courseName,
        int credits,
        bool isRetake,
        string? originalCourseIdStr,
        bool expectedValid)
    {
        // Arrange
        var validator = new CreateCourseCommandValidator();
        Guid? originalCourseId = string.IsNullOrEmpty(originalCourseIdStr) 
            ? null 
            : Guid.Parse(originalCourseIdStr);

        var command = new CreateCourseCommand(
            Guid.NewGuid(),
            courseCode,
            courseName,
            credits,
            isRetake,
            originalCourseId
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void UpdateCourseCommandValidator_ShouldBeInvalid_WhenIdIsEmpty()
    {
        // Arrange
        var validator = new UpdateCourseCommandValidator();
        var command = new UpdateCourseCommand(
            Guid.Empty,
            "CS101",
            "Intro CS",
            3,
            false,
            null
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public void UpdateCourseCommandValidator_ShouldBeInvalid_WhenRetakeHasNoOriginalId()
    {
        // Arrange
        var validator = new UpdateCourseCommandValidator();
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            "CS101",
            "Intro CS",
            3,
            true, // isRetake
            null // missing original ID
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OriginalCourseId");
    }
}
