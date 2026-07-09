using System.Threading.Tasks;
using AcademicGPA.Application.Features.Settings.Commands;
using AcademicGPA.Application.Features.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var query = new GetUserSettingsQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateUserSettingsCommand command)
    {
        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, Message: "Settings updated successfully."));
    }

    [HttpPut("email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateUserEmailCommand command)
    {
        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, Message: "Email updated successfully."));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
