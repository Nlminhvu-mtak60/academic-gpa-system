using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Goals.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class GoalPlannerServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly GpaCalculator _gpaCalculator = new();

    public GoalPlannerServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    private async Task<(Guid UserId, Guid StudentProfileId)> SeedStudentAsync(ApplicationDbContext context)
    {
        var userId = Guid.NewGuid();
        var studentProfileId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice.smith@example.com",
            PasswordHash = "hash"
        };

        var profile = new StudentProfile
        {
            Id = studentProfileId,
            UserId = userId,
            StudentCode = "STUDENT999",
            UniversityName = "Demo University",
            MajorName = "SE",
            EnrollmentYear = 2024,
            TotalRequiredCredits = 20, // Small limit for testing
            User = user
        };

        context.Users.Add(user);
        context.StudentProfiles.Add(profile);
        await context.SaveChangesAsync();

        return (userId, studentProfileId);
    }

    [Fact]
    public async Task SetGoalAsync_ShouldDeactivatePreviousActiveGoal_AndAutoComputeGpa4()
    {
        // Arrange
        using var context = CreateContext();
        var (userId, profileId) = await SeedStudentAsync(context);
        var service = new GoalPlannerService(context, _gpaCalculator, new Mock<INotificationService>().Object);

        // Act - set first goal
        var goal1 = await service.SetGoalAsync(userId, 8.2m, "My first goal", CancellationToken.None);

        // Assert first goal
        goal1.Should().NotBeNull();
        goal1.TargetCumulativeGpa10.Should().Be(8.2m);
        goal1.TargetCumulativeGpa4.Should().Be(3.5m); // Maps to 3.5 per MapGpa10ToGpa4
        goal1.IsActive.Should().BeTrue();
        goal1.IsAchieved.Should().BeFalse();

        // Act - set second goal
        var goal2 = await service.SetGoalAsync(userId, 9.1m, "My second goal", CancellationToken.None);

        // Assert second goal and first goal deactivation
        goal2.Should().NotBeNull();
        goal2.TargetCumulativeGpa10.Should().Be(9.1m);
        goal2.TargetCumulativeGpa4.Should().Be(4.0m);
        goal2.IsActive.Should().BeTrue();

        var firstGoalEntity = await context.AcademicGoals.FindAsync(goal1.Id);
        firstGoalEntity!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task SetGoalAsync_ShouldAutoMarkAsAchieved_WhenCurrentGpaExceedsTarget()
    {
        // Arrange
        using var context = CreateContext();
        var (userId, profileId) = await SeedStudentAsync(context);

        // Setup some academic data with GPA = 9.0
        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            StudentProfileId = profileId,
            YearName = "2024-2025",
            IsCurrent = true
        };
        var semester = new Semester { Id = Guid.NewGuid(), AcademicYearId = academicYear.Id, SemesterName = "Semester 1" };
        var course = new Course { Id = Guid.NewGuid(), SemesterId = semester.Id, CourseName = "Course 1", Credits = 4 };
        var score = new Score
        {
            CourseId = course.Id,
            AttendanceScore = 9.0m,
            ContinuousScore = 9.0m,
            FinalExamScore = 9.0m,
            CourseScore = 9.0m,
            IsPass = true
        };
        course.Score = score;

        context.AcademicYears.Add(academicYear);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.Scores.Add(score);
        await context.SaveChangesAsync();

        var service = new GoalPlannerService(context, _gpaCalculator, new Mock<INotificationService>().Object);

        // Act - Set target GPA 8.0 (current is 9.0)
        var goal = await service.SetGoalAsync(userId, 8.0m, "Low goal", CancellationToken.None);

        // Assert
        goal.IsAchieved.Should().BeTrue();
    }

    [Fact]
    public async Task GetRequiredGpaAsync_ShouldThrowUnprocessableEntity_WhenNoActiveGoalExists()
    {
        // Arrange
        using var context = CreateContext();
        var (userId, _) = await SeedStudentAsync(context);
        var service = new GoalPlannerService(context, _gpaCalculator, new Mock<INotificationService>().Object);

        // Act & Assert
        Func<Task> act = async () => await service.GetRequiredGpaAsync(userId, CancellationToken.None);
        await act.Should().ThrowAsync<UnprocessableEntityException>();
    }

    [Fact]
    public async Task GetRequiredGpaAsync_ShouldCalculateFeasibilityCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var (userId, profileId) = await SeedStudentAsync(context);

        // profile has TotalRequiredCredits = 20
        // Let's add 1 course of 4 credits with score 9.0 (current cumulative GPA = 9.0)
        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            StudentProfileId = profileId,
            YearName = "2024-2025",
            IsCurrent = true
        };
        var semester = new Semester { Id = Guid.NewGuid(), AcademicYearId = academicYear.Id, SemesterName = "Semester 1" };
        var course = new Course { Id = Guid.NewGuid(), SemesterId = semester.Id, CourseName = "Course 1", Credits = 4 };
        var score = new Score
        {
            CourseId = course.Id,
            AttendanceScore = 9.0m,
            ContinuousScore = 9.0m,
            FinalExamScore = 9.0m,
            CourseScore = 9.0m,
            IsPass = true
        };
        course.Score = score;

        context.AcademicYears.Add(academicYear);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.Scores.Add(score);
        await context.SaveChangesAsync();

        var service = new GoalPlannerService(context, _gpaCalculator, new Mock<INotificationService>().Object);

        // 1. Goal = 8.0. Current = 9.0.
        // Already achieved.
        await service.SetGoalAsync(userId, 8.0m, "Goal 8.0", CancellationToken.None);
        var requiredGpa8 = await service.GetRequiredGpaAsync(userId, CancellationToken.None);
        requiredGpa8.Feasibility.Should().Be("Already Achieved");

        // 2. Goal = 9.5. Current = 9.0 (4 credits out of 20 total, 16 credits remaining).
        // (9.5 * 20 - 9.0 * 4) / 16 = (190 - 36) / 16 = 154 / 16 = 9.625 -> rounds to 9.63
        // Since 9.63 <= 10.0, it is Achievable.
        await service.SetGoalAsync(userId, 9.5m, "Goal 9.5", CancellationToken.None);
        var requiredGpa95 = await service.GetRequiredGpaAsync(userId, CancellationToken.None);
        requiredGpa95.Feasibility.Should().Be("Achievable");
        requiredGpa95.RequiredRemainingGpa10.Should().Be(9.63m);

        // 3. Goal = 9.9. Current = 9.0.
        // (9.9 * 20 - 9.0 * 4) / 16 = (198 - 36) / 16 = 162 / 16 = 10.125 -> rounds to 10.13
        // Since 10.13 > 10.0, it is Not Achievable.
        await service.SetGoalAsync(userId, 9.9m, "Goal 9.9", CancellationToken.None);
        var requiredGpa99 = await service.GetRequiredGpaAsync(userId, CancellationToken.None);
        requiredGpa99.Feasibility.Should().Be("Not Achievable");
        requiredGpa99.RequiredRemainingGpa10.Should().Be(10.13m);
    }

    [Fact]
    public async Task SimulateScenariosAsync_ShouldReturnCorrectCalculations_WithoutPersisting()
    {
        // Arrange
        using var context = CreateContext();
        var (userId, profileId) = await SeedStudentAsync(context);

        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            StudentProfileId = profileId,
            YearName = "2024-2025",
            IsCurrent = true
        };
        var semester = new Semester { Id = Guid.NewGuid(), AcademicYearId = academicYear.Id, SemesterName = "Semester 1", SortOrder = 1 };
        var course = new Course { Id = Guid.NewGuid(), SemesterId = semester.Id, CourseName = "Course 1", Credits = 4 };
        
        context.AcademicYears.Add(academicYear);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        var service = new GoalPlannerService(context, _gpaCalculator, new Mock<INotificationService>().Object);
        await service.SetGoalAsync(userId, 8.5m, "Target 8.5", CancellationToken.None);

        // Act - Simulate Attendance=9.0, Continuous=9.0, FinalExam=9.0 -> CourseScore=9.0
        var simulationInput = new List<SimulatedCourseInput>
        {
            new SimulatedCourseInput(course.Id, 9.0m, 9.0m, 9.0m)
        };
        var result = await service.SimulateScenariosAsync(userId, simulationInput, CancellationToken.None);

        // Assert result
        result.Should().NotBeNull();
        result.SimulatedSemesterGpa10.Should().Be(9.0m);
        result.SimulatedCumulativeGpa10.Should().Be(9.0m);
        result.TargetVariance.Should().Be(0.5m); // 9.0 - 8.5 = 0.5

        // Verify DB course score was NOT persisted
        var dbCourse = await context.Courses.Include(c => c.Score).FirstOrDefaultAsync(c => c.Id == course.Id);
        dbCourse!.Score.Should().BeNull();
    }
}
