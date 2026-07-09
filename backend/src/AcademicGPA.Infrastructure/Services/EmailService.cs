using AcademicGPA.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AcademicGPA.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // Log transaction for local debugging/testing. Can be connected to SMTP/SendGrid clients.
        _logger.LogInformation("Sending email to {To}. Subject: {Subject}. Body: {Body}", to, subject, body);
        return Task.CompletedTask;
    }

    public Task SendVerificationEmailAsync(string to, string token)
    {
        var verificationLink = $"http://localhost:5173/verify-email?token={token}";
        var body = $"Please verify your account by clicking the following link: {verificationLink}";
        return SendEmailAsync(to, "Verify your Academic GPA Account", body);
    }

    public Task SendPasswordResetEmailAsync(string to, string token)
    {
        var resetLink = $"http://localhost:5173/reset-password?token={token}";
        var body = $"Please reset your password by clicking the following link: {resetLink}";
        return SendEmailAsync(to, "Reset your Academic GPA Account Password", body);
    }
}
