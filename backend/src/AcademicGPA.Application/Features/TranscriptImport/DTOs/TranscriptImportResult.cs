using System.Collections.Generic;

namespace AcademicGPA.Application.Features.TranscriptImport.DTOs;

public record TranscriptImportResult(
    List<ImportedCourseDto> Courses,
    string DetectedUniversity,
    string SourceType,
    List<string> Warnings
);
