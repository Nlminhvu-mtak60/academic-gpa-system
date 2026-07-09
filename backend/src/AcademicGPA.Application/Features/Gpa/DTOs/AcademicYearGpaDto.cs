using System;

namespace AcademicGPA.Application.Features.Gpa.DTOs;

public record AcademicYearGpaDto(
    Guid AcademicYearId,
    decimal? Gpa10,
    decimal? Gpa4,
    int TotalCredits,
    int PassedCredits,
    int FailedCredits
);
