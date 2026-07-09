namespace AcademicGPA.Application.Features.Settings.DTOs;

/// <summary>
/// Data transfer object representing a user's visual preferences and notification options.
/// </summary>
public record UserSettingsDto(
    string PreferredLanguage,
    string PreferredTheme,
    bool ReceiveSystem,
    bool ReceiveAcademic,
    bool ReceiveGoal,
    bool ReceiveGpaMilestone
);
