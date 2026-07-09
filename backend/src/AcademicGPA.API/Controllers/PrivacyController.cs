using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Settings.Commands.DeleteAcademicHistory;
using AcademicGPA.Application.Features.Settings.Commands.DeleteAccount;
using AcademicGPA.Application.Features.Settings.Queries.ExportPersonalData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/privacy")]
[Authorize]
public class PrivacyController : ControllerBase
{
    private readonly IMediator _mediator;

    public PrivacyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
    {
        var studentProfileIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(studentProfileIdString, out var studentProfileId))
            return Unauthorized();

        var command = new DeleteAccountCommand(studentProfileId);
        await _mediator.Send(command);

        Response.Cookies.Delete("refreshToken");
        return Ok(new { success = true, message = "Account deleted successfully." });
    }

    [HttpDelete("academic-history")]
    public async Task<IActionResult> DeleteAcademicHistory()
    {
        var studentProfileIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(studentProfileIdString, out var studentProfileId))
            return Unauthorized();

        var command = new DeleteAcademicHistoryCommand(studentProfileId);
        await _mediator.Send(command);

        return Ok(new { success = true, message = "Academic history deleted successfully." });
    }

    [HttpGet("data-export")]
    public async Task<IActionResult> ExportPersonalData()
    {
        var studentProfileIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(studentProfileIdString, out var studentProfileId))
            return Unauthorized();

        var query = new ExportPersonalDataQuery(studentProfileId);
        var json = await _mediator.Send(query);

        return File(Encoding.UTF8.GetBytes(json), "application/json", $"PersonalData_{DateTime.Now:yyyyMMdd}.json");
    }
}
