using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

[ApiController]
[Route("api/v1/export")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpGet("pdf")]
    public async Task<IActionResult> ExportPdf(CancellationToken ct)
    {
        var studentProfileIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(studentProfileIdString, out var studentProfileId))
            return Unauthorized();

        var bytes = await _exportService.ExportAcademicReportPdfAsync(studentProfileId, ct);
        return File(bytes, "application/pdf", $"AcademicReport_{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet("excel")]
    public async Task<IActionResult> ExportExcel(CancellationToken ct)
    {
        var studentProfileIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(studentProfileIdString, out var studentProfileId))
            return Unauthorized();

        var bytes = await _exportService.ExportAcademicReportExcelAsync(studentProfileId, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AcademicReport_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
