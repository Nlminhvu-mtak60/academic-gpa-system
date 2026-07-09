using System;

namespace AcademicGPA.Application.Features.Semesters.DTOs;

public record SemesterDto(
    Guid Id,
    string SemesterName,
    int SortOrder,
    int CompletedCredits,
    decimal SemesterGpa10,
    decimal SemesterGpa4,
    bool IsImported = false,
    int ImportedCredits = 0,
    decimal ImportedGpa10 = 0.00m,
    decimal ImportedGpa4 = 0.00m
);
