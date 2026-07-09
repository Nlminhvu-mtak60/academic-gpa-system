namespace AcademicGPA.Domain.ValueObjects;

/// <summary>
/// Encapsulates standard grading results translated from course 10-scale score.
/// </summary>
public record GradeResult(
    string LetterGrade,
    decimal Gpa4Value,
    string AcademicClassification,
    bool IsPass
);
