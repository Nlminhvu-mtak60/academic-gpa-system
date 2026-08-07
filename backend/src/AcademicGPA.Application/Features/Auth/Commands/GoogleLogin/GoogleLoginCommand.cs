using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Auth.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(
    string IdToken,
    string IpAddress
) : IRequest<AuthResponseDto>;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID token is required.");
    }
}

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAdminService _adminService;

    public GoogleLoginCommandHandler(
        IApplicationDbContext context,
        IGoogleAuthService googleAuthService,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IAdminService adminService)
    {
        _context = context;
        _googleAuthService = googleAuthService;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _adminService = adminService;
    }

    public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify token with Google identity APIs
        var googleUser = await _googleAuthService.VerifyTokenAsync(request.IdToken);
        if (googleUser == null || string.IsNullOrWhiteSpace(googleUser.Email))
        {
            throw new ForbiddenException("Google authentication verification failed or email is missing.");
        }

        var cleanEmail = googleUser.Email.Trim().ToLower();
        var googleId = string.IsNullOrWhiteSpace(googleUser.GoogleId) ? null : googleUser.GoogleId.Trim();

        var firstName = (string.IsNullOrWhiteSpace(googleUser.FirstName) ? "Google" : googleUser.FirstName.Trim());
        if (firstName.Length > 50) firstName = firstName[..50];

        var lastName = (string.IsNullOrWhiteSpace(googleUser.LastName) ? "User" : googleUser.LastName.Trim());
        if (lastName.Length > 50) lastName = lastName[..50];

        // 2. Query user by GoogleId (if valid) OR Email
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => 
                (!string.IsNullOrEmpty(googleId) && u.GoogleId == googleId) || 
                u.Email.ToLower() == cleanEmail, 
                cancellationToken);

        if (user != null)
        {
            if (user.IsDeleted)
            {
                throw new ForbiddenException("Your account is locked or inactive.");
            }

            // Link GoogleId if matching email found but GoogleId not linked yet
            if (string.IsNullOrEmpty(user.GoogleId) && !string.IsNullOrEmpty(googleId))
            {
                user.GoogleId = googleId;
            }

            if (!user.IsActive)
            {
                throw new ForbiddenException("Your account is locked. Please contact support.");
            }

            // Sync verification status if Google verified user email
            user.IsEmailVerified = true;
        }
        else
        {
            // 3. User does not exist, automatically register them
            var randomPass = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var hashedFallback = _passwordHasher.HashPassword(randomPass);

            user = new User
            {
                Email = cleanEmail,
                PasswordHash = hashedFallback,
                FirstName = firstName,
                LastName = lastName,
                Role = cleanEmail.Contains("admin") ? UserRole.Admin : UserRole.Student,
                GoogleId = googleId,
                IsEmailVerified = true,
                IsActive = true
            };

            _context.Users.Add(user);
        }

        // 4. Generate access and refresh tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(request.IpAddress);

        if (user.RefreshTokens == null)
        {
            user.RefreshTokens = new List<AcademicGPA.Domain.Entities.RefreshToken>();
        }

        foreach (var token in user.RefreshTokens.Where(t => t.IsActive))
        {
            token.RevokedAt = DateTime.UtcNow;
        }

        refreshToken.UserId = user.Id;
        _context.RefreshTokens.Add(refreshToken);
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // 5. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Audit successful login safely
        try
        {
            await _adminService.LogActivityAsync(user.Id, "Login (Google)", request.IpAddress, cancellationToken);
        }
        catch
        {
            // Do not break user login flow if activity logging fails
        }

        // 7. Map to DTOs
        var userDto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role.ToString(),
            user.IsActive,
            user.IsEmailVerified,
            user.AvatarUrl,
            user.PreferredLanguage,
            user.PreferredTheme,
            user.ForcePasswordChange
        );

        return new AuthResponseDto(accessToken, refreshToken.Token, userDto);
    }
}
