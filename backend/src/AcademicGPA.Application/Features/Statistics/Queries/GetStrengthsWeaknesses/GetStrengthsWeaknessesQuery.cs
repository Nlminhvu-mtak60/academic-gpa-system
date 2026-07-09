using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Statistics.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Statistics.Queries.GetStrengthsWeaknesses;

/// <summary>
/// Query to analyze student strengths and weaknesses by course score.
/// </summary>
public record GetStrengthsWeaknessesQuery : IRequest<StrengthsWeaknessesDto>;

public class GetStrengthsWeaknessesQueryHandler : IRequestHandler<GetStrengthsWeaknessesQuery, StrengthsWeaknessesDto>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ICurrentUserService _currentUserService;

    public GetStrengthsWeaknessesQueryHandler(
        IStatisticsService statisticsService,
        ICurrentUserService currentUserService)
    {
        _statisticsService = statisticsService;
        _currentUserService = currentUserService;
    }

    public async Task<StrengthsWeaknessesDto> Handle(GetStrengthsWeaknessesQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return await _statisticsService.GetStrengthsWeaknessesAsync(userId, cancellationToken);
    }
}
