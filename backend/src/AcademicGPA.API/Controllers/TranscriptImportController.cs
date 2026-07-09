using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.Commands.ConfirmImport;
using AcademicGPA.Application.Features.TranscriptImport.Commands.ParseTranscript;
using AcademicGPA.Application.Features.TranscriptImport.Commands.UndoImport;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/semesters/{semesterId}/transcript")]
public class TranscriptImportController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranscriptImportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("parse")]
    public async Task<IActionResult> ParseTranscript(
        [FromRoute] Guid semesterId, 
        IFormFile file, 
        [FromForm] string sourceType)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        using var stream = file.OpenReadStream();
        var command = new ParseTranscriptCommand(semesterId, stream, sourceType, file.FileName);
        var result = await _mediator.Send(command);

        return Ok(new { success = true, data = result });
    }

    [HttpPost("parse-text")]
    public async Task<IActionResult> ParseTextTranscript(
        [FromRoute] Guid semesterId, 
        [FromBody] ParseTextRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Text is empty.");

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(request.Text));
        var command = new ParseTranscriptCommand(semesterId, stream, "Text", null);
        var result = await _mediator.Send(command);

        return Ok(new { success = true, data = result });
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmImport(
        [FromRoute] Guid semesterId, 
        [FromBody] ConfirmTranscriptImportRequest request)
    {
        var command = new ConfirmTranscriptImportCommand(semesterId, request.Courses, request.SourceType);
        var batchId = await _mediator.Send(command);

        return Ok(new { success = true, data = new { batchId } });
    }
}

public record ParseTextRequest(string Text);

public record ConfirmTranscriptImportRequest(
    string SourceType,
    List<ImportedCourseDto> Courses
);
