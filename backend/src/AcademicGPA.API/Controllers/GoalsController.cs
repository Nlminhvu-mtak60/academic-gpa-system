using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Goals.Commands.SetGoal;
using AcademicGPA.Application.Features.Goals.Commands.SimulateScenario;
using AcademicGPA.Application.Features.Goals.Queries.GetGoals;
using AcademicGPA.Application.Features.Goals.Queries.GetRequiredGpa;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

/// <summary>
/// Endpoints for Goal Planner: managing GPA goals, required-GPA analysis, and scenario simulation.
/// </summary>
[ApiController]
[Route("api/v1/goals")]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public GoalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves the student's active and historical academic goals.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGoals()
    {
        var result = await _mediator.Send(new GetGoalsQuery());
        return Ok(new ApiResponse(true, result));
    }

    /// <summary>
    /// Sets a new target cumulative GPA goal. Replaces any previous active goal.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SetGoal([FromBody] SetGoalCommand command)
    {
        var result = await _mediator.Send(command);
        return StatusCode(201, new ApiResponse(true, result, Message: "Goal set successfully."));
    }

    /// <summary>
    /// Calculates the average GPA required in remaining credits to achieve the active goal.
    /// </summary>
    [HttpGet("required-gpa")]
    public async Task<IActionResult> GetRequiredGpa()
    {
        var result = await _mediator.Send(new GetRequiredGpaQuery());
        return Ok(new ApiResponse(true, result));
    }

    /// <summary>
    /// Simulates a "what-if" scenario for the current semester without saving changes.
    /// </summary>
    [HttpPost("simulate")]
    public async Task<IActionResult> SimulateScenario([FromBody] SimulateScenarioCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new ApiResponse(true, result));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
