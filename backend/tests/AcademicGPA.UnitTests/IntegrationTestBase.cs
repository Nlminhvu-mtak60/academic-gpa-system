using System;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Enums;
using AcademicGPA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AcademicGPA.UnitTests;

public class IntegrationTestBase : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<ICurrentUserService> MockCurrentUserService;
    protected Guid CurrentUserId;
    protected Guid CurrentStudentProfileId;

    public IntegrationTestBase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
        MockCurrentUserService = new Mock<ICurrentUserService>();
        
        SeedDatabase().GetAwaiter().GetResult();
    }

    private async Task SeedDatabase()
    {
        CurrentUserId = Guid.NewGuid();
        CurrentStudentProfileId = Guid.NewGuid();

        var user = new User
        {
            Id = CurrentUserId,
            Email = "student.integration@school.edu.vn",
            FirstName = "Integration",
            LastName = "Student",
            Role = UserRole.Student,
            PasswordHash = "hashedPassword"
        };

        var profile = new StudentProfile
        {
            Id = CurrentStudentProfileId,
            UserId = CurrentUserId,
            StudentCode = "SE123456",
            UniversityName = "Integration University",
            MajorName = "Software Engineering",
            EnrollmentYear = 2024,
            TotalRequiredCredits = 120,
            User = user
        };

        Context.Users.Add(user);
        Context.StudentProfiles.Add(profile);
        await Context.SaveChangesAsync();

        MockCurrentUserService.Setup(s => s.UserId).Returns(CurrentUserId.ToString());
        MockCurrentUserService.Setup(s => s.Email).Returns(user.Email);
        MockCurrentUserService.Setup(s => s.Role).Returns(UserRole.Student.ToString());
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}
