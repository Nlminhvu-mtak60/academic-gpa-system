using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Goals.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Goals.Queries.GetRequiredGpa;

/// <summary>
/// Query to calculate the required GPA in remaining credits to achieve the active goal.
/// </summary>
public record GetRequiredGpaQuery : IRequest<RequiredGpaDto>;

/// <summary>
/// Handler for GetRequiredGpaQuery.
/// </summary>
public class GetRequiredGpaQueryHandler : IRequestHandler<GetRequiredGpaQuery, RequiredGpaDto>
{
    private readonly IGoalPlannerService _goalPlannerService;
    private readonly ICurrentUserService _currentUserService;

    public GetRequiredGpaQueryHandler(IGoalPlannerService goalPlannerService, ICurrentUserService currentUserService)
    {
        _goalPlannerService = goalPlannerService;
        _currentUserService = currentUserService;
    }

    public async Task<RequiredGpaDto> Handle(GetRequiredGpaQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        return await _goalPlannerService.GetRequiredGpaAsync(userId, cancellationToken);
    }

    private Guid GetUserId()
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId;
    }
}
