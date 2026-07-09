namespace AcademicGPA.Application.Features.Statistics.DTOs;

/// <summary>
/// DTO representing the total count of earned letter grades.
/// </summary>
public record GradeDistributionDto(
    int Aplus,
    int A,
    int Bplus,
    int B,
    int Cplus,
    int C,
    int Dplus,
    int D,
    int F
);
