using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace AcademicGPA.UnitTests;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_WithValidPassword_ShouldReturnValidHash()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "MySecurePassword123!";

        // Act
        var hash = hasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().StartWith("$2"); // BCrypt prefix
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "MySecurePassword123!";
        var hash = hasher.HashPassword(password);

        // Act
        var isValid = hasher.VerifyPassword(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "MySecurePassword123!";
        var wrongPassword = "WrongPassword123!";
        var hash = hasher.HashPassword(password);

        // Act
        var isValid = hasher.VerifyPassword(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_ShouldGenerateUniqueHashesForSamePasswordDueToRandomSalt()
    {
        // Arrange
        var hasher = new PasswordHasher();
        var password = "MySecurePassword123!";

        // Act
        var hash1 = hasher.HashPassword(password);
        var hash2 = hasher.HashPassword(password);

        // Assert
        hash1.Should().NotBeNullOrWhiteSpace();
        hash2.Should().NotBeNullOrWhiteSpace();
        hash1.Should().NotBe(hash2);
        
        hasher.VerifyPassword(password, hash1).Should().BeTrue();
        hasher.VerifyPassword(password, hash2).Should().BeTrue();
    }

    [Fact]
    public void HashPassword_WithNullOrEmptyPassword_ShouldThrowArgumentNullException()
    {
        // Arrange
        var hasher = new PasswordHasher();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => hasher.HashPassword(null!));
        var emptyHash = hasher.HashPassword("");
        emptyHash.Should().NotBeNullOrWhiteSpace();
    }
}
