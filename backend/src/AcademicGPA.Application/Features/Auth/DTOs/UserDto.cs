namespace AcademicGPA.Application.Features.Auth.DTOs;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    bool IsEmailVerified,
    string? AvatarUrl,
    string PreferredLanguage,
    string PreferredTheme,
    bool ForcePasswordChange = false
);
