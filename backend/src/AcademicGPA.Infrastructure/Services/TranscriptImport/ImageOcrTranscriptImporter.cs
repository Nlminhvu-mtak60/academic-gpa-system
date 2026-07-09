using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;
using Microsoft.Extensions.Logging;

namespace AcademicGPA.Infrastructure.Services.TranscriptImport;

public class ImageOcrTranscriptImporter : ITranscriptImporter
{
    private readonly IGeminiVisionService _geminiVisionService;
    private readonly ILogger<ImageOcrTranscriptImporter> _logger;

    public ImageOcrTranscriptImporter(IGeminiVisionService geminiVisionService, ILogger<ImageOcrTranscriptImporter> logger)
    {
        _geminiVisionService = geminiVisionService;
        _logger = logger;
    }

    public async Task<TranscriptImportResult> ImportAsync(Stream input, string? fileName = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importing Image via Gemini 2.5 Pro Vision");
        
        string extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? ".png";
        string mimeType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            _ => "image/png"
        };
        
        return await _geminiVisionService.ExtractTranscriptAsync(input, fileName ?? "image.png", mimeType, cancellationToken);
    }
}
