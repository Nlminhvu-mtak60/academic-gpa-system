using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Settings.DTOs;
using FluentValidation;
using MediatR;

namespace AcademicGPA.Application.Features.Settings.Commands;

public record UpdateUserSettingsCommand(
    string PreferredLanguage,
    string PreferredTheme,
    bool ReceiveSystem,
    bool ReceiveAcademic,
    bool ReceiveGoal,
    bool ReceiveGpaMilestone
) : IRequest;

public class UpdateUserSettingsCommandValidator : AbstractValidator<UpdateUserSettingsCommand>
{
    public UpdateUserSettingsCommandValidator()
    {
        RuleFor(x => x.PreferredLanguage)
            .NotEmpty().WithMessage("Preferred language is required.")
            .Must(lang => lang == "vi" || lang == "en").WithMessage("Language must be either 'vi' or 'en'.");

        RuleFor(x => x.PreferredTheme)
            .NotEmpty().WithMessage("Preferred theme is required.")
            .Must(theme => theme == "light" || theme == "dark" || theme == "system")
            .WithMessage("Theme must be 'light', 'dark', or 'system'.");
    }
}

public class UpdateUserSettingsCommandHandler : IRequestHandler<UpdateUserSettingsCommand>
{
    private readonly IUserSettingsService _settingsService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserSettingsCommandHandler(
        IUserSettingsService settingsService,
        ICurrentUserService currentUserService)
    {
        _settingsService = settingsService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateUserSettingsCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var dto = new UserSettingsDto(
            request.PreferredLanguage,
            request.PreferredTheme,
            request.ReceiveSystem,
            request.ReceiveAcademic,
            request.ReceiveGoal,
            request.ReceiveGpaMilestone
        );

        await _settingsService.UpdateSettingsAsync(userId, dto, cancellationToken);
    }
}
