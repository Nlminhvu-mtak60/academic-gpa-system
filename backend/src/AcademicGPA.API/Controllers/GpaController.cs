using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Gpa.Queries.GetAcademicYearGpa;
using AcademicGPA.Application.Features.Gpa.Queries.GetCumulativeGpa;
using AcademicGPA.Application.Features.Gpa.Queries.GetGpaClassification;
using AcademicGPA.Application.Features.Gpa.Queries.GetSemesterGpa;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/gpa")]
[Authorize]
public class GpaController : ControllerBase
{
    private readonly IMediator _mediator;

    public GpaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("semester/{semesterId:guid}")]
    public async Task<IActionResult> GetSemesterGpa(Guid semesterId)
    {
        var query = new GetSemesterGpaQuery(semesterId);
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpGet("academic-year/{yearId:guid}")]
    public async Task<IActionResult> GetAcademicYearGpa(Guid yearId)
    {
        var query = new GetAcademicYearGpaQuery(yearId);
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpGet("cumulative")]
    public async Task<IActionResult> GetCumulativeGpa()
    {
        var query = new GetCumulativeGpaQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpGet("classification")]
    public async Task<IActionResult> GetGpaClassification()
    {
        var query = new GetGpaClassificationQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
