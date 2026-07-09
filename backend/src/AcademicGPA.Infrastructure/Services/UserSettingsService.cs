using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Settings.DTOs;
using AcademicGPA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Infrastructure.Services;

/// <summary>
/// Implements UserSettings CRUD and email update operations.
/// </summary>
public class UserSettingsService : IUserSettingsService
{
    private readonly IApplicationDbContext _context;

    public UserSettingsService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<UserSettingsDto> GetSettingsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId, cancellationToken);

        if (settings == null)
        {
            // Load the user to copy PreferredLanguage/PreferredTheme if they exist on the User entity
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException("User", userId);
            }

            settings = new UserSettings
            {
                UserId = userId,
                PreferredLanguage = user.PreferredLanguage,
                PreferredTheme = user.PreferredTheme,
                ReceiveSystem = true,
                ReceiveAcademic = true,
                ReceiveGoal = true,
                ReceiveGpaMilestone = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new UserSettingsDto(
            settings.PreferredLanguage,
            settings.PreferredTheme,
            settings.ReceiveSystem,
            settings.ReceiveAcademic,
            settings.ReceiveGoal,
            settings.ReceiveGpaMilestone
        );
    }

    /// <inheritdoc />
    public async Task UpdateSettingsAsync(Guid userId, UserSettingsDto settingsDto, CancellationToken cancellationToken)
    {
        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(us => us.UserId == userId, cancellationToken);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        if (settings == null)
        {
            settings = new UserSettings
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.UserSettings.Add(settings);
        }

        // Update settings
        settings.PreferredLanguage = settingsDto.PreferredLanguage;
        settings.PreferredTheme = settingsDto.PreferredTheme;
        settings.ReceiveSystem = settingsDto.ReceiveSystem;
        settings.ReceiveAcademic = settingsDto.ReceiveAcademic;
        settings.ReceiveGoal = settingsDto.ReceiveGoal;
        settings.ReceiveGpaMilestone = settingsDto.ReceiveGpaMilestone;
        settings.UpdatedAt = DateTime.UtcNow;

        // Sync visual settings back to User entity for compatibility
        user.PreferredLanguage = settingsDto.PreferredLanguage;
        user.PreferredTheme = settingsDto.PreferredTheme;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateEmailAsync(Guid userId, string newEmail, CancellationToken cancellationToken)
    {
        // 1. Verify email uniqueness
        var emailExists = await _context.Users
            .AnyAsync(u => u.Id != userId && u.Email.ToLower() == newEmail.ToLower(), cancellationToken);

        if (emailExists)
        {
            throw new ValidationException("Email", "Email is already in use by another user.");
        }

        // 2. Fetch current User
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        // 3. Update Email
        user.Email = newEmail;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
