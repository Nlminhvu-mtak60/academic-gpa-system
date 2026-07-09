using System.Security.Claims;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class AuthorizationTests
{
    [Fact]
    public void CurrentUserService_WithStandardClaims_ShouldExtractCorrectDetails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "student@school.edu";
        var role = "Student";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(principal);

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var currentUserService = new CurrentUserService(mockAccessor.Object);

        // Act & Assert
        currentUserService.UserId.Should().Be(userId);
        currentUserService.Email.Should().Be(email);
        currentUserService.Role.Should().Be(role);
    }

    [Fact]
    public void CurrentUserService_WithAlternativeClaims_ShouldExtractCorrectDetails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "admin@school.edu";
        var role = "Admin";

        var claims = new[]
        {
            new Claim("sub", userId),
            new Claim("email", email),
            new Claim("role", role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(principal);

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var currentUserService = new CurrentUserService(mockAccessor.Object);

        // Act & Assert
        currentUserService.UserId.Should().Be(userId);
        currentUserService.Email.Should().Be(email);
        currentUserService.Role.Should().Be(role);
    }

    [Fact]
    public void CurrentUserService_WithNoHttpContext_ShouldReturnNulls()
    {
        // Arrange
        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);

        var currentUserService = new CurrentUserService(mockAccessor.Object);

        // Act & Assert
        currentUserService.UserId.Should().BeNull();
        currentUserService.Email.Should().BeNull();
        currentUserService.Role.Should().BeNull();
    }

    [Fact]
    public void CurrentUserService_WithNoUserClaims_ShouldReturnNulls()
    {
        // Arrange
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.User).Returns(principal);

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

        var currentUserService = new CurrentUserService(mockAccessor.Object);

        // Act & Assert
        currentUserService.UserId.Should().BeNull();
        currentUserService.Email.Should().BeNull();
        currentUserService.Role.Should().BeNull();
    }
}
