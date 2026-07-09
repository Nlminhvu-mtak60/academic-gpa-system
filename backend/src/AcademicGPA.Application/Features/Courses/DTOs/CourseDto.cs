using System;
using AcademicGPA.Application.Features.Scores.DTOs;

namespace AcademicGPA.Application.Features.Courses.DTOs;

public record CourseDto(
    Guid Id,
    string CourseCode,
    string CourseName,
    int Credits,
    bool IsRetake,
    Guid? OriginalCourseId,
    ScoreDto? Score = null
);
