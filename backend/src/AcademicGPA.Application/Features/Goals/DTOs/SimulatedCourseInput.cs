using System;

namespace AcademicGPA.Application.Features.Goals.DTOs;

/// <summary>
/// Input for a simulated course score override in what-if analysis.
/// </summary>
public record SimulatedCourseInput(
    Guid CourseId,
    decimal AttendanceScore,
    decimal ContinuousScore,
    decimal FinalExamScore
);
