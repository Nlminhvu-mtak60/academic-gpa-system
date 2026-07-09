using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Statistics.Queries.GetCreditProgress;
using AcademicGPA.Application.Features.Statistics.Queries.GetGpaTrend;
using AcademicGPA.Application.Features.Statistics.Queries.GetGradeDistribution;
using AcademicGPA.Application.Features.Statistics.Queries.GetStrengthsWeaknesses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/statistics")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves GPA metrics organized chronologically by semester.
    /// </summary>
    [HttpGet("gpa-trend")]
    public async Task<IActionResult> GetGpaTrend()
    {
        var query = new GetGpaTrendQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    /// <summary>
    /// Retrieves the total count of earned letter grades.
    /// </summary>
    [HttpGet("grade-distribution")]
    public async Task<IActionResult> GetGradeDistribution()
    {
        var query = new GetGradeDistributionQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    /// <summary>
    /// Retrieves credit completion progress data.
    /// </summary>
    [HttpGet("credit-progress")]
    public async Task<IActionResult> GetCreditProgress()
    {
        var query = new GetCreditProgressQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    /// <summary>
    /// Analyzes historical performance to identify the student's strongest and weakest courses.
    /// </summary>
    [HttpGet("strengths-weaknesses")]
    public async Task<IActionResult> GetStrengthsWeaknesses()
    {
        var query = new GetStrengthsWeaknessesQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
