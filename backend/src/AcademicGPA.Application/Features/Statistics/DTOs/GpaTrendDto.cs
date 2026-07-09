using System;

namespace AcademicGPA.Application.Features.Statistics.DTOs;

/// <summary>
/// DTO representing chronological GPA trends by semester.
/// </summary>
public record GpaTrendDto(
    Guid SemesterId,
    string SemesterName,
    string YearName,
    decimal? Gpa10,
    decimal? Gpa4,
    decimal? CumulativeGpa10,
    decimal? CumulativeGpa4
);
