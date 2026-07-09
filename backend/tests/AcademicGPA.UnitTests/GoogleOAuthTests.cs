using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Auth.Commands.GoogleLogin;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class GoogleOAuthTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public GoogleOAuthTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    [Fact]
    public async Task Handle_ShouldCreateNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        
        var mockGoogleAuth = new Mock<IGoogleAuthService>();
        mockGoogleAuth.Setup(g => g.VerifyTokenAsync("valid-token"))
            .ReturnsAsync(new GoogleUserInfo("google-sub-123", "new-user@gmail.com", "John", "Doe"));

        var mockJwt = new Mock<IJwtService>();
        mockJwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access-token-123");
        mockJwt.Setup(j => j.GenerateRefreshToken(It.IsAny<string>())).Returns(new RefreshToken { Token = "refresh-token-123" });

        var mockHasher = new Mock<IPasswordHasher>();
        mockHasher.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("hash");

        var mockAdmin = new Mock<IAdminService>();

        var handler = new GoogleLoginCommandHandler(context, mockGoogleAuth.Object, mockJwt.Object, mockHasher.Object, mockAdmin.Object);
        var command = new GoogleLoginCommand("valid-token", "127.0.0.1");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token-123");
        result.User.Email.Should().Be("new-user@gmail.com");
        result.User.Role.Should().Be(UserRole.Student.ToString());

        var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "new-user@gmail.com");
        dbUser.Should().NotBeNull();
        dbUser!.GoogleId.Should().Be("google-sub-123");
    }

    [Fact]
    public async Task Handle_ShouldLoginExistingUser_WhenUserExistsWithSameEmail()
    {
        // Arrange
        using var context = CreateContext();
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@gmail.com",
            FirstName = "Alice",
            LastName = "Smith",
            PasswordHash = "hash",
            Role = UserRole.Student,
            IsActive = true
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var mockGoogleAuth = new Mock<IGoogleAuthService>();
        mockGoogleAuth.Setup(g => g.VerifyTokenAsync("valid-token"))
            .ReturnsAsync(new GoogleUserInfo("google-sub-456", "existing@gmail.com", "Alice", "Smith"));

        var mockJwt = new Mock<IJwtService>();
        mockJwt.Setup(j => j.GenerateAccessToken(It.IsAny<User>())).Returns("access-token-456");
        mockJwt.Setup(j => j.GenerateRefreshToken(It.IsAny<string>())).Returns(new RefreshToken { Token = "refresh-token-456" });

        var mockHasher = new Mock<IPasswordHasher>();
        var mockAdmin = new Mock<IAdminService>();

        var handler = new GoogleLoginCommandHandler(context, mockGoogleAuth.Object, mockJwt.Object, mockHasher.Object, mockAdmin.Object);
        var command = new GoogleLoginCommand("valid-token", "127.0.0.1");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token-456");
        result.User.Email.Should().Be("existing@gmail.com");

        var dbUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "existing@gmail.com");
        dbUser.Should().NotBeNull();
        dbUser!.GoogleId.Should().Be("google-sub-456"); // GoogleId was successfully linked
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenVerificationFails()
    {
        // Arrange
        using var context = CreateContext();
        
        var mockGoogleAuth = new Mock<IGoogleAuthService>();
        mockGoogleAuth.Setup(g => g.VerifyTokenAsync("invalid-token"))
            .ReturnsAsync((GoogleUserInfo?)null);

        var mockJwt = new Mock<IJwtService>();
        var mockHasher = new Mock<IPasswordHasher>();
        var mockAdmin = new Mock<IAdminService>();

        var handler = new GoogleLoginCommandHandler(context, mockGoogleAuth.Object, mockJwt.Object, mockHasher.Object, mockAdmin.Object);
        var command = new GoogleLoginCommand("invalid-token", "127.0.0.1");

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
