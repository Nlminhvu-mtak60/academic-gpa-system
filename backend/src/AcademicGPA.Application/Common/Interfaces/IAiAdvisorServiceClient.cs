using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.AiAdvisor.DTOs;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service interface defining HTTP client contracts for interacting with the Python FastAPI AI Advisor.
/// </summary>
public interface IAiAdvisorServiceClient
{
    /// <summary>
    /// Sends chat logs, student context, and message history to the AI Advisor service.
    /// </summary>
    Task<AiServiceResponseDto> GetChatResponseAsync(
        string message, 
        string language, 
        AiServiceAcademicContextDto context, 
        IEnumerable<AiServiceChatMessageDto> history, 
        CancellationToken cancellationToken);

    /// <summary>
    /// Evaluates target Final Exam score requirements using the prediction engine.
    /// </summary>
    Task<AiPredictResponseDto> GetPredictResponseAsync(
        decimal attendanceScore, 
        decimal continuousScore, 
        string targetGrade, 
        CancellationToken cancellationToken);
}
