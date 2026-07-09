using AcademicGPA.Application.Features.Students.Commands.ChangePassword;
using AcademicGPA.Application.Features.Students.Commands.UpdatePreferences;
using AcademicGPA.Application.Features.Students.Commands.UpdateProfile;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class StudentTests
{
    [Theory]
    [InlineData("2024ABC", "Tech Uni", "Software Eng", 2024, 120, true)]
    [InlineData("2024_ABC", "Tech Uni", "Software Eng", 2024, 120, false)] // Alphanumeric check fail
    [InlineData("2024", "Tech Uni", "Software Eng", 1999, 120, false)]    // Year range fail
    [InlineData("2024", "Tech Uni", "Software Eng", 2024, 25, false)]     // Credits min fail
    [InlineData("2024", "Tech Uni", "Software Eng", 2024, 350, false)]    // Credits max fail
    public void UpdateStudentProfileCommandValidator_ShouldValidateCorrectly(
        string studentCode, 
        string universityName, 
        string majorName, 
        int enrollmentYear, 
        int totalRequiredCredits, 
        bool expectedValid)
    {
        // Arrange
        var validator = new UpdateStudentProfileCommandValidator();
        var command = new UpdateStudentProfileCommand(
            studentCode, 
            universityName, 
            majorName, 
            enrollmentYear, 
            totalRequiredCredits
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("vi", "light", true)]
    [InlineData("en", "dark", true)]
    [InlineData("fr", "light", false)] // Invalid language code
    [InlineData("vi", "blue", false)]   // Invalid theme
    public void UpdateUserPreferencesCommandValidator_ShouldValidateCorrectly(
        string lang, 
        string theme, 
        bool expectedValid)
    {
        // Arrange
        var validator = new UpdateUserPreferencesCommandValidator();
        var command = new UpdateUserPreferencesCommand(lang, theme);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData("OldPassword!", "NewSecure123!", true)]
    [InlineData("OldPassword!", "weak", false)]         // Too short
    [InlineData("OldPassword!", "NoSpecialChar1", false)] // Missing symbol
    public void ChangePasswordCommandValidator_ShouldValidateCorrectly(
        string currentPassword, 
        string newPassword, 
        bool expectedValid)
    {
        // Arrange
        var validator = new ChangePasswordCommandValidator();
        var command = new ChangePasswordCommand(currentPassword, newPassword);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}
