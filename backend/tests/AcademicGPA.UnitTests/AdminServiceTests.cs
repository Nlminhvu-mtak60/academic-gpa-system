using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Admin.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using AcademicGPA.Infrastructure.Persistence;
using AcademicGPA.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcademicGPA.UnitTests;

public class AdminServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public AdminServiceTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private ApplicationDbContext CreateContext() => new(_options);

    private class MockPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => "hashed_" + password;
        public bool VerifyPassword(string password, string hashedPassword) => hashedPassword == "hashed_" + password;
    }

    private async Task SeedDataAsync(ApplicationDbContext context)
    {
        // Add Admin
        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@test.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            IsActive = true
        };
        context.Users.Add(admin);

        // Student 1 (Excellent)
        var student1 = new User
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@student.com",
            PasswordHash = "hash",
            Role = UserRole.Student,
            IsActive = true
        };
        context.Users.Add(student1);

        var profile1 = new StudentProfile
        {
            UserId = student1.Id,
            StudentCode = "STUD001",
            UniversityName = "VNU",
            MajorName = "CS",
            EnrollmentYear = 2022,
            TotalRequiredCredits = 120
        };
        context.StudentProfiles.Add(profile1);

        var ay1 = new AcademicYear { StudentProfile = profile1, YearName = "2022-2023" };
        var sem1 = new Semester { AcademicYear = ay1, SemesterName = "Semester 1", SortOrder = 1 };
        var courseA = new Course { Semester = sem1, CourseName = "CS101", Credits = 3 };
        var scoreA = new Score
        {
            Course = courseA,
            AttendanceScore = 10,
            ContinuousScore = 10,
            FinalExamScore = 10,
            CourseScore = 10,
            Gpa4Value = 4.0m,
            LetterGrade = "A+",
            IsPass = true
        };
        context.Scores.Add(scoreA);

        // Student 2 (Good)
        var student2 = new User
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FirstName = "Bob",
            LastName = "Jones",
            Email = "bob@student.com",
            PasswordHash = "hash",
            Role = UserRole.Student,
            IsActive = true
        };
        context.Users.Add(student2);

        var profile2 = new StudentProfile
        {
            UserId = student2.Id,
            StudentCode = "STUD002",
            UniversityName = "VNU",
            MajorName = "CS",
            EnrollmentYear = 2022,
            TotalRequiredCredits = 120
        };
        context.StudentProfiles.Add(profile2);

        var ay2 = new AcademicYear { StudentProfile = profile2, YearName = "2022-2023" };
        var sem2 = new Semester { AcademicYear = ay2, SemesterName = "Semester 1", SortOrder = 1 };
        var courseB = new Course { Semester = sem2, CourseName = "MATH101", Credits = 2 };
        var scoreB = new Score
        {
            Course = courseB,
            AttendanceScore = 8,
            ContinuousScore = 8,
            FinalExamScore = 8,
            CourseScore = 8,
            Gpa4Value = 3.5m,
            LetterGrade = "B+",
            IsPass = true
        };
        context.Scores.Add(scoreB);

        // Locked Student
        var student3 = new User
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            FirstName = "Charlie",
            LastName = "Brown",
            Email = "charlie@student.com",
            PasswordHash = "hash",
            Role = UserRole.Student,
            IsActive = false
        };
        context.Users.Add(student3);

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetStatisticsAsync_ShouldComputeSystemMetrics()
    {
        using var context = CreateContext();
        await SeedDataAsync(context);

        var service = new AdminService(context, new MockPasswordHasher());

        var stats = await service.GetStatisticsAsync(CancellationToken.None);

        stats.Should().NotBeNull();
        stats.UserStats.TotalStudents.Should().Be(3);
        stats.UserStats.ActiveStudents.Should().Be(2);
        stats.UserStats.LockedAccounts.Should().Be(1);

        stats.AcademicOverview.SystemAverageGpa10.Should().Be(9.0m); // (10*3 + 8*2)/5 = 38/5 = 7.6? Wait!
        // Alice GPA = 10, Bob GPA = 8. Alice GPA4 = 4.0, Bob GPA4 = 3.5.
        // SystemAverageGpa10 = (10 + 8) / 2 = 9.0.
        // SystemAverageGpa4 = (4.0 + 3.5) / 2 = 3.75.
        stats.AcademicOverview.SystemAverageGpa4.Should().Be(3.75m);
        stats.AcademicOverview.TotalCreditsEarned.Should().Be(5); // 3 + 2

        stats.GpaDistribution.Excellent.Should().Be(1);
        stats.GpaDistribution.VeryGood.Should().Be(1);
        stats.GpaDistribution.Good.Should().Be(0);
    }

    [Fact]
    public async Task LockStudentAsync_ShouldDeactivateAccount_AndRevokeTokens_AndAudit()
    {
        using var context = CreateContext();
        await SeedDataAsync(context);

        var studentId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Add refresh token for Student 1
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = studentId,
            Token = "token123",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1"
        });
        await context.SaveChangesAsync();

        var service = new AdminService(context, new MockPasswordHasher());

        await service.LockStudentAsync(studentId, "Suspected plagiarism", "192.168.1.100", CancellationToken.None);

        var updatedStudent = await context.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Id == studentId);
        updatedStudent!.IsActive.Should().BeFalse();
        updatedStudent.LockReason.Should().Be("Suspected plagiarism");
        updatedStudent.LockedAt.Should().NotBeNull();
        updatedStudent.RefreshTokens.First().IsActive.Should().BeFalse();

        var logs = await context.UserActivityLogs.ToListAsync();
        logs.Should().ContainSingle();
        logs.First().UserId.Should().Be(studentId);
        logs.First().Activity.Should().Contain("Locked Account");
        logs.First().IpAddress.Should().Be("192.168.1.100");
    }

    [Fact]
    public async Task UnlockStudentAsync_ShouldReactivateAccount_AndAudit()
    {
        using var context = CreateContext();
        await SeedDataAsync(context);

        var lockedId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var service = new AdminService(context, new MockPasswordHasher());

        await service.UnlockStudentAsync(lockedId, "192.168.1.100", CancellationToken.None);

        var updatedStudent = await context.Users.FindAsync(lockedId);
        updatedStudent!.IsActive.Should().BeTrue();
        updatedStudent.LockReason.Should().BeNull();
        updatedStudent.LockedAt.Should().BeNull();

        var logs = await context.UserActivityLogs.ToListAsync();
        logs.Should().ContainSingle();
        logs.First().UserId.Should().Be(lockedId);
        logs.First().Activity.Should().Be("Unlocked Account");
    }

    [Fact]
    public async Task ResetStudentPasswordAsync_ShouldSetTempPassword_AndForceChange_AndAudit()
    {
        using var context = CreateContext();
        await SeedDataAsync(context);

        var studentId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var service = new AdminService(context, new MockPasswordHasher());

        var tempPassword = await service.ResetStudentPasswordAsync(studentId, "192.168.1.100", CancellationToken.None);

        tempPassword.Should().NotBeNullOrEmpty();
        tempPassword.Length.Should().Be(12);

        var updatedStudent = await context.Users.FindAsync(studentId);
        updatedStudent!.ForcePasswordChange.Should().BeTrue();
        updatedStudent.PasswordHash.Should().Be("hashed_" + tempPassword);

        var logs = await context.UserActivityLogs.ToListAsync();
        logs.Should().ContainSingle();
        logs.First().UserId.Should().Be(studentId);
        logs.First().Activity.Should().Be("Reset Password to Temporary Key");
    }

    [Fact]
    public async Task DeleteStudentAsync_ShouldSoftDelete_AndAudit()
    {
        using var context = CreateContext();
        await SeedDataAsync(context);

        var studentId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var service = new AdminService(context, new MockPasswordHasher());

        await service.DeleteStudentAsync(studentId, "192.168.1.100", CancellationToken.None);

        // Note: DbContext global query filter will hide the user from typical queries,
        // so let's bypass it using IgnoreQueryFilters()
        var deletedStudent = await context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == studentId);
        deletedStudent!.IsDeleted.Should().BeTrue();
        deletedStudent.IsActive.Should().BeFalse();

        var logs = await context.UserActivityLogs.ToListAsync();
        logs.Should().ContainSingle();
        logs.First().UserId.Should().Be(studentId);
        logs.First().Activity.Should().Be("Soft Deleted Account");
    }

    [Fact]
    public async Task BroadcastNotificationAsync_ShouldCreateNotificationsForAllActiveStudents()
    {
        using var context = CreateContext();
        await SeedDataAsync(context);

        var adminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var service = new AdminService(context, new MockPasswordHasher());

        await service.BroadcastNotificationAsync(adminId, "Maintenance", "System down at midnight", CancellationToken.None);

        var notifications = await context.Notifications.ToListAsync();
        // 1 history record linked to admin + 2 records for the active students (Alice and Bob)
        notifications.Count.Should().Be(3);

        var aliceNotifications = notifications.Where(n => n.UserId == Guid.Parse("22222222-2222-2222-2222-222222222222")).ToList();
        aliceNotifications.Should().ContainSingle();
        aliceNotifications.First().Title.Should().Be("Maintenance");
        aliceNotifications.First().Message.Should().Be("System down at midnight");
        aliceNotifications.First().IsRead.Should().BeFalse();
        aliceNotifications.First().SenderId.Should().Be(adminId);

        var charlieNotifications = notifications.Where(n => n.UserId == Guid.Parse("44444444-4444-4444-4444-444444444444")).ToList();
        charlieNotifications.Should().BeEmpty(); // Charlie is inactive/locked
    }
}
