using AcademicGPA.Application.Features.Students.Commands.ChangePassword;
using AcademicGPA.Application.Features.Students.Commands.UpdatePreferences;
using AcademicGPA.Application.Features.Students.Commands.UpdateProfile;
using AcademicGPA.Application.Features.Students.Commands.UploadAvatar;
using AcademicGPA.Application.Features.Students.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/students")]
[Authorize] // Requires authentication for all profile endpoints
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var query = new GetStudentProfileQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result, null));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var command = new UpdateStudentProfileCommand(
            request.StudentCode,
            request.UniversityName,
            request.MajorName,
            request.EnrollmentYear,
            request.TotalRequiredCredits
        );

        var result = await _mediator.Send(command);
        return Ok(new ApiResponse(true, result, null, "Profile updated successfully."));
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        var command = new UpdateUserPreferencesCommand(
            request.PreferredLanguage,
            request.PreferredTheme
        );

        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, null, "Preferences updated successfully."));
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand(
            request.CurrentPassword,
            request.NewPassword
        );

        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, null, "Password has been changed successfully."));
    }

    [HttpPost("profile/avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ApiResponse(false, null, new { error = new[] { "No file uploaded." } }));
        }

        using (var stream = file.OpenReadStream())
        {
            var command = new UploadAvatarCommand(
                stream,
                file.FileName,
                file.ContentType,
                file.Length
            );

            var avatarUrl = await _mediator.Send(command);
            return Ok(new ApiResponse(true, new { avatarUrl }, null, "Avatar uploaded successfully."));
        }
    }

    // Standard API response envelope matching conventions
    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    // Request Models
    public record UpdateProfileRequest(
        string StudentCode,
        string UniversityName,
        string MajorName,
        int EnrollmentYear,
        int TotalRequiredCredits
    );

    public record UpdatePreferencesRequest(
        string PreferredLanguage,
        string PreferredTheme
    );

    public record ChangePasswordRequest(
        string CurrentPassword,
        string NewPassword
    );
}
