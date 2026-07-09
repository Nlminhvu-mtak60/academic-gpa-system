using System.Collections.Generic;

namespace AcademicGPA.Application.Features.Statistics.DTOs;

/// <summary>
/// DTO representing details of a course in strength/weakness analysis.
/// </summary>
public record StrengthWeaknessCourseDto(
    string CourseCode,
    string CourseName,
    decimal Score,
    string LetterGrade
);

/// <summary>
/// DTO representing the student's strongest and weakest courses.
/// </summary>
public record StrengthsWeaknessesDto(
    IReadOnlyList<StrengthWeaknessCourseDto> StrongestCourses,
    IReadOnlyList<StrengthWeaknessCourseDto> WeakestCourses
);
