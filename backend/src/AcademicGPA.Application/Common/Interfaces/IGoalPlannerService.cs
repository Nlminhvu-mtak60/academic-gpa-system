using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Goals.DTOs;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service contract for Goal Planner features: setting goals, computing required GPAs, and running simulations.
/// </summary>
public interface IGoalPlannerService
{
    /// <summary>
    /// Retrieves all goals for a student, ordered by creation date descending.
    /// </summary>
    Task<List<GoalDto>> GetGoalsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Sets a new active cumulative GPA goal, deactivating any previous active goal (BR-GOAL-002).
    /// </summary>
    Task<GoalDto> SetGoalAsync(Guid userId, decimal targetGpa10, string? notes, CancellationToken cancellationToken);

    /// <summary>
    /// Calculates the required average GPA on remaining credits to achieve the active goal (BR-GOAL-001).
    /// </summary>
    Task<RequiredGpaDto> GetRequiredGpaAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Simulates a "what-if" scenario by temporarily overriding course scores and recalculating GPAs.
    /// Does NOT persist any changes.
    /// </summary>
    Task<SimulationResultDto> SimulateScenariosAsync(Guid userId, List<SimulatedCourseInput> courses, CancellationToken cancellationToken);
}
