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

public class StatisticsServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public StatisticsServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    [Fact]
    public async Task GetGpaTrendAsync_ShouldReturnChronologicalSemestersWithRollingCumulativeGpas()
    {
        // Arrange
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var studentProfileId = Guid.NewGuid();

        var profile = new StudentProfile
        {
            Id = studentProfileId,
            UserId = userId,
            StudentCode = "STUDENT123",
            UniversityName = "Vite University",
            MajorName = "CS",
            EnrollmentYear = 2024,
            TotalRequiredCredits = 120
        };

        // Year 1
        var year1 = new AcademicYear
        {
            Id = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            YearName = "2024-2025",
            StartYear = 2024,
            EndYear = 2025,
            SortOrder = 0
        };

        var sem1_1 = new Semester
        {
            Id = Guid.NewGuid(),
            AcademicYearId = year1.Id,
            SemesterName = "Semester 1",
            SortOrder = 0
        };

        var course1_1 = new Course { Id = Guid.NewGuid(), SemesterId = sem1_1.Id, CourseCode = "CS101", Credits = 3 };
        var score1_1 = new Score { CourseId = course1_1.Id, CourseScore = 8.0m, Gpa4Value = 3.0m, IsPass = true, LetterGrade = "B" };
        course1_1.Score = score1_1;

        var sem1_2 = new Semester
        {
            Id = Guid.NewGuid(),
            AcademicYearId = year1.Id,
            SemesterName = "Semester 2",
            SortOrder = 1
        };

        var course1_2 = new Course { Id = Guid.NewGuid(), SemesterId = sem1_2.Id, CourseCode = "CS102", Credits = 3 };
        var score1_2 = new Score { CourseId = course1_2.Id, CourseScore = 9.0m, Gpa4Value = 4.0m, IsPass = true, LetterGrade = "A+" };
        course1_2.Score = score1_2;

        context.StudentProfiles.Add(profile);
        context.AcademicYears.Add(year1);
        context.Semesters.AddRange(sem1_1, sem1_2);
        context.Courses.AddRange(course1_1, course1_2);
        context.Scores.AddRange(score1_1, score1_2);
        await context.SaveChangesAsync();

        var gpaCalculator = new GpaCalculator();
        var statsService = new StatisticsService(context, gpaCalculator);

        // Act
        var result = await statsService.GetGpaTrendAsync(userId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        
        // Semester 1
        result[0].SemesterName.Should().Be("Semester 1");
        result[0].Gpa10.Should().Be(8.0m);
        result[0].CumulativeGpa10.Should().Be(8.0m);

        // Semester 2
        result[1].SemesterName.Should().Be("Semester 2");
        result[1].Gpa10.Should().Be(9.0m);
        // Cumulative GPA up to semester 2: (8.0 * 3 + 9.0 * 3) / 6 = 8.5
        result[1].CumulativeGpa10.Should().Be(8.5m);
        result[1].CumulativeGpa4.Should().Be(3.5m);
    }

    [Fact]
    public async Task GetGradeDistributionAsync_ShouldCountCorrectly_WhenDataExists()
    {
        // Arrange
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var studentProfileId = Guid.NewGuid();

        var profile = new StudentProfile
        {
            Id = studentProfileId,
            UserId = userId,
            StudentCode = "STUDENT123",
            UniversityName = "Vite University",
            MajorName = "CS",
            EnrollmentYear = 2024,
            TotalRequiredCredits = 120
        };

        var year = new AcademicYear { Id = Guid.NewGuid(), StudentProfileId = studentProfileId, YearName = "2024-2025", SortOrder = 0 };
        var sem = new Semester { Id = Guid.NewGuid(), AcademicYearId = year.Id, SemesterName = "Semester 1", SortOrder = 0 };

        var course1 = new Course { Id = Guid.NewGuid(), SemesterId = sem.Id, CourseCode = "CS101", Credits = 3 };
        var score1 = new Score { CourseId = course1.Id, CourseScore = 9.0m, LetterGrade = "A+" };
        course1.Score = score1;

        var course2 = new Course { Id = Guid.NewGuid(), SemesterId = sem.Id, CourseCode = "CS102", Credits = 3 };
        var score2 = new Score { CourseId = course2.Id, CourseScore = 6.0m, LetterGrade = "C" };
        course2.Score = score2;

        context.StudentProfiles.Add(profile);
        context.AcademicYears.Add(year);
        context.Semesters.Add(sem);
        context.Courses.AddRange(course1, course2);
        context.Scores.AddRange(score1, score2);
        await context.SaveChangesAsync();

        var gpaCalculator = new GpaCalculator();
        var statsService = new StatisticsService(context, gpaCalculator);

        // Act
        var result = await statsService.GetGradeDistributionAsync(userId, CancellationToken.None);

        // Assert
        result.Aplus.Should().Be(1);
        result.C.Should().Be(1);
        result.B.Should().Be(0);
    }

    [Fact]
    public async Task GetCreditProgressAsync_ShouldCalculateCreditsCorrectly()
    {
        // Arrange
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var studentProfileId = Guid.NewGuid();

        var profile = new StudentProfile
        {
            Id = studentProfileId,
            UserId = userId,
            StudentCode = "STUDENT123",
            UniversityName = "Vite University",
            MajorName = "CS",
            EnrollmentYear = 2024,
            TotalRequiredCredits = 100
        };

        var year = new AcademicYear { Id = Guid.NewGuid(), StudentProfileId = studentProfileId, YearName = "2024-2025", SortOrder = 0 };
        var sem = new Semester { Id = Guid.NewGuid(), AcademicYearId = year.Id, SemesterName = "Semester 1", SortOrder = 0 };

        var coursePass = new Course { Id = Guid.NewGuid(), SemesterId = sem.Id, CourseCode = "CS101", Credits = 3 };
        var scorePass = new Score { CourseId = coursePass.Id, CourseScore = 8.0m, IsPass = true };
        coursePass.Score = scorePass;

        var courseFail = new Course { Id = Guid.NewGuid(), SemesterId = sem.Id, CourseCode = "CS102", Credits = 4 };
        var scoreFail = new Score { CourseId = courseFail.Id, CourseScore = 3.5m, IsPass = false };
        courseFail.Score = scoreFail;

        var courseInProgress = new Course { Id = Guid.NewGuid(), SemesterId = sem.Id, CourseCode = "CS103", Credits = 3 };

        context.StudentProfiles.Add(profile);
        context.AcademicYears.Add(year);
        context.Semesters.Add(sem);
        context.Courses.AddRange(coursePass, courseFail, courseInProgress);
        context.Scores.AddRange(scorePass, scoreFail);
        await context.SaveChangesAsync();

        var gpaCalculator = new GpaCalculator();
        var statsService = new StatisticsService(context, gpaCalculator);

        // Act
        var result = await statsService.GetCreditProgressAsync(userId, CancellationToken.None);

        // Assert
        result.CompletedCredits.Should().Be(3);
        result.FailedCredits.Should().Be(4);
        result.InProgressCredits.Should().Be(3);
        result.TotalRequiredCredits.Should().Be(100);
        result.RemainingCredits.Should().Be(97); // 100 - 3
    }

    [Fact]
    public async Task GetStrengthsWeaknessesAsync_ShouldCorrectlySortCourses()
    {
        // Arrange
        using var context = CreateContext();
        var userId = Guid.NewGuid();
        var studentProfileId = Guid.NewGuid();

        var profile = new StudentProfile
        {
            Id = studentProfileId,
            UserId = userId,
            StudentCode = "STUDENT123",
            UniversityName = "Vite University",
            MajorName = "CS",
            EnrollmentYear = 2024,
            TotalRequiredCredits = 120
        };

        var year = new AcademicYear { Id = Guid.NewGuid(), StudentProfileId = studentProfileId, YearName = "2024-2025", SortOrder = 0 };
        var sem = new Semester { Id = Guid.NewGuid(), AcademicYearId = year.Id, SemesterName = "Semester 1", SortOrder = 0 };

        var courseHigh = new Course { Id = Guid.NewGuid(), SemesterId = sem.Id, CourseCode = "CS101", CourseName = "Intro to CS", Credits = 3 };
        var scoreHigh = new Score { CourseId = courseHigh.Id, CourseScore = 9.5m, LetterGrade = "A+" };
        courseHigh.Score = scoreHigh;

        var courseLow = new Course { Id = Guid.NewGuid(), SemesterId = sem.Id, CourseCode = "MATH101", CourseName = "Calculus 1", Credits = 4 };
        var scoreLow = new Score { CourseId = courseLow.Id, CourseScore = 4.5m, LetterGrade = "D" };
        courseLow.Score = scoreLow;

        context.StudentProfiles.Add(profile);
        context.AcademicYears.Add(year);
        context.Semesters.Add(sem);
        context.Courses.AddRange(courseHigh, courseLow);
        context.Scores.AddRange(scoreHigh, scoreLow);
        await context.SaveChangesAsync();

        var gpaCalculator = new GpaCalculator();
        var statsService = new StatisticsService(context, gpaCalculator);

        // Act
        var result = await statsService.GetStrengthsWeaknessesAsync(userId, CancellationToken.None);

        // Assert
        result.StrongestCourses.Should().HaveCount(2);
        result.StrongestCourses[0].CourseCode.Should().Be("CS101");
        result.StrongestCourses[0].Score.Should().Be(9.5m);

        result.WeakestCourses.Should().HaveCount(2);
        result.WeakestCourses[0].CourseCode.Should().Be("MATH101");
        result.WeakestCourses[0].Score.Should().Be(4.5m);
    }
}
