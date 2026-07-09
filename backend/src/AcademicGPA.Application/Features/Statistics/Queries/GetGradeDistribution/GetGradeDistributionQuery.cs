using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Statistics.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Statistics.Queries.GetGradeDistribution;

/// <summary>
/// Query to retrieve total counts of earned letter grades.
/// </summary>
public record GetGradeDistributionQuery : IRequest<GradeDistributionDto>;

public class GetGradeDistributionQueryHandler : IRequestHandler<GetGradeDistributionQuery, GradeDistributionDto>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ICurrentUserService _currentUserService;

    public GetGradeDistributionQueryHandler(
        IStatisticsService statisticsService,
        ICurrentUserService currentUserService)
    {
        _statisticsService = statisticsService;
        _currentUserService = currentUserService;
    }

    public async Task<GradeDistributionDto> Handle(GetGradeDistributionQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return await _statisticsService.GetGradeDistributionAsync(userId, cancellationToken);
    }
}
