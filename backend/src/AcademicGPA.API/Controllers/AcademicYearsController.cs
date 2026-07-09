using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.AcademicYears.Commands.CreateAcademicYear;
using AcademicGPA.Application.Features.AcademicYears.Commands.DeleteAcademicYear;
using AcademicGPA.Application.Features.AcademicYears.Commands.SetCurrent;
using AcademicGPA.Application.Features.AcademicYears.Commands.UpdateAcademicYear;
using AcademicGPA.Application.Features.AcademicYears.Commands.ImportAcademicYear;
using AcademicGPA.Application.Features.AcademicYears.Queries.GetAcademicYears;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/academic-years")]
[Authorize]
public class AcademicYearsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AcademicYearsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAcademicYears()
    {
        var query = new GetAcademicYearsQuery();
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAcademicYearRequest request)
    {
        var command = new CreateAcademicYearCommand(request.YearName, request.StartYear, request.EndYear);
        var result = await _mediator.Send(command);
        // Returns 201 Created with standard Envelope structure
        return StatusCode(201, new ApiResponse(true, result, null, "Academic year created successfully."));
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportAcademicYearRequest request)
    {
        var command = new ImportHistoricalAcademicYearCommand(
            request.YearName,
            request.StartYear,
            request.EndYear,
            request.ImportedCredits,
            request.ImportedGpa10,
            request.ImportedGpa4
        );
        var result = await _mediator.Send(command);
        return StatusCode(201, new ApiResponse(true, result, null, "Historical academic year imported successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAcademicYearRequest request)
    {
        var command = new UpdateAcademicYearCommand(
            id, 
            request.YearName, 
            request.StartYear, 
            request.EndYear, 
            request.StartDate, 
            request.EndDate,
            request.ImportedCredits,
            request.ImportedGpa10,
            request.ImportedGpa4
        );
        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, null, "Academic year updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteAcademicYearCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPut("{id:guid}/current")]
    public async Task<IActionResult> SetCurrent(Guid id)
    {
        var command = new SetCurrentAcademicYearCommand(id);
        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, null, "Academic year set as current successfully."));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    public record CreateAcademicYearRequest(string YearName, int StartYear, int EndYear);
    public record UpdateAcademicYearRequest(
        string YearName, 
        int StartYear, 
        int EndYear, 
        DateTime? StartDate, 
        DateTime? EndDate,
        int? ImportedCredits,
        decimal? ImportedGpa10,
        decimal? ImportedGpa4
    );
    public record ImportAcademicYearRequest(string YearName, int StartYear, int EndYear, int ImportedCredits, decimal ImportedGpa10, decimal ImportedGpa4);
}
