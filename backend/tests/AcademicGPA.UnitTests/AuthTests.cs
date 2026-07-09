using System.Security.Claims;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Auth.Commands.Register;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class AuthTests
{
    [Fact]
    public void HashPassword_ShouldReturnValidHash()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "StrongPassword123!";

        // Act
        var hash = hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hasher.VerifyPassword(password, hash).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ShouldProduceTokenWithValidClaims()
    {
        // Arrange
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Jwt:Secret"]).Returns("SuperSecretKeyEnsure32CharactersLong!");
        mockConfig.Setup(c => c["Jwt:Issuer"]).Returns("gpa-api-server");
        mockConfig.Setup(c => c["Jwt:Audience"]).Returns("gpa-client-app");
        mockConfig.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("15");

        var jwtService = new JwtService(mockConfig.Object);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@university.edu.vn",
            Role = UserRole.Student
        };

        // Act
        var token = jwtService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        
        var principal = jwtService.GetPrincipalFromExpiredToken(token);
        principal.Should().NotBeNull();

        var emailClaim = principal!.FindFirst(ClaimTypes.Email) ?? principal.FindFirst("email");
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(user.Email);
    }

    [Theory]
    [InlineData("invalid-email", "Pass123!", false)]
    [InlineData("valid@email.com", "short", false)]
    [InlineData("valid@email.com", "NoSpecialChar1", false)]
    [InlineData("valid@email.com", "ValidPass123!", true)]
    public void RegisterCommandValidator_ShouldValidateCorrectly(string email, string password, bool expectedValid)
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand(email, password, "First", "Last", "127.0.0.1");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}
