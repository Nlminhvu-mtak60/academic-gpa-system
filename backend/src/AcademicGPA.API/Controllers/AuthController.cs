using AcademicGPA.Application.Features.Auth.Commands.ForgotPassword;
using AcademicGPA.Application.Features.Auth.Commands.GoogleLogin;
using AcademicGPA.Application.Features.Auth.Commands.Login;
using AcademicGPA.Application.Features.Auth.Commands.Logout;
using AcademicGPA.Application.Features.Auth.Commands.RefreshToken;
using AcademicGPA.Application.Features.Auth.Commands.Register;
using AcademicGPA.Application.Features.Auth.Commands.ResetPassword;
using AcademicGPA.Application.Features.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            GetIpAddress()
        );

        var result = await _mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new ApiResponse(true, result, null));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password, GetIpAddress());
        var result = await _mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new ApiResponse(true, result, null));
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // Read refresh token from cookie first, fallback to request body
        var refreshToken = Request.Cookies["refreshToken"] ?? request.RefreshToken;

        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new ApiResponse(false, null, new { error = new[] { "Refresh token is required." } }));
        }

        var command = new RefreshTokenCommand(request.AccessToken, refreshToken, GetIpAddress());
        var result = await _mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new ApiResponse(true, result, null));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        var refreshToken = Request.Cookies["refreshToken"] ?? request?.RefreshToken;

        if (!string.IsNullOrEmpty(refreshToken))
        {
            var command = new LogoutCommand(refreshToken);
            await _mediator.Send(command);
        }

        Response.Cookies.Delete("refreshToken");
        return Ok(new ApiResponse(true, "Logged out successfully", null));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await _mediator.Send(command);

        return Ok(new ApiResponse(true, "If the email is registered, a password recovery link has been sent.", null));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        await _mediator.Send(command);

        return Ok(new ApiResponse(true, "Password has been reset successfully.", null));
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var command = new GoogleLoginCommand(request.IdToken, GetIpAddress());
        var result = await _mediator.Send(command);
        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(new ApiResponse(true, result, null));
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"]!;

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
    }

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Force secure in prod
            SameSite = SameSiteMode.None, // Supports cross-site cookie usage
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/api/v1/auth"
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    // Helper API Response classes
    private record ApiResponse(bool Success, object? Data, object? Errors)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    // Request Models
    public record RegisterRequest(string Email, string Password, string FirstName, string LastName);
    public record LoginRequest(string Email, string Password);
    public record RefreshTokenRequest(string AccessToken, string? RefreshToken);
    public record LogoutRequest(string? RefreshToken);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);
    public record GoogleLoginRequest(string IdToken);
}
