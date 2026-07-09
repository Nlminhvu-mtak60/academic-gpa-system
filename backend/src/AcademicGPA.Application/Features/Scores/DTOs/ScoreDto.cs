using System;

namespace AcademicGPA.Application.Features.Scores.DTOs;

/// <summary>
/// Data transfer object representing the score details of a course.
/// </summary>
public record ScoreDto(
    decimal? AttendanceScore,
    decimal? ContinuousScore,
    decimal? FinalExamScore,
    decimal? CourseScore,
    string? LetterGrade,
    decimal? Gpa4Value,
    string? AcademicClassification,
    bool? IsPass,
    DateTime? CalculatedAt
);
