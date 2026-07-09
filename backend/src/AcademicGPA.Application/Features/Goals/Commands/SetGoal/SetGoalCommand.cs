using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Goals.DTOs;
using FluentValidation;
using MediatR;

namespace AcademicGPA.Application.Features.Goals.Commands.SetGoal;

/// <summary>
/// Command to set a new target cumulative GPA goal.
/// </summary>
public record SetGoalCommand(
    decimal TargetCumulativeGpa10,
    string? Notes
) : IRequest<GoalDto>;

/// <summary>
/// Validator for SetGoalCommand.
/// </summary>
public class SetGoalCommandValidator : AbstractValidator<SetGoalCommand>
{
    public SetGoalCommandValidator()
    {
        RuleFor(x => x.TargetCumulativeGpa10)
            .InclusiveBetween(0.00m, 10.00m)
            .WithMessage("Target GPA must be between 0.00 and 10.00.");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters.");
    }
}

/// <summary>
/// Handler for SetGoalCommand.
/// </summary>
public class SetGoalCommandHandler : IRequestHandler<SetGoalCommand, GoalDto>
{
    private readonly IGoalPlannerService _goalPlannerService;
    private readonly ICurrentUserService _currentUserService;

    public SetGoalCommandHandler(IGoalPlannerService goalPlannerService, ICurrentUserService currentUserService)
    {
        _goalPlannerService = goalPlannerService;
        _currentUserService = currentUserService;
    }

    public async Task<GoalDto> Handle(SetGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        return await _goalPlannerService.SetGoalAsync(userId, request.TargetCumulativeGpa10, request.Notes, cancellationToken);
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
