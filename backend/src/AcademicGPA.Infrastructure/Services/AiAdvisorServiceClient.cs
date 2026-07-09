using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.AiAdvisor.DTOs;
using Microsoft.Extensions.Logging;

namespace AcademicGPA.Infrastructure.Services;

public class AiAdvisorServiceClient : IAiAdvisorServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiAdvisorServiceClient> _logger;

    public AiAdvisorServiceClient(HttpClient httpClient, ILogger<AiAdvisorServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AiServiceResponseDto> GetChatResponseAsync(
        string message, 
        string language, 
        AiServiceAcademicContextDto context, 
        IEnumerable<AiServiceChatMessageDto> history, 
        CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                message,
                preferredLanguage = language,
                academicContext = context,
                chatHistory = history
            };

            var response = await _httpClient.PostAsJsonAsync("/ai/advisor/chat", requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI Service Chat failed with status code {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"AI Service Chat failed with status code {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<AiServiceResponseDto>(cancellationToken: cancellationToken);
            return result ?? new AiServiceResponseDto("Error parsing response", 0, "Unknown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during AI Service Chat call");
            throw;
        }
    }

    public async Task<AiPredictResponseDto> GetPredictResponseAsync(
        decimal attendanceScore, 
        decimal continuousScore, 
        string targetGrade, 
        CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                attendanceScore = (double)attendanceScore,
                continuousScore = (double)continuousScore,
                targetGrade
            };

            var response = await _httpClient.PostAsJsonAsync("/ai/predict/final-score", requestBody, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AI Service Predict failed with status code {StatusCode}", response.StatusCode);
                throw new HttpRequestException($"AI Service Predict failed with status code {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<AiPredictResponseDto>(cancellationToken: cancellationToken);
            return result ?? new AiPredictResponseDto(0.0m, 0.0m, "Error", "Error parsing response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during AI Service Predict call");
            throw;
        }
    }
}
