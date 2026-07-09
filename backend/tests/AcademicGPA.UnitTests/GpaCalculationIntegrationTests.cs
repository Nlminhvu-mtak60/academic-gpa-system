using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcademicGPA.UnitTests;

public class GpaCalculationIntegrationTests : IntegrationTestBase
{
    private readonly GpaCalculator _gpaCalculator = new();

    [Fact]
    public async Task GpaCalculation_WithRetakesAndCreditCounting_IntegrationTest()
    {
        // 1. Arrange: Create Academic Structure
        var year1 = new AcademicYear { Id = Guid.NewGuid(), StudentProfileId = CurrentStudentProfileId, YearName = "2024-2025" };
        var sem1 = new Semester { Id = Guid.NewGuid(), AcademicYearId = year1.Id, SemesterName = "Semester 1", SortOrder = 1 };
        var sem2 = new Semester { Id = Guid.NewGuid(), AcademicYearId = year1.Id, SemesterName = "Semester 2", SortOrder = 2 };

        // Course 1 (Original attempt, failed)
        var course1 = new Course { Id = Guid.NewGuid(), SemesterId = sem1.Id, CourseCode = "CS101", CourseName = "Intro to CS", Credits = 3, IsRetake = false };
        var score1 = new Score { CourseId = course1.Id, CourseScore = 3.5m, Gpa4Value = 0.0m, IsPass = false };
        course1.Score = score1;

        // Course 2 (Retake attempt, passed, better score)
        var course1Retake = new Course { Id = Guid.NewGuid(), SemesterId = sem2.Id, CourseCode = "CS101", CourseName = "Intro to CS", Credits = 3, IsRetake = true, OriginalCourseId = course1.Id };
        var score1Retake = new Score { CourseId = course1Retake.Id, CourseScore = 8.0m, Gpa4Value = 3.5m, IsPass = true };
        course1Retake.Score = score1Retake;

        // Course 3 (Other course, passed)
        var course2 = new Course { Id = Guid.NewGuid(), SemesterId = sem1.Id, CourseCode = "MATH101", CourseName = "Calculus I", Credits = 4, IsRetake = false };
        var score2 = new Score { CourseId = course2.Id, CourseScore = 7.0m, Gpa4Value = 3.0m, IsPass = true };
        course2.Score = score2;

        Context.AcademicYears.Add(year1);
        Context.Semesters.AddRange(sem1, sem2);
        Context.Courses.AddRange(course1, course1Retake, course2);
        Context.Scores.AddRange(score1, score1Retake, score2);
        await Context.SaveChangesAsync();

        // 2. Load all courses for the student
        var studentCourses = await Context.Courses
            .Include(c => c.Score)
            .Where(c => c.Semester.AcademicYear.StudentProfileId == CurrentStudentProfileId && !c.IsDeleted)
            .ToListAsync();

        // 3. Act: Calculate Semester 1 GPA
        var sem1Courses = studentCourses.Where(c => c.SemesterId == sem1.Id).ToList();
        var sem1Gpa10 = _gpaCalculator.CalculateGpa10(sem1Courses);
        var sem1Gpa4 = _gpaCalculator.CalculateGpa4(sem1Courses);

        // Assert Sem 1 GPA
        // Weighted sum = (3.5 * 3) + (7.0 * 4) = 10.5 + 28.0 = 38.5
        // Credits = 7
        // GPA 10 = 38.5 / 7 = 5.5
        // GPA 4 = (0.0 * 3 + 3.0 * 4) / 7 = 12 / 7 = 1.71m
        sem1Gpa10.Should().Be(5.5m);
        sem1Gpa4.Should().Be(1.71m);

        // 4. Act: Calculate Cumulative GPA (should filter CS101 original, keep retake)
        var bestAttempts = _gpaCalculator.FilterBestAttempts(studentCourses).ToList();
        var cumulativeGpa10 = _gpaCalculator.CalculateGpa10(bestAttempts);
        var cumulativeGpa4 = _gpaCalculator.CalculateGpa4(bestAttempts);

        // Assert Cumulative GPA (CS101 retake = 8.0, MATH101 = 7.0)
        // Weighted sum = (8.0 * 3) + (7.0 * 4) = 24.0 + 28.0 = 52.0
        // Total credits = 7
        // Cumulative GPA 10 = 52 / 7 = 7.43m
        // Cumulative GPA 4 = (3.5 * 3 + 3.0 * 4) / 7 = (10.5 + 12) / 7 = 22.5 / 7 = 3.21m
        cumulativeGpa10.Should().Be(7.43m);
        cumulativeGpa4.Should().Be(3.21m);

        // 5. Act: Calculate Credits completed
        int totalCreditsAttempted = bestAttempts.Sum(c => c.Credits);
        int totalCreditsPassed = bestAttempts.Where(c => c.Score != null && c.Score.IsPass == true).Sum(c => c.Credits);
        int totalCreditsFailed = bestAttempts.Where(c => c.Score != null && c.Score.IsPass == false).Sum(c => c.Credits);

        totalCreditsAttempted.Should().Be(7);
        totalCreditsPassed.Should().Be(7);
        totalCreditsFailed.Should().Be(0);
    }
}
