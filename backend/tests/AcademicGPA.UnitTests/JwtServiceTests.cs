using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly string _secret = "SuperSecretKeyEnsure32CharactersLong!";
    private readonly string _issuer = "gpa-api-server";
    private readonly string _audience = "gpa-client-app";

    public JwtServiceTests()
    {
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["Jwt:Secret"]).Returns(_secret);
        _mockConfig.Setup(c => c["Jwt:Issuer"]).Returns(_issuer);
        _mockConfig.Setup(c => c["Jwt:Audience"]).Returns(_audience);
        _mockConfig.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("15");
        _mockConfig.Setup(c => c["Jwt:RefreshTokenExpiryDays"]).Returns("7");
    }

    [Fact]
    public void GenerateAccessToken_ShouldProduceValidJwt()
    {
        // Arrange
        var jwtService = new JwtService(_mockConfig.Object);
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

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be(_issuer);
        jwtToken.Audiences.Should().Contain(_audience);
        
        var subClaim = jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        subClaim.Should().Be(user.Id.ToString());

        var emailClaim = jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value;
        emailClaim.Should().Be(user.Email);

        var roleClaim = jwtToken.Claims.First(c => c.Type == "role").Value;
        roleClaim.Should().Be(UserRole.Student.ToString());
    }

    [Fact]
    public void GenerateRefreshToken_ShouldProduceCryptographicallyRandomToken()
    {
        // Arrange
        var jwtService = new JwtService(_mockConfig.Object);
        var ip = "127.0.0.1";

        // Act
        var refreshToken = jwtService.GenerateRefreshToken(ip);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Token.Should().NotBeNullOrWhiteSpace();
        refreshToken.Token.Length.Should().BeGreaterThan(40); // 64 bytes base64 encoded should be around 88 chars
        refreshToken.CreatedByIp.Should().Be(ip);
        refreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow.AddDays(6.9));
        refreshToken.ExpiresAt.Should().BeBefore(DateTime.UtcNow.AddDays(7.1));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_ShouldParseExpiredTokenCorrectly()
    {
        // Arrange
        var jwtService = new JwtService(_mockConfig.Object);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@university.edu.vn",
            Role = UserRole.Student
        };

        var token = jwtService.GenerateAccessToken(user);

        // Act
        var principal = jwtService.GetPrincipalFromExpiredToken(token);

        // Assert
        principal.Should().NotBeNull();
        var subClaim = principal!.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);
        subClaim.Should().NotBeNull();
        subClaim!.Value.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WithInvalidSignature_ShouldReturnNull()
    {
        // Arrange
        var jwtService = new JwtService(_mockConfig.Object);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "student@university.edu.vn",
            Role = UserRole.Student
        };

        var token = jwtService.GenerateAccessToken(user);
        
        // Let's modify the signature (the last part after the second dot) to make it invalid
        var parts = token.Split('.');
        parts[2] = "InvalidSignatureHashHereInvalidSignatureHashHere";
        var invalidToken = string.Join(".", parts);

        // Act
        var principal = jwtService.GetPrincipalFromExpiredToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }
}
