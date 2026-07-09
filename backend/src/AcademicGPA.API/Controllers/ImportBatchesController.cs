using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.Commands.UndoImport;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/import-batches")]
public class ImportBatchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImportBatchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpDelete("{batchId}")]
    public async Task<IActionResult> UndoImport([FromRoute] Guid batchId)
    {
        await _mediator.Send(new UndoImportCommand(batchId));
        return Ok(new { success = true });
    }
}
