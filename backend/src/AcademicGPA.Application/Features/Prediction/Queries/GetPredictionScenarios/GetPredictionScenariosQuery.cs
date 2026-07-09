using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Prediction.DTOs;
using FluentValidation;
using MediatR;

namespace AcademicGPA.Application.Features.Prediction.Queries.GetPredictionScenarios;

/// <summary>
/// Query to retrieve required Final Exam scores for all possible passing letter grades.
/// </summary>
public record GetPredictionScenariosQuery(
    decimal AttendanceScore,
    decimal ContinuousScore
) : IRequest<List<ScenarioPredictionDto>>;

/// <summary>
/// Validator for GetPredictionScenariosQuery.
/// </summary>
public class GetPredictionScenariosQueryValidator : AbstractValidator<GetPredictionScenariosQuery>
{
    public GetPredictionScenariosQueryValidator()
    {
        RuleFor(x => x.AttendanceScore)
            .InclusiveBetween(0.0m, 10.0m)
            .WithMessage("Attendance score must be between 0.0 and 10.0.");

        RuleFor(x => x.ContinuousScore)
            .InclusiveBetween(0.0m, 10.0m)
            .WithMessage("Continuous assessment score must be between 0.0 and 10.0.");
    }
}

/// <summary>
/// Handler for GetPredictionScenariosQuery.
/// </summary>
public class GetPredictionScenariosQueryHandler : IRequestHandler<GetPredictionScenariosQuery, List<ScenarioPredictionDto>>
{
    private readonly IPredictionService _predictionService;

    public GetPredictionScenariosQueryHandler(IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    public Task<List<ScenarioPredictionDto>> Handle(GetPredictionScenariosQuery request, CancellationToken cancellationToken)
    {
        var result = _predictionService.PredictAllScenarios(request.AttendanceScore, request.ContinuousScore);
        return Task.FromResult(result);
    }
}
