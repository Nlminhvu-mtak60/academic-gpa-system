using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Settings.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Settings.Queries;

public record GetUserSettingsQuery : IRequest<UserSettingsDto>;

public class GetUserSettingsQueryHandler : IRequestHandler<GetUserSettingsQuery, UserSettingsDto>
{
    private readonly IUserSettingsService _settingsService;
    private readonly ICurrentUserService _currentUserService;

    public GetUserSettingsQueryHandler(
        IUserSettingsService settingsService,
        ICurrentUserService currentUserService)
    {
        _settingsService = settingsService;
        _currentUserService = currentUserService;
    }

    public async Task<UserSettingsDto> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return await _settingsService.GetSettingsAsync(userId, cancellationToken);
    }
}
