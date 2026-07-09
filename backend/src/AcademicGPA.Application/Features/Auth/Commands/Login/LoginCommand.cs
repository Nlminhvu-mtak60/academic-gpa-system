using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Auth.DTOs;
using AcademicGPA.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password,
    string IpAddress
) : IRequest<AuthResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IAdminService _adminService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IAdminService adminService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _adminService = adminService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch user by email including active refresh tokens
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        // 2. Validate user existence and password (and check IsDeleted)
        if (user == null || user.IsDeleted || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("Credentials", "Invalid email or password.");
        }

        // 3. Check if account is active
        if (!user.IsActive)
        {
            throw new ForbiddenException("Your account has been locked. Please contact support.");
        }

        // 4. Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(request.IpAddress);

        // 5. Invalidate old tokens (good practice on manual login)
        foreach (var existingToken in user.RefreshTokens.Where(t => t.IsActive))
        {
            existingToken.RevokedAt = DateTime.UtcNow;
        }

        refreshToken.UserId = user.Id;
        _context.RefreshTokens.Add(refreshToken);
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // 6. Save modifications
        await _context.SaveChangesAsync(cancellationToken);

        // Audit successful login
        await _adminService.LogActivityAsync(user.Id, "Login", request.IpAddress, cancellationToken);

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
