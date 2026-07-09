using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using AcademicGPA.Application.Features.TranscriptImport.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AcademicGPA.Infrastructure.AI.Gemini;

public class GeminiVisionService : IGeminiVisionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiVisionService> _logger;

    public GeminiVisionService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiVisionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TranscriptImportResult> ExtractTranscriptAsync(Stream fileStream, string fileName, string mimeType, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        var model = _configuration["Gemini:Model"] ?? "gemini-2.5-pro";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("Gemini API Key is missing in configuration.");
            throw new InvalidOperationException("Gemini API Key is not configured.");
        }

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        var base64Data = Convert.ToBase64String(memoryStream.ToArray());

        var prompt = @"You are an academic transcript extraction system.

Your job:
Extract ONLY information that actually exists.
Never hallucinate.
Never invent courses.
Never generate fake scores.

If a value cannot be determined:
Return null.

Return JSON ONLY.

Schema:
{
  ""university"":"""",
  ""academicYear"":"""",
  ""semester"":"""",
  ""courses"":[
    {
      ""courseName"":"""",
      ""credits"":0,
      ""finalScore"":null,
      ""componentScores"":{
        ""CC"":null,
        ""TX"":null,
        ""Thi"":null,
        ""GK"":null,
        ""CK"":null
      },
      ""confidence"":95
    }
  ]
}

Rules:
1. Never fabricate data.
2. If confidence < 80:
Mark confidence accordingly.
3. If no transcript exists:
Return:
{
  ""courses"":[]
}
4. If image quality is poor:
Return:
{
  ""error"":""Could not confidently recognize transcript""
}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = mimeType,
                                data = base64Data
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json"
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        try
        {
            var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseGeminiResponse(responseJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with Gemini API.");
            return new TranscriptImportResult(
                new List<ImportedCourseDto>(),
                "Unknown",
                "Gemini",
                new List<string> { "Failed to communicate with AI service: " + ex.Message }
            );
        }
    }

    private TranscriptImportResult ParseGeminiResponse(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return CreateErrorResult("No response candidates from Gemini.");
            }

            var content = candidates[0].GetProperty("content");
            var parts = content.GetProperty("parts");
            var text = parts[0].GetProperty("text").GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                return CreateErrorResult("Empty response from Gemini.");
            }

            text = text.Trim();
            if (text.StartsWith("```json"))
            {
                text = text.Substring(7);
            }
            if (text.EndsWith("```"))
            {
                text = text.Substring(0, text.Length - 3);
            }

            var extractionResult = JsonSerializer.Deserialize<GeminiExtractionResult>(text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (extractionResult == null)
            {
                return CreateErrorResult("Failed to deserialize Gemini output.");
            }

            if (!string.IsNullOrWhiteSpace(extractionResult.Error))
            {
                return CreateErrorResult(extractionResult.Error);
            }

            var courses = new List<ImportedCourseDto>();
            if (extractionResult.Courses != null)
            {
                foreach (var c in extractionResult.Courses)
                {
                    var compScores = new Dictionary<string, decimal>();
                    if (c.ComponentScores != null)
                    {
                        foreach (var kvp in c.ComponentScores)
                        {
                            if (kvp.Value.HasValue)
                            {
                                compScores[kvp.Key] = kvp.Value.Value;
                            }
                        }
                    }

                    courses.Add(new ImportedCourseDto(
                        c.CourseName ?? "Unknown Course",
                        c.Credits ?? 0,
                        c.FinalScore,
                        compScores,
                        c.Confidence ?? 100
                    ));
                }
            }

            return new TranscriptImportResult(
                courses,
                extractionResult.University ?? "Unknown",
                "Gemini",
                new List<string>()
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response.");
            return CreateErrorResult("Failed to parse the transcript structure.");
        }
    }

    private TranscriptImportResult CreateErrorResult(string message)
    {
        return new TranscriptImportResult(
            new List<ImportedCourseDto>(),
            "Unknown",
            "Gemini",
            new List<string> { message }
        );
    }

    private class GeminiExtractionResult
    {
        public string? University { get; set; }
        public string? AcademicYear { get; set; }
        public string? Semester { get; set; }
        public string? Error { get; set; }
        public List<GeminiCourse>? Courses { get; set; }
    }

    private class GeminiCourse
    {
        public string? CourseName { get; set; }
        public int? Credits { get; set; }
        public decimal? FinalScore { get; set; }
        public Dictionary<string, decimal?>? ComponentScores { get; set; }
        public double? Confidence { get; set; }
    }
}
