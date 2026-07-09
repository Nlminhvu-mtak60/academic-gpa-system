using System;
using System.Collections.Generic;

namespace AcademicGPA.Application.Features.AiAdvisor.DTOs;

public record AiServiceGpaTrendItemDto(
    string SemesterName,
    decimal Gpa
);

public record AiServiceCourseItemDto(
    string CourseCode,
    string CourseName,
    decimal Score
);

public record AiServiceAcademicContextDto(
    decimal CurrentCumulativeGpa,
    int TotalCreditsCompleted,
    int TotalCreditsRequired,
    List<AiServiceGpaTrendItemDto> GpaTrend,
    List<AiServiceCourseItemDto> WeakCourses
);

public record AiServiceChatMessageDto(
    string Role,
    string Content
);

public record AiServiceResponseDto(
    string Response,
    int TokensUsed,
    string Provider
);

public record AiPredictResponseDto(
    decimal TargetScoreThreshold,
    decimal RequiredFinalExamScore,
    string Feasibility,
    string Advice
);

public record ConversationDto(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record ConversationMessageDto(
    string Role,
    string Content,
    DateTime CreatedAt
);

public record StartConversationRequest(
    string Title
);

public record SendMessageRequest(
    string Message
);
