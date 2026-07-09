using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Dashboard.Queries.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves consolidated summary data for the student dashboard.
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var query = new GetDashboardSummaryQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
