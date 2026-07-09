using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Students.Commands.UpdatePreferences;

public record UpdateUserPreferencesCommand(
    string PreferredLanguage,
    string PreferredTheme
) : IRequest<Unit>;

public class UpdateUserPreferencesCommandValidator : AbstractValidator<UpdateUserPreferencesCommand>
{
    public UpdateUserPreferencesCommandValidator()
    {
        RuleFor(x => x.PreferredLanguage)
            .NotEmpty().WithMessage("Preferred language is required.")
            .Must(lang => lang == "vi" || lang == "en").WithMessage("Language must be 'vi' or 'en'.");

        RuleFor(x => x.PreferredTheme)
            .NotEmpty().WithMessage("Preferred theme is required.")
            .Must(theme => theme == "light" || theme == "dark").WithMessage("Theme must be 'light' or 'dark'.");
    }
}

public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserPreferencesCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UpdateUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch current User ID
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // 2. Fetch User from DB
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        // 3. Update Visual Preferences
        user.PreferredLanguage = request.PreferredLanguage;
        user.PreferredTheme = request.PreferredTheme;
        user.UpdatedAt = DateTime.UtcNow;

        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
