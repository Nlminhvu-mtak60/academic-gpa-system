using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Scores.Commands.UpdateScores;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Persistence.Repositories;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class CourseGradeIntegrationTests : IntegrationTestBase
{
    private readonly Mock<INotificationService> _mockNotification;
    private readonly GpaCalculator _gpaCalculator;

    public CourseGradeIntegrationTests()
    {
        _mockNotification = new Mock<INotificationService>();
        _gpaCalculator = new GpaCalculator();
    }

    [Fact]
    public async Task CourseGrade_UpdateAndRecalculate_IntegrationTest()
    {
        // 1. Arrange Data
        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            StudentProfileId = CurrentStudentProfileId,
            YearName = "2024-2025"
        };
        var semester = new Semester
        {
            Id = Guid.NewGuid(),
            AcademicYearId = academicYear.Id,
            SemesterName = "Semester 1",
            SortOrder = 1
        };
        var course = new Course
        {
            Id = Guid.NewGuid(),
            SemesterId = semester.Id,
            CourseCode = "CS101",
            CourseName = "Introduction to CS",
            Credits = 3
        };

        Context.AcademicYears.Add(academicYear);
        Context.Semesters.Add(semester);
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();

        var unitOfWork = new UnitOfWork(Context);
        var updateScoresHandler = new UpdateScoresCommandHandler(
            Context, 
            MockCurrentUserService.Object, 
            unitOfWork, 
            _gpaCalculator, 
            _mockNotification.Object
        );

        // 2. Act: Set Scores (Attendance=9.0, Continuous=8.5, Final=9.0)
        // 9.0 * 0.1 + 8.5 * 0.3 + 9.0 * 0.6 = 0.9 + 2.55 + 5.4 = 8.85 -> rounds to 8.9
        var command = new UpdateScoresCommand(course.Id, 9.0m, 8.5m, 9.0m);
        var result = await updateScoresHandler.Handle(command, CancellationToken.None);

        // 3. Assert calculation
        result.Should().NotBeNull();
        result.AttendanceScore.Should().Be(9.0m);
        result.ContinuousScore.Should().Be(8.5m);
        result.FinalExamScore.Should().Be(9.0m);
        result.CourseScore.Should().Be(8.9m);
        result.LetterGrade.Should().Be("A"); // 8.9 is in [8.5, 9.0) -> A
        result.Gpa4Value.Should().Be(3.7m);
        result.IsPass.Should().BeTrue();

        // 4. Assert Audit Logs are created
        var auditLogs = await Context.ScoreAuditLogs.Where(l => l.CourseId == course.Id).ToListAsync();
        auditLogs.Should().HaveCount(3);
        auditLogs.Should().Contain(l => l.FieldChanged == "AttendanceScore" && l.NewValue == 9.0m.ToString("0.0"));
        auditLogs.Should().Contain(l => l.FieldChanged == "ContinuousScore" && l.NewValue == 8.5m.ToString("0.0"));
        auditLogs.Should().Contain(l => l.FieldChanged == "FinalExamScore" && l.NewValue == 9.0m.ToString("0.0"));

        // 5. Act: Update scores again (continuous changed from 8.5 to 9.0)
        // 9.0 * 0.1 + 9.0 * 0.3 + 9.0 * 0.6 = 9.0
        var updateCommand = new UpdateScoresCommand(course.Id, 9.0m, 9.0m, 9.0m);
        var updateResult = await updateScoresHandler.Handle(updateCommand, CancellationToken.None);

        // 6. Assert recalculation
        updateResult.CourseScore.Should().Be(9.0m);
        updateResult.LetterGrade.Should().Be("A+");
        updateResult.Gpa4Value.Should().Be(4.0m);

        // Assert audit log for updated score field
        var newAuditLogs = await Context.ScoreAuditLogs.Where(l => l.CourseId == course.Id).ToListAsync();
        newAuditLogs.Should().HaveCount(4);
        newAuditLogs.Should().Contain(l => l.FieldChanged == "ContinuousScore" && l.OldValue == 8.5m.ToString("0.0") && l.NewValue == 9.0m.ToString("0.0"));

        // 7. Act: Clear scores (set null)
        var clearCommand = new UpdateScoresCommand(course.Id, null, null, null);
        var clearResult = await updateScoresHandler.Handle(clearCommand, CancellationToken.None);

        // Assert cleared values
        clearResult.AttendanceScore.Should().BeNull();
        clearResult.ContinuousScore.Should().BeNull();
        clearResult.FinalExamScore.Should().BeNull();
        clearResult.CourseScore.Should().BeNull();
        clearResult.LetterGrade.Should().BeNull();
        clearResult.Gpa4Value.Should().BeNull();
        clearResult.IsPass.Should().BeNull();
    }
}
