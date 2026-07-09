using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcademicGPA.UnitTests;

public class HistoricalImportTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public HistoricalImportTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    [Fact]
    public async Task GetDashboardSummaryAsync_ShouldCalculateCorrectGPA_WithImportedAndDetailedData()
    {
        // Arrange
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var studentProfileId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            FirstName = "Alice",
            LastName = "Import",
            Email = "alice.import@example.com",
            PasswordHash = "hash"
        };

        var profile = new StudentProfile
        {
            Id = studentProfileId,
            UserId = userId,
            StudentCode = "STIMPORT123",
            UniversityName = "Import University",
            MajorName = "CS",
            EnrollmentYear = 2024,
            TotalRequiredCredits = 120,
            User = user
        };

        // 1. Imported Academic Year: 30 credits, 8.5 GPA10, 3.5 GPA4
        var importedYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            YearName = "2023-2024",
            StartYear = 2023,
            EndYear = 2024,
            Status = "Completed",
            IsCurrent = false,
            SortOrder = 0,
            IsImported = true,
            ImportedCredits = 30,
            ImportedGpa10 = 8.5m,
            ImportedGpa4 = 3.5m
        };

        // 2. Active Year (detailed): contains 1 imported semester and 1 detailed semester
        var activeYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            YearName = "2024-2025",
            StartYear = 2024,
            EndYear = 2025,
            Status = "Current",
            IsCurrent = true,
            SortOrder = 1,
            IsImported = false
        };

        // 2a. Imported Semester: 15 credits, 8.0 GPA10, 3.0 GPA4
        var importedSemester = new Semester
        {
            Id = Guid.NewGuid(),
            AcademicYearId = activeYear.Id,
            SemesterName = "Semester 1",
            SortOrder = 0,
            IsImported = true,
            ImportedCredits = 15,
            ImportedGpa10 = 8.0m,
            ImportedGpa4 = 3.0m
        };

        // 2b. Detailed Semester: contains 2 courses
        var detailedSemester = new Semester
        {
            Id = Guid.NewGuid(),
            AcademicYearId = activeYear.Id,
            SemesterName = "Semester 2",
            SortOrder = 1,
            IsImported = false
        };

        var course1 = new Course
        {
            Id = Guid.NewGuid(),
            SemesterId = detailedSemester.Id,
            CourseCode = "CS102",
            CourseName = "Advanced CS",
            Credits = 3,
            UpdatedAt = DateTime.UtcNow
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
            SemesterId = detailedSemester.Id,
            CourseCode = "MATH102",
            CourseName = "Calculus 2",
            Credits = 4,
            UpdatedAt = DateTime.UtcNow
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
        context.AcademicYears.AddRange(importedYear, activeYear);
        context.Semesters.AddRange(importedSemester, detailedSemester);
        context.Courses.AddRange(course1, course2);
        context.Scores.AddRange(score1, score2);
        await context.SaveChangesAsync();

        var gpaCalculator = new GpaCalculator();
        var dashboardService = new DashboardService(context, gpaCalculator);

        // Act
        var result = await dashboardService.GetDashboardSummaryAsync(userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PerformanceSummary.TotalCredits.Should().Be(52); // 30 + 15 + 7
        result.PerformanceSummary.PassedCredits.Should().Be(52);

        // GPA 10 calculation:
        // Weight = (30 * 8.5) + (15 * 8.0) + (3 * 9.0) + (4 * 6.0) = 255 + 120 + 27 + 24 = 426
        // Total Graded Credits = 30 + 15 + 7 = 52
        // GPA 10 = 426 / 52 = 8.1923 -> rounded to 8.19
        result.PerformanceSummary.CumulativeGpa10.Should().Be(8.19m);

        // GPA 4 calculation:
        // Weight = (30 * 3.5) + (15 * 3.0) + (3 * 4.0) + (4 * 2.0) = 105 + 45 + 12 + 8 = 170
        // GPA 4 = 170 / 52 = 3.2692 -> rounded to 3.27
        result.PerformanceSummary.CumulativeGpa4.Should().Be(3.27m);
    }
}
