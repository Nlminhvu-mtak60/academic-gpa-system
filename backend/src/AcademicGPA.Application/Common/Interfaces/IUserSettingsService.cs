using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Settings.DTOs;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service managing user visual preferences, language, notification preferences, and account settings.
/// </summary>
public interface IUserSettingsService
{
    /// <summary>
    /// Retrieves user settings, initializing with defaults if none exist in the database.
    /// </summary>
    Task<UserSettingsDto> GetSettingsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Updates user settings (theme, language, and notification toggles).
    /// </summary>
    Task UpdateSettingsAsync(Guid userId, UserSettingsDto settingsDto, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the user's login email address.
    /// </summary>
    Task UpdateEmailAsync(Guid userId, string newEmail, CancellationToken cancellationToken);
}
