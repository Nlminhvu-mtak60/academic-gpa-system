using System;

namespace AcademicGPA.Application.Features.Gpa.DTOs;

public record CumulativeGpaDto(
    decimal? CumulativeGpa10,
    decimal? CumulativeGpa4,
    int TotalCreditsCompleted,
    int TotalCreditsRequired,
    decimal CompletionPercentage
);
