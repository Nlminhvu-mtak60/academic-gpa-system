using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;

namespace AcademicGPA.Application.Features.TranscriptImport.Interfaces;

public interface IGeminiVisionService
{
    Task<TranscriptImportResult> ExtractTranscriptAsync(Stream fileStream, string fileName, string mimeType, CancellationToken cancellationToken);
}
