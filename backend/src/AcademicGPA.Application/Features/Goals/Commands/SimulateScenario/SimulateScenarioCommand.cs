using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Goals.DTOs;
using FluentValidation;
using MediatR;

namespace AcademicGPA.Application.Features.Goals.Commands.SimulateScenario;

/// <summary>
/// Command to run a "what-if" scenario simulation without persisting changes.
/// </summary>
public record SimulateScenarioCommand(
    List<SimulatedCourseInput> SimulatedCourses
) : IRequest<SimulationResultDto>;

/// <summary>
/// Validator for SimulateScenarioCommand.
/// </summary>
public class SimulateScenarioCommandValidator : AbstractValidator<SimulateScenarioCommand>
{
    public SimulateScenarioCommandValidator()
    {
        RuleFor(x => x.SimulatedCourses)
            .NotEmpty()
            .WithMessage("At least one simulated course is required.");

        RuleForEach(x => x.SimulatedCourses).ChildRules(course =>
        {
            course.RuleFor(c => c.CourseId)
                .NotEmpty()
                .WithMessage("Course ID is required.");

            course.RuleFor(c => c.AttendanceScore)
                .InclusiveBetween(0.0m, 10.0m)
                .WithMessage("Attendance score must be between 0.0 and 10.0.");

            course.RuleFor(c => c.ContinuousScore)
                .InclusiveBetween(0.0m, 10.0m)
                .WithMessage("Continuous assessment score must be between 0.0 and 10.0.");

            course.RuleFor(c => c.FinalExamScore)
                .InclusiveBetween(0.0m, 10.0m)
                .WithMessage("Final exam score must be between 0.0 and 10.0.");
        });
    }
}

/// <summary>
/// Handler for SimulateScenarioCommand.
/// </summary>
public class SimulateScenarioCommandHandler : IRequestHandler<SimulateScenarioCommand, SimulationResultDto>
{
    private readonly IGoalPlannerService _goalPlannerService;
    private readonly ICurrentUserService _currentUserService;

    public SimulateScenarioCommandHandler(IGoalPlannerService goalPlannerService, ICurrentUserService currentUserService)
    {
        _goalPlannerService = goalPlannerService;
        _currentUserService = currentUserService;
    }

    public async Task<SimulationResultDto> Handle(SimulateScenarioCommand request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        return await _goalPlannerService.SimulateScenariosAsync(userId, request.SimulatedCourses, cancellationToken);
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
