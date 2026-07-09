using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Statistics.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Statistics.Queries.GetGpaTrend;

/// <summary>
/// Query to retrieve GPA metrics organized chronologically by semester.
/// </summary>
public record GetGpaTrendQuery : IRequest<IReadOnlyList<GpaTrendDto>>;

public class GetGpaTrendQueryHandler : IRequestHandler<GetGpaTrendQuery, IReadOnlyList<GpaTrendDto>>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ICurrentUserService _currentUserService;

    public GetGpaTrendQueryHandler(
        IStatisticsService statisticsService,
        ICurrentUserService currentUserService)
    {
        _statisticsService = statisticsService;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<GpaTrendDto>> Handle(GetGpaTrendQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return await _statisticsService.GetGpaTrendAsync(userId, cancellationToken);
    }
}
