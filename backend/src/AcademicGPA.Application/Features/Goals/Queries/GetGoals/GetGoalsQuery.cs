using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Goals.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Goals.Queries.GetGoals;

/// <summary>
/// Query to retrieve all goals for the current student.
/// </summary>
public record GetGoalsQuery : IRequest<List<GoalDto>>;

/// <summary>
/// Handler for GetGoalsQuery.
/// </summary>
public class GetGoalsQueryHandler : IRequestHandler<GetGoalsQuery, List<GoalDto>>
{
    private readonly IGoalPlannerService _goalPlannerService;
    private readonly ICurrentUserService _currentUserService;

    public GetGoalsQueryHandler(IGoalPlannerService goalPlannerService, ICurrentUserService currentUserService)
    {
        _goalPlannerService = goalPlannerService;
        _currentUserService = currentUserService;
    }

    public async Task<List<GoalDto>> Handle(GetGoalsQuery request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        return await _goalPlannerService.GetGoalsAsync(userId, cancellationToken);
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
