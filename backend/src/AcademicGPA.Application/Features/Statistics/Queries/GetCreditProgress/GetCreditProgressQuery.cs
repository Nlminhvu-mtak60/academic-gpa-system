using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Statistics.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Statistics.Queries.GetCreditProgress;

/// <summary>
/// Query to retrieve credit completion progress statistics.
/// </summary>
public record GetCreditProgressQuery : IRequest<CreditProgressDto>;

public class GetCreditProgressQueryHandler : IRequestHandler<GetCreditProgressQuery, CreditProgressDto>
{
    private readonly IStatisticsService _statisticsService;
    private readonly ICurrentUserService _currentUserService;

    public GetCreditProgressQueryHandler(
        IStatisticsService statisticsService,
        ICurrentUserService currentUserService)
    {
        _statisticsService = statisticsService;
        _currentUserService = currentUserService;
    }

    public async Task<CreditProgressDto> Handle(GetCreditProgressQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        return await _statisticsService.GetCreditProgressAsync(userId, cancellationToken);
    }
}
