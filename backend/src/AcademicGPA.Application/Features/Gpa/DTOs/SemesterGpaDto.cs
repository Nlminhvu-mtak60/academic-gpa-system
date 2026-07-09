using System;

namespace AcademicGPA.Application.Features.Gpa.DTOs;

public record SemesterGpaDto(
    Guid SemesterId,
    decimal? Gpa10,
    decimal? Gpa4,
    int TotalCredits,
    int PassedCredits,
    int FailedCredits
);
