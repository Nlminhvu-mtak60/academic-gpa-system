using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;
using Microsoft.Extensions.Logging;

namespace AcademicGPA.Infrastructure.Services.TranscriptImport;

public class PdfTranscriptImporter : ITranscriptImporter
{
    private readonly IGeminiVisionService _geminiVisionService;
    private readonly ILogger<PdfTranscriptImporter> _logger;

    public PdfTranscriptImporter(IGeminiVisionService geminiVisionService, ILogger<PdfTranscriptImporter> logger)
    {
        _geminiVisionService = geminiVisionService;
        _logger = logger;
    }

    public async Task<TranscriptImportResult> ImportAsync(Stream input, string? fileName = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importing PDF via Gemini 2.5 Pro Vision");
        return await _geminiVisionService.ExtractTranscriptAsync(input, fileName ?? "transcript.pdf", "application/pdf", cancellationToken);
    }
}
