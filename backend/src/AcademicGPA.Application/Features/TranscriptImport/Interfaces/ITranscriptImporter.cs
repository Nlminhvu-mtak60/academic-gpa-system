using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;

namespace AcademicGPA.Application.Features.TranscriptImport.Interfaces;

public interface ITranscriptImporter
{
    Task<TranscriptImportResult> ImportAsync(Stream input, string? fileName = null, CancellationToken cancellationToken = default);
}
