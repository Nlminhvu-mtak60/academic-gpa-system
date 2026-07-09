using System;

namespace AcademicGPA.Application.Features.AcademicYears.DTOs;

public record AcademicYearDto(
    Guid Id,
    string YearName,
    int StartYear,
    int EndYear,
    string Status,
    bool IsCurrent,
    int CompletedCredits,
    decimal YearGpa10,
    decimal YearGpa4,
    bool IsImported = false,
    int ImportedCredits = 0,
    decimal ImportedGpa10 = 0.00m,
    decimal ImportedGpa4 = 0.00m
);
