using System.Security.Claims;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Auth.DTOs;
using AcademicGPA.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken,
    string IpAddress
) : IRequest<AuthResponseDto>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Access token is required.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Extract ClaimsPrincipal from expired AccessToken
        var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            throw new ForbiddenException("Invalid access token signature.");
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new ForbiddenException("Invalid token claims payload.");
        }

        // 2. Load user and all active refresh tokens from DB
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new ForbiddenException("User account is locked or inactive.");
        }

        // 3. Find the submitted RefreshToken
        var submittedToken = user.RefreshTokens
            .FirstOrDefault(t => t.Token == request.RefreshToken);

        // 4. Token Reuse / Breach Detection Mitigation
        if (submittedToken == null)
        {
            throw new ForbiddenException("Session token does not exist.");
        }

        if (submittedToken.IsRevoked)
        {
            // Token has been revoked previously! This indicates potential token theft/reuse.
            // Revoke ALL active refresh tokens for the user to force complete logout.
            foreach (var activeToken in user.RefreshTokens.Where(t => t.IsActive))
            {
                activeToken.RevokedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException("Security alert: Token reuse detected. All active sessions terminated.");
        }

        if (submittedToken.IsExpired)
        {
            throw new ForbiddenException("Refresh token session expired. Please log in again.");
        }

        // 5. Token is active and valid: Rotate it
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken(request.IpAddress);

        // Invalidate old token and link it to the new one
        submittedToken.RevokedAt = DateTime.UtcNow;
        submittedToken.ReplacedByToken = newRefreshToken.Token;

        newRefreshToken.UserId = user.Id;
        _context.RefreshTokens.Add(newRefreshToken);
        user.UpdatedAt = DateTime.UtcNow;

        // 6. Save changes
        await _context.SaveChangesAsync(cancellationToken);

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

        return new AuthResponseDto(newAccessToken, newRefreshToken.Token, userDto);
    }
}
