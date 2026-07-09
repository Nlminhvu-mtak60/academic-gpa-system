using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicGPA.Application.Features.TranscriptImport.Commands.ParseTranscript;

public record ParseTranscriptCommand(
    Guid SemesterId,
    Stream InputStream,
    string SourceType,
    string? FileName
) : IRequest<TranscriptImportResult>;

public class ParseTranscriptCommandHandler : IRequestHandler<ParseTranscriptCommand, TranscriptImportResult>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IUniversityDetector _universityDetector;

    public ParseTranscriptCommandHandler(IServiceProvider serviceProvider, IUniversityDetector universityDetector)
    {
        _serviceProvider = serviceProvider;
        _universityDetector = universityDetector;
    }

    public async Task<TranscriptImportResult> Handle(ParseTranscriptCommand request, CancellationToken cancellationToken)
    {
        // Resolve importer based on source type
        ITranscriptImporter importer = request.SourceType.ToLowerInvariant() switch
        {
            "excel" => _serviceProvider.GetRequiredKeyedService<ITranscriptImporter>("Excel"),
            "pdf" => _serviceProvider.GetRequiredKeyedService<ITranscriptImporter>("Pdf"),
            "imageocr" => _serviceProvider.GetRequiredKeyedService<ITranscriptImporter>("ImageOcr"),
            "text" => _serviceProvider.GetRequiredKeyedService<ITranscriptImporter>("Text"),
            _ => throw new ArgumentException($"Unsupported source type: {request.SourceType}")
        };

        var result = await importer.ImportAsync(request.InputStream, request.FileName, cancellationToken);
        
        // Map raw university name to known constant if possible
        if (!string.IsNullOrWhiteSpace(result.DetectedUniversity) && result.DetectedUniversity != "Unknown")
        {
            var mappedUni = _universityDetector.DetectUniversity(result.DetectedUniversity);
            if (mappedUni != "Generic")
            {
                result = result with { DetectedUniversity = mappedUni };
            }
        }
        
        // This is just a preview. It does NOT save to the database.
        return result;
    }
}
