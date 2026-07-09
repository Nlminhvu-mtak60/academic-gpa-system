using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace AcademicGPA.Application.Features.Settings.Commands;

public record UpdateUserEmailCommand(string NewEmail) : IRequest;

public class UpdateUserEmailCommandValidator : AbstractValidator<UpdateUserEmailCommand>
{
    public UpdateUserEmailCommandValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

public class UpdateUserEmailCommandHandler : IRequestHandler<UpdateUserEmailCommand>
{
    private readonly IUserSettingsService _settingsService;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserEmailCommandHandler(
        IUserSettingsService settingsService,
        ICurrentUserService currentUserService)
    {
        _settingsService = settingsService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateUserEmailCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        await _settingsService.UpdateEmailAsync(userId, request.NewEmail, cancellationToken);
    }
}
