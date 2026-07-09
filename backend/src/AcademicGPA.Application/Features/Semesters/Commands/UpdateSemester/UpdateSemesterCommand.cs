using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Semesters.Commands.UpdateSemester;

public record UpdateSemesterCommand(
    Guid Id,
    string SemesterName,
    int? ImportedCredits = null,
    decimal? ImportedGpa10 = null,
    decimal? ImportedGpa4 = null
) : IRequest;

public class UpdateSemesterCommandValidator : AbstractValidator<UpdateSemesterCommand>
{
    public UpdateSemesterCommandValidator()
    {
        RuleFor(x => x.SemesterName)
            .NotEmpty().WithMessage("Semester name is required.")
            .MaximumLength(50).WithMessage("Semester name cannot exceed 50 characters.");
    }
}

public class UpdateSemesterCommandHandler : IRequestHandler<UpdateSemesterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSemesterCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateSemesterCommand request, CancellationToken cancellationToken)
    {
        // 1. Get current User ID
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // 2. Fetch Student Profile
        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("StudentProfile", userId);
        }

        // 3. Fetch Semester and check ownership
        var semester = await _unitOfWork.Semesters.GetByIdWithOwnerAsync(request.Id, profile.Id, cancellationToken);
        if (semester == null)
        {
            throw new NotFoundException("Semester", request.Id);
        }

        // 4. Check uniqueness of SemesterName in the Academic Year (excluding the current semester)
        var isUnique = await _unitOfWork.Semesters.IsSemesterNameUniqueAsync(semester.AcademicYearId, request.SemesterName, semester.Id, cancellationToken);
        if (!isUnique)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("semesterName", "Semester name must be unique within the academic year.");
        }

        // 5. Update semester
        semester.SemesterName = request.SemesterName;

        if (semester.IsImported)
        {
            if (request.ImportedCredits.HasValue)
                semester.ImportedCredits = request.ImportedCredits.Value;
            if (request.ImportedGpa10.HasValue)
                semester.ImportedGpa10 = request.ImportedGpa10.Value;
            if (request.ImportedGpa4.HasValue)
                semester.ImportedGpa4 = request.ImportedGpa4.Value;
        }

        _unitOfWork.Semesters.Update(semester);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
