namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service managing transactional email communications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Dispatches an email asynchronously.
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Sends an account email verification link.
    /// </summary>
    Task SendVerificationEmailAsync(string to, string token);

    /// <summary>
    /// Sends a password reset recovery link.
    /// </summary>
    Task SendPasswordResetEmailAsync(string to, string token);
}
