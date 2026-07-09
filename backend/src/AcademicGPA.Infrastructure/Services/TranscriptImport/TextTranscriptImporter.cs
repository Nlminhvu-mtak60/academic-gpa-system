using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;

namespace AcademicGPA.Infrastructure.Services.TranscriptImport;

public class TextTranscriptImporter : ITranscriptImporter
{
    private readonly IEnumerable<IUniversityParser> _parsers;
    private readonly IUniversityDetector _detector;

    public TextTranscriptImporter(IEnumerable<IUniversityParser> parsers, IUniversityDetector detector)
    {
        _parsers = parsers;
        _detector = detector;
    }

    public async Task<TranscriptImportResult> ImportAsync(Stream input, string? fileName = null, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(input, leaveOpen: true);
        var text = await reader.ReadToEndAsync(cancellationToken);

        return ParseText(text);
    }

    public TranscriptImportResult ParseText(string text)
    {
        var university = _detector.DetectUniversity(text);
        
        var parser = _parsers.FirstOrDefault(p => p.SupportedUniversity == university) 
            ?? _parsers.First(p => p.SupportedUniversity == "Generic");

        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        
        var courses = parser.ParseLines(lines, out var warnings);

        return new TranscriptImportResult(courses, university, "Text", warnings);
    }
}
