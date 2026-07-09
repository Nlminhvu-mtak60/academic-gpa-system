using System.Collections.Generic;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;

namespace AcademicGPA.Application.Features.TranscriptImport.Interfaces;

public interface IUniversityParser
{
    string SupportedUniversity { get; }
    
    /// <summary>
    /// Parses raw lines of text into structured courses.
    /// </summary>
    List<ImportedCourseDto> ParseLines(IEnumerable<string> lines, out List<string> warnings);
    
    /// <summary>
    /// Parses structured rows (e.g. from Excel) into structured courses.
    /// </summary>
    List<ImportedCourseDto> ParseRows(IEnumerable<Dictionary<string, string>> rows, out List<string> warnings);
}
