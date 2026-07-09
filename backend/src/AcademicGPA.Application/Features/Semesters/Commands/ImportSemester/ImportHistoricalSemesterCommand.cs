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

namespace AcademicGPA.Application.Features.Semesters.Commands.ImportSemester;

public record ImportHistoricalSemesterCommand(
    Guid AcademicYearId,
    string SemesterName,
    int ImportedCredits,
    decimal ImportedGpa10,
    decimal ImportedGpa4
) : IRequest<SemesterDto>;

public class ImportHistoricalSemesterCommandValidator : AbstractValidator<ImportHistoricalSemesterCommand>
{
    public ImportHistoricalSemesterCommandValidator()
    {
        RuleFor(x => x.SemesterName)
            .NotEmpty().WithMessage("Semester name is required.")
            .MaximumLength(50).WithMessage("Semester name cannot exceed 50 characters.");

        RuleFor(x => x.ImportedCredits)
            .GreaterThanOrEqualTo(0).WithMessage("Imported credits cannot be negative.");

        RuleFor(x => x.ImportedGpa10)
            .InclusiveBetween(0.00m, 10.00m).WithMessage("GPA 10 must be between 0.0 and 10.0.");

        RuleFor(x => x.ImportedGpa4)
            .InclusiveBetween(0.00m, 4.00m).WithMessage("GPA 4 must be between 0.0 and 4.0.");
    }
}

public class ImportHistoricalSemesterCommandHandler : IRequestHandler<ImportHistoricalSemesterCommand, SemesterDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ImportHistoricalSemesterCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<SemesterDto> Handle(ImportHistoricalSemesterCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new NotFoundException("StudentProfile", userId);
        }

        var academicYear = await _unitOfWork.AcademicYears.GetByIdWithOwnerAsync(request.AcademicYearId, profile.Id, cancellationToken);
        if (academicYear == null)
        {
            throw new NotFoundException("AcademicYear", request.AcademicYearId);
        }

        var isUnique = await _unitOfWork.Semesters.IsSemesterNameUniqueAsync(request.AcademicYearId, request.SemesterName, null, cancellationToken);
        if (!isUnique)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("semesterName", "Semester name must be unique within the academic year.");
        }

        var existingSemestersCount = await _unitOfWork.Semesters.CountByAcademicYearIdAsync(request.AcademicYearId, cancellationToken);
        if (existingSemestersCount >= 3)
        {
            throw new UnprocessableEntityException("Maximum of 3 semesters per academic year.");
        }

        var semester = new Semester
        {
            AcademicYearId = request.AcademicYearId,
            SemesterName = request.SemesterName,
            SortOrder = existingSemestersCount,
            IsDeleted = false,
            IsImported = true,
            ImportedCredits = request.ImportedCredits,
            ImportedGpa10 = request.ImportedGpa10,
            ImportedGpa4 = request.ImportedGpa4
        };

        await _unitOfWork.Semesters.AddAsync(semester, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SemesterDto(
            semester.Id,
            semester.SemesterName,
            semester.SortOrder,
            semester.ImportedCredits,
            semester.ImportedGpa10,
            semester.ImportedGpa4,
            semester.IsImported,
            semester.ImportedCredits,
            semester.ImportedGpa10,
            semester.ImportedGpa4
        );
    }
}
