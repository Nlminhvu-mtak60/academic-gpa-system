using System.Collections.Generic;

namespace AcademicGPA.Application.Features.TranscriptImport.DTOs;

public record ImportedCourseDto(
    string CourseName,
    int Credits,
    decimal? FinalScore,
    Dictionary<string, decimal> ComponentScores,
    double Confidence = 1.0
);
