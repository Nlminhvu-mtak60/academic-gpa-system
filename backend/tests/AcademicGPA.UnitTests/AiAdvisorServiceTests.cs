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
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class AiAdvisorServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public AiAdvisorServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    private async Task SeedStudentDataAsync(ApplicationDbContext context)
    {
        // Student A
        var studentA = new User
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@student.com",
            PasswordHash = "hash",
            Role = UserRole.Student,
            IsActive = true
        };
        context.Users.Add(studentA);

        var profileA = new StudentProfile
        {
            UserId = studentA.Id,
            StudentCode = "STUD001",
            UniversityName = "VNU",
            MajorName = "CS",
            EnrollmentYear = 2022,
            TotalRequiredCredits = 120
        };
        context.StudentProfiles.Add(profileA);

        var ay1 = new AcademicYear { StudentProfile = profileA, YearName = "2022-2023", StartYear = 2022, EndYear = 2023 };
        var sem1 = new Semester { AcademicYear = ay1, SemesterName = "Semester 1", SortOrder = 1 };
        
        var course1 = new Course { Semester = sem1, CourseCode = "MATH101", CourseName = "Calculus 1", Credits = 3 };
        var score1 = new Score
        {
            Course = course1,
            AttendanceScore = 5.5m,
            ContinuousScore = 5.5m,
            FinalExamScore = 5.5m,
            CourseScore = 5.5m,
            Gpa4Value = 2.0m,
            LetterGrade = "C",
            IsPass = true
        };
        context.Scores.Add(score1);

        var course2 = new Course { Semester = sem1, CourseCode = "CS101", CourseName = "Intro to CS", Credits = 3 };
        var score2 = new Score
        {
            Course = course2,
            AttendanceScore = 9.0m,
            ContinuousScore = 9.0m,
            FinalExamScore = 9.0m,
            CourseScore = 9.0m,
            Gpa4Value = 4.0m,
            LetterGrade = "A+",
            IsPass = true
        };
        context.Scores.Add(score2);

        // Student B (Other Student)
        var studentB = new User
        {
            Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            FirstName = "Bob",
            LastName = "Jones",
            Email = "bob@student.com",
            PasswordHash = "hash",
            Role = UserRole.Student,
            IsActive = true
        };
        context.Users.Add(studentB);

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task StartConversation_ShouldThrowUnprocessableEntityException_WhenLimitOf50Exceeded()
    {
        using var context = CreateContext();
        await SeedStudentDataAsync(context);

        var aliceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        // Seed 50 active threads
        for (int i = 1; i <= 50; i++)
        {
            context.Conversations.Add(new Conversation
            {
                UserId = aliceId,
                Title = $"Conversation {i}",
                IsDeleted = false
            });
        }
        await context.SaveChangesAsync();

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(u => u.UserId).Returns(aliceId.ToString());

        var handler = new StartConversationCommandHandler(context, mockUserService.Object);

        // Try adding the 51st
        Func<Task> act = async () => await handler.Handle(new StartConversationCommand("New Thread"), CancellationToken.None);
        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    [Fact]
    public async Task SendMessage_ShouldThrowRateLimitException_WhenHourlyLimitOf20Exceeded()
    {
        using var context = CreateContext();
        await SeedStudentDataAsync(context);

        var aliceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = aliceId,
            Title = "Cal 1 Help"
        };
        context.Conversations.Add(conversation);

        // Seed 20 user messages in the last hour
        for (int i = 0; i < 20; i++)
        {
            context.ConversationMessages.Add(new ConversationMessage
            {
                Conversation = conversation,
                Role = "user",
                Content = $"Message {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            });
        }
        await context.SaveChangesAsync();

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(u => u.UserId).Returns(aliceId.ToString());

        var mockAiClient = new Mock<IAiAdvisorServiceClient>();
        var gpaCalc = new GpaCalculator();

        var handler = new SendMessageCommandHandler(context, mockUserService.Object, mockAiClient.Object, gpaCalc);

        Func<Task> act = async () => await handler.Handle(new SendMessageCommand(conversation.Id, "Hello Advisor"), CancellationToken.None);
        await act.Should().ThrowAsync<RateLimitException>();
    }

    [Fact]
    public async Task GetMessages_ShouldThrowForbiddenException_WhenNotOwner()
    {
        using var context = CreateContext();
        await SeedStudentDataAsync(context);

        var aliceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var bobId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var bobsConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = bobId,
            Title = "Bob's Secrets"
        };
        context.Conversations.Add(bobsConversation);
        await context.SaveChangesAsync();

        // Alice tries to read Bob's thread
        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(u => u.UserId).Returns(aliceId.ToString());

        var handler = new GetMessagesQueryHandler(context, mockUserService.Object);

        Func<Task> act = async () => await handler.Handle(new GetMessagesQuery(bobsConversation.Id), CancellationToken.None);
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task SendMessage_ShouldCorrectlyAssembleContext_AndSaveMessages()
    {
        using var context = CreateContext();
        await SeedStudentDataAsync(context);

        var aliceId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = aliceId,
            Title = "Academic Guidance"
        };
        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(u => u.UserId).Returns(aliceId.ToString());

        var mockAiClient = new Mock<IAiAdvisorServiceClient>();
        mockAiClient.Setup(c => c.GetChatResponseAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<AiServiceAcademicContextDto>(),
            It.IsAny<IEnumerable<AiServiceChatMessageDto>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AiServiceResponseDto("Focus on CS101 methods.", 20, "RuleEngine"))
            .Verifiable();

        var gpaCalc = new GpaCalculator();

        var handler = new SendMessageCommandHandler(context, mockUserService.Object, mockAiClient.Object, gpaCalc);

        var result = await handler.Handle(new SendMessageCommand(conversation.Id, "How can I improve my grades?"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Role.Should().Be("assistant");
        result.Content.Should().Be("Focus on CS101 methods.");

        // Verify messages added to database
        var dbMessages = await context.ConversationMessages
            .Where(m => m.ConversationId == conversation.Id)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        dbMessages.Count.Should().Be(2);
        dbMessages[0].Role.Should().Be("user");
        dbMessages[0].Content.Should().Be("How can I improve my grades?");
        dbMessages[1].Role.Should().Be("assistant");
        dbMessages[1].Content.Should().Be("Focus on CS101 methods.");

        // Verify client called with correct parameters
        mockAiClient.Verify(c => c.GetChatResponseAsync(
            "How can I improve my grades?",
            "vi",
            It.Is<AiServiceAcademicContextDto>(ctx => 
                ctx.CurrentCumulativeGpa == 7.25m && 
                ctx.TotalCreditsCompleted == 6 &&
                ctx.TotalCreditsRequired == 120 &&
                ctx.WeakCourses.Count == 1 &&
                ctx.WeakCourses[0].CourseCode == "MATH101"
            ),
            It.IsAny<IEnumerable<AiServiceChatMessageDto>>(),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }
}
