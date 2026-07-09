using System.Collections.Generic;

namespace AcademicGPA.Application.Features.TranscriptImport.Interfaces;

public interface IUniversityDetector
{
    string DetectUniversity(string transcriptText);
    string DetectUniversityFromHeaders(IEnumerable<string> headers);
}
