using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Semesters.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Semesters.Commands.CreateSemester;

public record CreateSemesterCommand(
    Guid AcademicYearId,
    string SemesterName
) : IRequest<SemesterDto>;

public class CreateSemesterCommandValidator : AbstractValidator<CreateSemesterCommand>
{
    public CreateSemesterCommandValidator()
    {
        RuleFor(x => x.SemesterName)
            .NotEmpty().WithMessage("Semester name is required.")
            .MaximumLength(50).WithMessage("Semester name cannot exceed 50 characters.");
    }
}

public class CreateSemesterCommandHandler : IRequestHandler<CreateSemesterCommand, SemesterDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSemesterCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<SemesterDto> Handle(CreateSemesterCommand request, CancellationToken cancellationToken)
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

        // 3. Verify that the AcademicYear exists and belongs to the student profile
        var academicYear = await _unitOfWork.AcademicYears.GetByIdWithOwnerAsync(request.AcademicYearId, profile.Id, cancellationToken);
        if (academicYear == null)
        {
            throw new NotFoundException("AcademicYear", request.AcademicYearId);
        }

        // 4. Verify unique SemesterName per Academic Year
        var isUnique = await _unitOfWork.Semesters.IsSemesterNameUniqueAsync(request.AcademicYearId, request.SemesterName, null, cancellationToken);
        if (!isUnique)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("semesterName", "Semester name must be unique within the academic year.");
        }

        // 5. Verify maximum count of semesters
        var existingSemestersCount = await _unitOfWork.Semesters.CountByAcademicYearIdAsync(request.AcademicYearId, cancellationToken);
        if (existingSemestersCount >= 3)
        {
            throw new UnprocessableEntityException("Maximum of 3 semesters per academic year.");
        }

        // 6. Create Semester entity
        var semester = new Semester
        {
            AcademicYearId = request.AcademicYearId,
            SemesterName = request.SemesterName,
            SortOrder = existingSemestersCount,
            IsDeleted = false
        };

        await _unitOfWork.Semesters.AddAsync(semester, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SemesterDto(
            semester.Id,
            semester.SemesterName,
            semester.SortOrder,
            0,
            0.00m,
            0.00m
        );
    }
}
