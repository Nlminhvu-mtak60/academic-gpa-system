using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.AiAdvisor;
using AcademicGPA.Application.Features.AiAdvisor.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class ConversationIntegrationTests : IntegrationTestBase
{
    private readonly Mock<IAiAdvisorServiceClient> _mockAiClient;
    private readonly GpaCalculator _gpaCalculator;

    public ConversationIntegrationTests()
    {
        _mockAiClient = new Mock<IAiAdvisorServiceClient>();
        _gpaCalculator = new GpaCalculator();

        _mockAiClient
            .Setup(c => c.GetChatResponseAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<AiServiceAcademicContextDto>(),
                It.IsAny<IEnumerable<AiServiceChatMessageDto>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiServiceResponseDto("Mocked AI advice response.", 100, "mock-gpt"));
    }

    [Fact]
    public async Task StartConversation_WhenLimitExceeded_ShouldThrowUnprocessableEntityException()
    {
        // Arrange
        var handler = new StartConversationCommandHandler(Context, MockCurrentUserService.Object);

        // Seed 50 active conversations for the current user
        for (int i = 0; i < 50; i++)
        {
            Context.Conversations.Add(new Conversation
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUserId,
                Title = $"Chat {i}",
                IsDeleted = false
            });
        }
        await Context.SaveChangesAsync();

        // Act
        var act = () => handler.Handle(new StartConversationCommand("New Chat"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    [Fact]
    public async Task SendMessage_WhenNotOwner_ShouldThrowForbiddenException()
    {
        // Arrange
        var anotherUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "another@school.edu.vn",
            FirstName = "Another",
            LastName = "Student",
            Role = UserRole.Student,
            PasswordHash = "hash"
        };
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = anotherUser.Id,
            Title = "Another User's Conversation",
            IsDeleted = false
        };
        Context.Users.Add(anotherUser);
        Context.Conversations.Add(conversation);
        await Context.SaveChangesAsync();

        var handler = new SendMessageCommandHandler(Context, MockCurrentUserService.Object, _mockAiClient.Object, _gpaCalculator);

        // Act
        var act = () => handler.Handle(new SendMessageCommand(conversation.Id, "Hello"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task SendMessage_WhenExceedingHourlyRateLimit_ShouldThrowRateLimitException()
    {
        // Arrange
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,
            Title = "Integration Chat",
            IsDeleted = false
        };
        Context.Conversations.Add(conversation);

        // Seed 20 messages sent by the user within the last hour
        for (int i = 0; i < 20; i++)
        {
            Context.ConversationMessages.Add(new ConversationMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                Role = "user",
                Content = $"Message {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await Context.SaveChangesAsync();

        var handler = new SendMessageCommandHandler(Context, MockCurrentUserService.Object, _mockAiClient.Object, _gpaCalculator);

        // Act
        var act = () => handler.Handle(new SendMessageCommand(conversation.Id, "21st Message"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RateLimitException>();
    }

    [Fact]
    public async Task DeleteConversation_ShouldPerformSoftDeleteAndVerifyOwnership()
    {
        // Arrange
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = CurrentUserId,
            Title = "ToDelete",
            IsDeleted = false
        };
        Context.Conversations.Add(conversation);
        await Context.SaveChangesAsync();

        var handler = new DeleteConversationCommandHandler(Context, MockCurrentUserService.Object);

        // Act - Soft Delete
        await handler.Handle(new DeleteConversationCommand(conversation.Id), CancellationToken.None);

        // Assert
        var deletedConversation = await Context.Conversations.IgnoreQueryFilters().AsNoTracking().FirstAsync(c => c.Id == conversation.Id);
        deletedConversation.IsDeleted.Should().BeTrue();
    }
}
