using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Admin;
using AcademicGPA.Application.Features.Admin.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AdminController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    private string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
    }

    private Guid GetAdminUserId()
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("Admin user is not authenticated.");
        }
        return userId;
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var result = await _mediator.Send(new GetAdminStatisticsQuery());
        return Ok(new ApiResponse(true, result));
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null)
    {
        var result = await _mediator.Send(new GetAdminStudentsQuery(page, pageSize, search, isActive, sortBy, sortOrder));
        return Ok(new ApiResponse(true, new { result.Items, result.TotalCount }));
    }

    [HttpGet("students/{id:guid}")]
    public async Task<IActionResult> GetStudentDetails(Guid id)
    {
        var result = await _mediator.Send(new GetAdminStudentDetailsQuery(id));
        return Ok(new ApiResponse(true, result));
    }

    [HttpPut("students/{id:guid}")]
    public async Task<IActionResult> EditStudentInfo(Guid id, [FromBody] EditStudentInfoDto dto)
    {
        await _mediator.Send(new EditStudentInfoCommand(id, dto, GetIpAddress()));
        return Ok(new ApiResponse(true, null, null, "Student info updated successfully."));
    }

    [HttpPut("students/{id:guid}/lock")]
    public async Task<IActionResult> LockStudent(Guid id, [FromBody] LockStudentRequest request)
    {
        await _mediator.Send(new LockStudentCommand(id, request.Reason, GetIpAddress()));
        return Ok(new ApiResponse(true, null, null, "Student account locked successfully."));
    }

    [HttpPut("students/{id:guid}/unlock")]
    public async Task<IActionResult> UnlockStudent(Guid id)
    {
        await _mediator.Send(new UnlockStudentCommand(id, GetIpAddress()));
        return Ok(new ApiResponse(true, null, null, "Student account unlocked successfully."));
    }

    [HttpDelete("students/{id:guid}")]
    public async Task<IActionResult> DeleteStudent(Guid id)
    {
        await _mediator.Send(new DeleteStudentCommand(id, GetIpAddress()));
        return NoContent();
    }

    [HttpPost("students/{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id)
    {
        var tempPassword = await _mediator.Send(new ResetStudentPasswordCommand(id, GetIpAddress()));
        return Ok(new ApiResponse(true, new { TemporaryPassword = tempPassword }, null, "Password reset successfully. Please share the temporary password with the student."));
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetAdminUsersQuery(page, pageSize, search));
        return Ok(new ApiResponse(true, new { result.Items, result.TotalCount }));
    }

    [HttpGet("activity-logs")]
    public async Task<IActionResult> GetActivityLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null)
    {
        var result = await _mediator.Send(new GetAdminActivityLogsQuery(page, pageSize, userId));
        return Ok(new ApiResponse(true, new { result.Items, result.TotalCount }));
    }

    [HttpPost("notifications")]
    public async Task<IActionResult> SendDirectNotification([FromBody] SendDirectNotificationRequest request)
    {
        await _mediator.Send(new SendDirectNotificationCommand(
            GetAdminUserId(),
            request.RecipientId,
            request.Title,
            request.Message,
            request.Type
        ));
        return StatusCode(201, new ApiResponse(true, null, null, "Notification sent successfully."));
    }

    [HttpPost("notifications/broadcast")]
    public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationRequest request)
    {
        await _mediator.Send(new BroadcastNotificationCommand(
            GetAdminUserId(),
            request.Title,
            request.Message
        ));
        return StatusCode(201, new ApiResponse(true, null, null, "Broadcast notification dispatched successfully."));
    }

    [HttpGet("notifications/history")]
    public async Task<IActionResult> GetNotificationHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetAdminNotificationHistoryQuery(page, pageSize));
        return Ok(new ApiResponse(true, new { result.Items, result.TotalCount }));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    public record LockStudentRequest(string Reason);
    public record SendDirectNotificationRequest(Guid RecipientId, string Title, string Message, string Type);
    public record BroadcastNotificationRequest(string Title, string Message);
}
