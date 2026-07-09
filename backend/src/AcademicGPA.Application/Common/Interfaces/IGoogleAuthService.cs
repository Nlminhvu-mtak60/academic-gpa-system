namespace AcademicGPA.Application.Common.Interfaces;

public record GoogleUserInfo(string GoogleId, string Email, string FirstName, string LastName);

/// <summary>
/// Service managing external Google OAuth credential verification.
/// </summary>
public interface IGoogleAuthService
{
    /// <summary>
    /// Verifies a Google ID token and returns the user's Google profile information.
    /// </summary>
    Task<GoogleUserInfo?> VerifyTokenAsync(string idToken);
}
