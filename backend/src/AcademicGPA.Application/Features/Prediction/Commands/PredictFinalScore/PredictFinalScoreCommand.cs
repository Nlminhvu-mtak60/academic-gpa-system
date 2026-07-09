using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Prediction.DTOs;
using FluentValidation;
using MediatR;

namespace AcademicGPA.Application.Features.Prediction.Commands.PredictFinalScore;

/// <summary>
/// Command to predict the required Final Exam score for a target grade.
/// </summary>
public record PredictFinalScoreCommand(
    decimal AttendanceScore,
    decimal ContinuousScore,
    string TargetGrade
) : IRequest<FinalScorePredictionDto>;

/// <summary>
/// Validator for PredictFinalScoreCommand.
/// </summary>
public class PredictFinalScoreCommandValidator : AbstractValidator<PredictFinalScoreCommand>
{
    private static readonly string[] ValidGrades = { "A+", "A", "B+", "B", "C+", "C", "D+", "D" };

    public PredictFinalScoreCommandValidator()
    {
        RuleFor(x => x.AttendanceScore)
            .InclusiveBetween(0.0m, 10.0m)
            .WithMessage("Attendance score must be between 0.0 and 10.0.");

        RuleFor(x => x.ContinuousScore)
            .InclusiveBetween(0.0m, 10.0m)
            .WithMessage("Continuous assessment score must be between 0.0 and 10.0.");

        RuleFor(x => x.TargetGrade)
            .NotEmpty()
            .WithMessage("Target grade is required.")
            .Must(grade => ValidGrades.Contains(grade))
            .WithMessage("Target grade must be one of: A+, A, B+, B, C+, C, D+, D.");
    }
}

/// <summary>
/// Handler for PredictFinalScoreCommand.
/// </summary>
public class PredictFinalScoreCommandHandler : IRequestHandler<PredictFinalScoreCommand, FinalScorePredictionDto>
{
    private readonly IPredictionService _predictionService;

    public PredictFinalScoreCommandHandler(IPredictionService predictionService)
    {
        _predictionService = predictionService;
    }

    public Task<FinalScorePredictionDto> Handle(PredictFinalScoreCommand request, CancellationToken cancellationToken)
    {
        var result = _predictionService.PredictFinalScore(
            request.AttendanceScore,
            request.ContinuousScore,
            request.TargetGrade);

        return Task.FromResult(result);
    }
}
