using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.AiAdvisor.DTOs;
using AcademicGPA.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.AiAdvisor;

public record StartConversationCommand(string Title) : IRequest<ConversationDto>;
public record DeleteConversationCommand(Guid ConversationId) : IRequest;
public record SendMessageCommand(Guid ConversationId, string Message) : IRequest<ConversationMessageDto>;

// Validators
public class StartConversationCommandValidator : AbstractValidator<StartConversationCommand>
{
    public StartConversationCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");
    }
}

public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(2000).WithMessage("Message cannot exceed 2000 characters.");
    }
}

// Handlers
public class StartConversationCommandHandler : IRequestHandler<StartConversationCommand, ConversationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StartConversationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ConversationDto> Handle(StartConversationCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // Check if user has exceeded 50 active threads limit
        var activeCount = await _context.Conversations
            .CountAsync(c => c.UserId == userId && !c.IsDeleted, cancellationToken);

        if (activeCount >= 50)
        {
            throw new UnprocessableEntityException("You have exceeded the maximum limit of 50 active conversations.");
        }

        var conversation = new Conversation
        {
            Title = request.Title,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync(cancellationToken);

        return new ConversationDto(conversation.Id, conversation.Title, conversation.CreatedAt, conversation.UpdatedAt);
    }
}

public class DeleteConversationCommandHandler : IRequestHandler<DeleteConversationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteConversationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var conversation = await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            throw new NotFoundException("Conversation", request.ConversationId);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenException("You are not authorized to delete this conversation.");
        }

        // Soft delete
        conversation.IsDeleted = true;
        conversation.UpdatedAt = DateTime.UtcNow;

        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, ConversationMessageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAiAdvisorServiceClient _aiClient;
    private readonly IGpaCalculator _gpaCalculator;

    public SendMessageCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAiAdvisorServiceClient aiClient,
        IGpaCalculator gpaCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _aiClient = aiClient;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<ConversationMessageDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var conversation = await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && !c.IsDeleted, cancellationToken);

        if (conversation == null)
        {
            throw new NotFoundException("Conversation", request.ConversationId);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenException("You are not authorized to post messages to this conversation.");
        }

        // Enforce hourly rate limit of 20 messages per user
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        var msgCount = await _context.ConversationMessages
            .CountAsync(m => m.Conversation.UserId == userId && m.Role == "user" && m.CreatedAt >= oneHourAgo, cancellationToken);

        if (msgCount >= 20)
        {
            throw new RateLimitException("You have exceeded the rate limit of 20 messages per hour.");
        }

        // Fetch User Preferred Language
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        var preferredLang = user?.PreferredLanguage ?? "vi";

        // Aggregate student academic history context
        var profile = await _context.StudentProfiles
            .Include(sp => sp.AcademicYears.Where(ay => !ay.IsDeleted))
                .ThenInclude(ay => ay.Semesters.Where(s => !s.IsDeleted))
                    .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                        .ThenInclude(c => c.Score)
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        decimal currentGpa = 0.00m;
        int completedCredits = 0;
        int requiredCredits = 120;
        var trendItems = new List<AiServiceGpaTrendItemDto>();
        var weakCourses = new List<AiServiceCourseItemDto>();

        if (profile != null)
        {
            requiredCredits = profile.TotalRequiredCredits;

            var allCourses = profile.AcademicYears
                .SelectMany(ay => ay.Semesters)
                .SelectMany(s => s.Courses)
                .ToList();

            var bestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();
            currentGpa = _gpaCalculator.CalculateGpa10(bestAttempts) ?? 0.00m;
            completedCredits = bestAttempts
                .Where(c => c.Score != null && c.Score.IsPass == true)
                .Sum(c => c.Credits);

            // Compute GPA Trend chronologically
            var sortedSemesters = profile.AcademicYears
                .OrderBy(ay => ay.StartYear)
                .SelectMany(ay => ay.Semesters)
                .OrderBy(s => s.SortOrder)
                .ToList();

            foreach (var sem in sortedSemesters)
            {
                var semCourses = sem.Courses.Where(c => !c.IsDeleted).ToList();
                var semGpa = _gpaCalculator.CalculateGpa10(semCourses);
                if (semGpa.HasValue)
                {
                    trendItems.Add(new AiServiceGpaTrendItemDto(sem.SemesterName, semGpa.Value));
                }
            }

            // Select weak courses (e.g. courseScore < 7.0)
            weakCourses = bestAttempts
                .Where(c => c.Score != null && c.Score.CourseScore.HasValue && c.Score.CourseScore.Value < 7.0m)
                .Select(c => new AiServiceCourseItemDto(c.CourseCode, c.CourseName, c.Score!.CourseScore!.Value))
                .ToList();
        }

        var contextDto = new AiServiceAcademicContextDto(
            currentGpa,
            completedCredits,
            requiredCredits,
            trendItems,
            weakCourses
        );

        // Fetch recent messages for history mapping (anonymized)
        var historyDtos = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .TakeLast(10) // Only send recent 10 messages context for performance
            .Select(m => new AiServiceChatMessageDto(m.Role, m.Content))
            .ToList();

        // Call Python FastAPI microservice
        var responseDto = await _aiClient.GetChatResponseAsync(
            request.Message,
            preferredLang,
            contextDto,
            historyDtos,
            cancellationToken
        );

        // Save User Message
        var userMsg = new ConversationMessage
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        };
        _context.ConversationMessages.Add(userMsg);

        // Save Assistant Message
        var assistantMsg = new ConversationMessage
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = responseDto.Response,
            CreatedAt = DateTime.UtcNow.AddMilliseconds(50) // slightly offset to ensure order
        };
        _context.ConversationMessages.Add(assistantMsg);

        // Update conversation Timestamp
        conversation.UpdatedAt = DateTime.UtcNow;
        _context.Conversations.Update(conversation);

        await _context.SaveChangesAsync(cancellationToken);

        return new ConversationMessageDto(assistantMsg.Role, assistantMsg.Content, assistantMsg.CreatedAt);
    }
}
