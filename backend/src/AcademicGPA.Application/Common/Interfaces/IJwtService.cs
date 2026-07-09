using System.Security.Claims;
using AcademicGPA.Domain.Entities;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service managing stateless JWT generation and verification.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a signed JWT access token for a user, valid for 15 minutes.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically secure random refresh token.
    /// </summary>
    RefreshToken GenerateRefreshToken(string ipAddress);

    /// <summary>
    /// Extracts user claims principal from an expired access token to validate token rotation requests.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
