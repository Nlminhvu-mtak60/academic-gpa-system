using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcademicGPA.UnitTests;

public class DashboardServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public DashboardServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldThrowUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var gpaCalculator = new GpaCalculator();
        var dashboardService = new DashboardService(context, gpaCalculator);
        var userId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await dashboardService.GetDashboardSummaryAsync(userId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldAutoCreateProfile_WhenUserExistsButProfileDoesNotExist()
    {
        // Arrange
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "Alice",
            LastName = "Green",
            Email = "alice@example.com",
            PasswordHash = "hash"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var gpaCalculator = new GpaCalculator();
        var dashboardService = new DashboardService(context, gpaCalculator);

        // Act
        var result = await dashboardService.GetDashboardSummaryAsync(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Student.FirstName.Should().Be("Alice");
        result.Student.StudentCode.Should().StartWith("ST");
        
        var profile = await context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        profile.Should().NotBeNull();
        profile!.UniversityName.Should().Be("Chưa cập nhật");
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldCalculateCorrectSummary_WhenProfileAndDataExist()
    {
        // Arrange
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var studentProfileId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "hash"
        };

        var profile = new StudentProfile
        {
            Id = studentProfileId,
            UserId = userId,
            StudentCode = "STUDENT123",
            UniversityName = "Vite University",
            MajorName = "CS",
            EnrollmentYear = 2024,
            TotalRequiredCredits = 120,
            User = user
        };

        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            YearName = "2024-2025",
            StartYear = 2024,
            EndYear = 2025,
            Status = "Current",
            IsCurrent = true,
            SortOrder = 0
        };

        var semester = new Semester
        {
            Id = Guid.NewGuid(),
            AcademicYearId = academicYear.Id,
            SemesterName = "Semester 1",
            SortOrder = 0
        };

        var course1 = new Course
        {
            Id = Guid.NewGuid(),
            SemesterId = semester.Id,
            CourseCode = "CS101",
            CourseName = "Intro to CS",
            Credits = 3,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var score1 = new Score
        {
            CourseId = course1.Id,
            AttendanceScore = 9.0m,
            ContinuousScore = 9.0m,
            FinalExamScore = 9.0m,
            CourseScore = 9.0m,
            LetterGrade = "A+",
            Gpa4Value = 4.0m,
            IsPass = true,
            AcademicClassification = "Outstanding"
        };
        course1.Score = score1;

        var course2 = new Course
        {
            Id = Guid.NewGuid(),
            SemesterId = semester.Id,
            CourseCode = "MATH101",
            CourseName = "Calculus 1",
            Credits = 4,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var score2 = new Score
        {
            CourseId = course2.Id,
            AttendanceScore = 6.0m,
            ContinuousScore = 6.0m,
            FinalExamScore = 6.0m,
            CourseScore = 6.0m,
            LetterGrade = "C",
            Gpa4Value = 2.0m,
            IsPass = true,
            AcademicClassification = "Average"
        };
        course2.Score = score2;

        context.Users.Add(user);
        context.StudentProfiles.Add(profile);
        context.AcademicYears.Add(academicYear);
        context.Semesters.Add(semester);
        context.Courses.AddRange(course1, course2);
        context.Scores.AddRange(score1, score2);
        await context.SaveChangesAsync();

        var gpaCalculator = new GpaCalculator();
        var dashboardService = new DashboardService(context, gpaCalculator);

        // Act
        var result = await dashboardService.GetDashboardSummaryAsync(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Student.FirstName.Should().Be("John");
        result.Student.StudentCode.Should().Be("STUDENT123");

        // (9.0 * 3 + 6.0 * 4) / 7 = (27 + 24) / 7 = 51 / 7 = 7.2857 -> rounded to 7.29
        result.PerformanceSummary.CumulativeGpa10.Should().Be(7.29m);
        // (4.0 * 3 + 2.0 * 4) / 7 = (12 + 8) / 7 = 20 / 7 = 2.8571 -> rounded to 2.86
        result.PerformanceSummary.CumulativeGpa4.Should().Be(2.86m);
        result.PerformanceSummary.TotalCreditsCompleted.Should().Be(7);
        result.PerformanceSummary.ClassificationVn.Should().Be("Khá");
        result.PerformanceSummary.CurrentAcademicYearName.Should().Be("2024-2025");
        result.PerformanceSummary.CurrentSemesterName.Should().Be("Semester 1");
        
        result.RecentCourses.Should().HaveCount(2);
        result.RecentCourses.First().CourseCode.Should().Be("CS101"); // Most recently updated
    }
}
