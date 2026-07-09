using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Prediction.Commands.PredictFinalScore;
using AcademicGPA.Application.Features.Prediction.Queries.GetPredictionScenarios;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicGPA.API.Controllers;

/// <summary>
/// Endpoints for Final Exam score prediction calculations.
/// </summary>
[ApiController]
[Route("api/v1/prediction")]
[Authorize]
public class PredictionController : ControllerBase
{
    private readonly IMediator _mediator;

    public PredictionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Predicts the required Final Exam score for a specific target grade.
    /// </summary>
    [HttpPost("final-score")]
    public async Task<IActionResult> PredictFinalScore([FromBody] PredictFinalScoreCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new ApiResponse(true, result));
    }

    /// <summary>
    /// Returns required Final Exam scores for all possible passing letter grades.
    /// </summary>
    [HttpPost("scenarios")]
    public async Task<IActionResult> GetPredictionScenarios([FromBody] GetPredictionScenariosQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(new ApiResponse(true, result));
    }

    private record ApiResponse(bool Success, object? Data, object? Errors = null, string? Message = null)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
