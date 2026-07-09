using AcademicGPA.Application.Features.Students.Commands.UpdateProfile;
using AcademicGPA.Application.Features.Auth.Commands.Register;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class InputValidationSecurityTests
{
    [Theory]
    [InlineData("STUDENT123", "Hanoi University", "Computer Science", 2024, 120, true)]
    [InlineData("STU_123", "Hanoi University", "Computer Science", 2024, 120, false)] // non-alphanumeric student code
    [InlineData("STUDENT123", "", "Computer Science", 2024, 120, false)] // empty university
    [InlineData("STUDENT123", "Hanoi University", "", 2024, 120, false)] // empty major
    [InlineData("STUDENT123", "Hanoi University", "Computer Science", 1999, 120, false)] // year too low
    [InlineData("STUDENT123", "Hanoi University", "Computer Science", 2101, 120, false)] // year too high
    [InlineData("STUDENT123", "Hanoi University", "Computer Science", 2024, 29, false)] // credits too low
    [InlineData("STUDENT123", "Hanoi University", "Computer Science", 2024, 301, false)] // credits too high
    public void UpdateStudentProfileCommandValidator_ShouldValidateCorrectly(
        string studentCode, string universityName, string majorName, int enrollmentYear, int totalRequiredCredits, bool expectedValid)
    {
        // Arrange
        var validator = new UpdateStudentProfileCommandValidator();
        var command = new UpdateStudentProfileCommand(studentCode, universityName, majorName, enrollmentYear, totalRequiredCredits);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }

    [Fact]
    public void UpdateStudentProfileCommandValidator_WithLongStrings_ShouldFail()
    {
        // Arrange
        var validator = new UpdateStudentProfileCommandValidator();
        var longCode = new string('A', 51);
        var longUni = new string('B', 201);
        var longMajor = new string('C', 201);
        
        var command = new UpdateStudentProfileCommand(longCode, longUni, longMajor, 2024, 120);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StudentCode");
        result.Errors.Should().Contain(e => e.PropertyName == "UniversityName");
        result.Errors.Should().Contain(e => e.PropertyName == "MajorName");
    }

    [Fact]
    public void UpdateStudentProfileCommandValidator_WithUnicodeAndEmojis_ShouldValidateSuccessfully()
    {
        // Arrange
        var validator = new UpdateStudentProfileCommandValidator();
        // StudentCode must be alphanumeric: alphanumeric does not allow emoji, but allows English letters and numbers.
        // UniversityName and MajorName can have Vietnamese characters and emojis.
        var command = new UpdateStudentProfileCommand(
            "STUDENT999",
            "Đại học Bách Khoa Hà Nội 🏫",
            "Công nghệ thông tin 💻",
            2024,
            120
        );

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("'; DROP TABLE Users; --")]
    [InlineData("<script>alert('xss')</script>")]
    public void UpdateStudentProfileCommandValidator_WithSQLInjectionAndXSSPayloads_ShouldAllowValidation_ButFailStudentCodeConstraint(string payload)
    {
        // Arrange
        var validator = new UpdateStudentProfileCommandValidator();
        // Since StudentCode is alphanumeric, it will fail when payload is in StudentCode
        var commandCode = new UpdateStudentProfileCommand(payload, "Valid Uni", "Valid Major", 2024, 120);
        
        // Act
        var resultCode = validator.Validate(commandCode);

        // Assert
        resultCode.IsValid.Should().BeFalse();
        resultCode.Errors.Should().Contain(e => e.PropertyName == "StudentCode");

        // However, payloads in UniversityName/MajorName will pass validator checks (which only check non-empty & max-length), 
        // as database queries should use parameterized SQL and outputs should be HTML-encoded.
        var commandUni = new UpdateStudentProfileCommand("STUDENT123", payload, "Valid Major", 2024, 120);
        var resultUni = validator.Validate(commandUni);
        resultUni.IsValid.Should().BeTrue();
    }
}
