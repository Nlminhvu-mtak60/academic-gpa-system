using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Features.AcademicYears.Commands.CreateAcademicYear;
using AcademicGPA.Application.Features.AcademicYears.Commands.UpdateAcademicYear;
using AcademicGPA.Application.Features.AcademicYears.Commands.DeleteAcademicYear;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcademicGPA.UnitTests;

public class AcademicYearIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task AcademicYear_FullLifecycleIntegrationTest()
    {
        // 1. Arrange Command Handlers
        var unitOfWork = new UnitOfWork(Context);
        var createHandler = new CreateAcademicYearCommandHandler(Context, MockCurrentUserService.Object, unitOfWork);
        var updateHandler = new UpdateAcademicYearCommandHandler(Context, MockCurrentUserService.Object, unitOfWork);
        var deleteHandler = new DeleteAcademicYearCommandHandler(Context, MockCurrentUserService.Object, unitOfWork);

        // 2. Create Academic Year
        var createCommand = new CreateAcademicYearCommand("2024-2025", 2024, 2025);
        var createResult = await createHandler.Handle(createCommand, CancellationToken.None);

        createResult.Should().NotBeNull();
        createResult.YearName.Should().Be("2024-2025");
        
        var createdYear = await Context.AcademicYears.FindAsync(createResult.Id);
        createdYear.Should().NotBeNull();
        createdYear!.StudentProfileId.Should().Be(CurrentStudentProfileId);

        // 3. Update Academic Year
        var updateCommand = new UpdateAcademicYearCommand(createResult.Id, "2024-2025 Updated", 2024, 2025);
        await updateHandler.Handle(updateCommand, CancellationToken.None);

        var updatedYear = await Context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(y => y.Id == createResult.Id);
        updatedYear.Should().NotBeNull();
        updatedYear!.YearName.Should().Be("2024-2025 Updated");

        // 4. Try to Delete when it has a Semester (should throw ValidationException)
        var semester = new Semester
        {
            Id = Guid.NewGuid(),
            AcademicYearId = createResult.Id,
            SemesterName = "Semester 1",
            SortOrder = 1,
            IsDeleted = false
        };
        Context.Semesters.Add(semester);
        await Context.SaveChangesAsync();

        var actDeleteWithSemester = () => deleteHandler.Handle(new DeleteAcademicYearCommand(createResult.Id), CancellationToken.None);
        await actDeleteWithSemester.Should().ThrowAsync<ValidationException>();

        // 5. Delete after removing the semester (should mark IsDeleted as true)
        Context.Semesters.Remove(semester);
        await Context.SaveChangesAsync();

        await deleteHandler.Handle(new DeleteAcademicYearCommand(createResult.Id), CancellationToken.None);

        var deletedYear = await Context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(y => y.Id == createResult.Id);
        deletedYear.Should().NotBeNull();
        deletedYear!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAcademicYear_WithDuplicateName_ShouldThrowValidationException()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(Context);
        var createHandler = new CreateAcademicYearCommandHandler(Context, MockCurrentUserService.Object, unitOfWork);

        var command = new CreateAcademicYearCommand("2025-2026", 2025, 2026);
        await createHandler.Handle(command, CancellationToken.None);

        // Act
        var act = () => createHandler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }
}
