using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Semesters.Commands.CreateSemester;
using AcademicGPA.Application.Features.Semesters.Commands.DeleteSemester;
using AcademicGPA.Application.Features.Semesters.Commands.UpdateSemester;
using AcademicGPA.Application.Features.Semesters.Commands.ImportSemester;
using AcademicGPA.Application.Features.Semesters.Queries.GetSemesters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class SemestersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SemestersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("academic-years/{yearId:guid}/semesters")]
    public async Task<IActionResult> GetSemesters(Guid yearId)
    {
        var query = new GetSemestersQuery(yearId);
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    [HttpPost("academic-years/{yearId:guid}/semesters")]
    public async Task<IActionResult> Create(Guid yearId, [FromBody] CreateSemesterRequest request)
    {
        var command = new CreateSemesterCommand(yearId, request.SemesterName);
        var result = await _mediator.Send(command);
        
        var responseData = new
        {
            id = result.Id,
            semesterName = result.SemesterName,
            sortOrder = result.SortOrder
        };

        return StatusCode(201, new ApiResponse(true, responseData, null, "Semester created successfully."));
    }

    [HttpPost("academic-years/{yearId:guid}/semesters/import")]
    public async Task<IActionResult> Import(Guid yearId, [FromBody] ImportSemesterRequest request)
    {
        var command = new ImportHistoricalSemesterCommand(
            yearId,
            request.SemesterName,
            request.ImportedCredits,
            request.ImportedGpa10,
            request.ImportedGpa4
        );
        var result = await _mediator.Send(command);
        
        var responseData = new
        {
            id = result.Id,
            semesterName = result.SemesterName,
            sortOrder = result.SortOrder,
            isImported = result.IsImported,
            importedCredits = result.ImportedCredits,
            importedGpa10 = result.ImportedGpa10,
            importedGpa4 = result.ImportedGpa4
        };

        return StatusCode(201, new ApiResponse(true, responseData, null, "Historical semester imported successfully."));
    }

    [HttpPut("semesters/{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSemesterRequest request)
    {
        var command = new UpdateSemesterCommand(
            id, 
            request.SemesterName,
            request.ImportedCredits,
            request.ImportedGpa10,
            request.ImportedGpa4
        );
        await _mediator.Send(command);
        return Ok(new ApiResponse(true, null, null, "Semester updated successfully."));
    }

    [HttpDelete("semesters/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteSemesterCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    public record CreateSemesterRequest(string SemesterName);
    public record UpdateSemesterRequest(
        string SemesterName,
        int? ImportedCredits,
        decimal? ImportedGpa10,
        decimal? ImportedGpa4
    );
    public record ImportSemesterRequest(string SemesterName, int ImportedCredits, decimal ImportedGpa10, decimal ImportedGpa4);
}
