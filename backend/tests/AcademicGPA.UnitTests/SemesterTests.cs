using System;
using AcademicGPA.Application.Features.Semesters.Commands.CreateSemester;
using AcademicGPA.Application.Features.Semesters.Commands.UpdateSemester;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class SemesterTests
{
    [Theory]
    [InlineData("Semester 1", true)]
    [InlineData("Semester 2", true)]
    [InlineData("Summer Semester", true)]
    [InlineData("", false)] // Empty name not allowed
    [InlineData("A very long semester name that exceeds fifty characters limit of database design", false)] // Max length 50
    public void CreateSemesterCommandValidator_ShouldValidateCorrectly(
        string semesterName,
        bool expectedValid)
    {
        // Arrange
        var validator = new CreateSemesterCommandValidator();
        var command = new CreateSemesterCommand(Guid.NewGuid(), semesterName);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("Semester 1", true)]
    [InlineData("Semester 2", true)]
    [InlineData("", false)] // Empty name not allowed
    [InlineData("A very long semester name that exceeds fifty characters limit of database design", false)] // Max length 50
    public void UpdateSemesterCommandValidator_ShouldValidateCorrectly(
        string semesterName,
        bool expectedValid)
    {
        // Arrange
        var validator = new UpdateSemesterCommandValidator();
        var command = new UpdateSemesterCommand(Guid.NewGuid(), semesterName);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}
