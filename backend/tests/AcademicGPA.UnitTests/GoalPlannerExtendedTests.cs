using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class GoalPlannerExtendedTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly GpaCalculator _gpaCalculator = new();
    private readonly Mock<INotificationService> _mockNotification;

    public GoalPlannerExtendedTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockNotification = new Mock<INotificationService>();
    }

    private ApplicationDbContext CreateContext() => new(_options);

    private async Task<(Guid UserId, Guid ProfileId)> SeedStudentAsync(ApplicationDbContext context, int totalRequiredCredits)
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = $"test.{Guid.NewGuid()}@example.com",
            PasswordHash = "hash"
        };

        var profile = new StudentProfile
        {
            Id = profileId,
            UserId = userId,
            StudentCode = $"STU{Guid.NewGuid().ToString()[..6]}",
            UniversityName = "Demo Uni",
            MajorName = "CS",
            EnrollmentYear = 2024,
            TotalRequiredCredits = totalRequiredCredits,
            User = user
        };

        context.Users.Add(user);
        context.StudentProfiles.Add(profile);
        await context.SaveChangesAsync();

        return (userId, profileId);
    }

    [Fact]
    public async Task GetRequiredGpaAsync_WhenNoRemainingCreditsAndTargetAchieved_ShouldReturnAlreadyAchieved()
    {
        // Arrange
        using var context = CreateContext();
        var (userId, profileId) = await SeedStudentAsync(context, 10);

        var academicYear = new AcademicYear { Id = Guid.NewGuid(), StudentProfileId = profileId, YearName = "2024-2025" };
        var semester = new Semester { Id = Guid.NewGuid(), AcademicYearId = academicYear.Id, SemesterName = "Semester 1" };
        var course = new Course { Id = Guid.NewGuid(), SemesterId = semester.Id, CourseName = "C1", Credits = 10 };
        var score = new Score { CourseId = course.Id, CourseScore = 9.0m, Gpa4Value = 4.0m, IsPass = true };
        course.Score = score;

        context.AcademicYears.Add(academicYear);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.Scores.Add(score);
        await context.SaveChangesAsync();

        var service = new GoalPlannerService(context, _gpaCalculator, _mockNotification.Object);
        await service.SetGoalAsync(userId, 8.5m, "Target 8.5", CancellationToken.None);

        // Act
        var result = await service.GetRequiredGpaAsync(userId, CancellationToken.None);

        // Assert
        result.Feasibility.Should().Be("Already Achieved");
        result.CreditsRemaining.Should().Be(0);
        result.RequiredRemainingGpa10.Should().Be(0m);
    }

    [Fact]
    public async Task GetRequiredGpaAsync_WhenNoRemainingCreditsAndTargetNotAchieved_ShouldReturnNotAchievable()
    {
        // Arrange
        using var context = CreateContext();
        var (userId, profileId) = await SeedStudentAsync(context, 10);

        var academicYear = new AcademicYear { Id = Guid.NewGuid(), StudentProfileId = profileId, YearName = "2024-2025" };
        var semester = new Semester { Id = Guid.NewGuid(), AcademicYearId = academicYear.Id, SemesterName = "Semester 1" };
        var course = new Course { Id = Guid.NewGuid(), SemesterId = semester.Id, CourseName = "C1", Credits = 10 };
        var score = new Score { CourseId = course.Id, CourseScore = 7.0m, Gpa4Value = 3.0m, IsPass = true };
        course.Score = score;

        context.AcademicYears.Add(academicYear);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.Scores.Add(score);
        await context.SaveChangesAsync();

        var service = new GoalPlannerService(context, _gpaCalculator, _mockNotification.Object);
        await service.SetGoalAsync(userId, 8.5m, "Target 8.5", CancellationToken.None);

        // Act
        var result = await service.GetRequiredGpaAsync(userId, CancellationToken.None);

        // Assert
        result.Feasibility.Should().Be("Not Achievable");
        result.CreditsRemaining.Should().Be(0);
    }

    [Fact]
    public async Task GetRequiredGpaAsync_WhenRequiredGpaIsNegative_ShouldReturnAlreadyAchieved()
    {
        // Arrange
        using var context = CreateContext();
        var (userId, profileId) = await SeedStudentAsync(context, 20); // 10 credits remaining

        var academicYear = new AcademicYear { Id = Guid.NewGuid(), StudentProfileId = profileId, YearName = "2024-2025" };
        var semester = new Semester { Id = Guid.NewGuid(), AcademicYearId = academicYear.Id, SemesterName = "Semester 1" };
        var course = new Course { Id = Guid.NewGuid(), SemesterId = semester.Id, CourseName = "C1", Credits = 10 };
        var score = new Score { CourseId = course.Id, CourseScore = 9.5m, Gpa4Value = 4.0m, IsPass = true };
        course.Score = score;

        context.AcademicYears.Add(academicYear);
        context.Semesters.Add(semester);
        context.Courses.Add(course);
        context.Scores.Add(score);
        await context.SaveChangesAsync();

        var service = new GoalPlannerService(context, _gpaCalculator, _mockNotification.Object);
        
        // Target is 4.0. Current cumulative score is 9.5 (weighted sum = 95).
        // (4.0 * 20 - 9.5 * 10) / 10 = (80 - 95) / 10 = -1.5 (which is <= 0)
        await service.SetGoalAsync(userId, 4.0m, "Target 4.0", CancellationToken.None);

        // Act
        var result = await service.GetRequiredGpaAsync(userId, CancellationToken.None);

        // Assert
        result.RequiredRemainingGpa10.Should().BeLessThanOrEqualTo(0m);
        result.Feasibility.Should().Be("Already Achieved");
    }
}
