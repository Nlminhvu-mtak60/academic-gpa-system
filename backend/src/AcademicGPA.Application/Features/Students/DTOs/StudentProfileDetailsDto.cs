namespace AcademicGPA.Application.Features.Students.DTOs;

public record StudentProfileSubDto(
    string StudentCode,
    string UniversityName,
    string MajorName,
    int EnrollmentYear,
    int TotalRequiredCredits
);

public record StudentProfileDetailsDto(
    string Email,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string PreferredLanguage,
    string PreferredTheme,
    StudentProfileSubDto? Profile
);
