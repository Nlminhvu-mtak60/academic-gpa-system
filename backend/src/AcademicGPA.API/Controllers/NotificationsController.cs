using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Notifications.Commands;
using AcademicGPA.Application.Features.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool? unreadOnly = null)
    {
        var query = new GetNotificationsQuery(page, pageSize, unreadOnly);
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var query = new GetUnreadCountQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var command = new MarkNotificationReadCommand(id);
        var result = await _mediator.Send(command);
        return Ok(new ApiResponse(true, result, Message: "Notification marked as read."));
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var command = new MarkAllNotificationsReadCommand();
        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, Message: "All notifications marked as read."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteNotification(Guid id)
    {
        var command = new DeleteNotificationCommand(id);
        var result = await _mediator.Send(command);
        return Ok(new ApiResponse(true, result, Message: "Notification deleted."));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
