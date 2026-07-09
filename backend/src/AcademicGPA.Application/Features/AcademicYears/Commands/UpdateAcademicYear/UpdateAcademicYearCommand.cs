using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.AcademicYears.Commands.UpdateAcademicYear;

public record UpdateAcademicYearCommand(
    Guid Id,
    string YearName,
    int StartYear,
    int EndYear,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? ImportedCredits = null,
    decimal? ImportedGpa10 = null,
    decimal? ImportedGpa4 = null
) : IRequest<Unit>;

public class UpdateAcademicYearCommandValidator : AbstractValidator<UpdateAcademicYearCommand>
{
    public UpdateAcademicYearCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Academic year ID is required.");

        RuleFor(x => x.YearName)
            .NotEmpty().WithMessage("Year name is required.")
            .MaximumLength(20).WithMessage("Year name cannot exceed 20 characters.");

        RuleFor(x => x.StartYear)
            .InclusiveBetween(2000, 2100).WithMessage("Start year must be between 2000 and 2100.");

        RuleFor(x => x.EndYear)
            .InclusiveBetween(2000, 2100).WithMessage("End year must be between 2000 and 2100.")
            .Must((cmd, endYear) => endYear >= cmd.StartYear && endYear <= cmd.StartYear + 1)
            .WithMessage("End year must be equal to start year or start year + 1.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate.GetValueOrDefault())
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be greater than or equal to start date.");
    }
}

public class UpdateAcademicYearCommandHandler : IRequestHandler<UpdateAcademicYearCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAcademicYearCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateAcademicYearCommand request, CancellationToken cancellationToken)
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

        // 3. Fetch the Academic Year owned by student
        var academicYear = await _unitOfWork.AcademicYears.GetByIdWithOwnerAsync(request.Id, profile.Id, cancellationToken);
        if (academicYear == null)
        {
            throw new NotFoundException("AcademicYear", request.Id);
        }

        // 4. Verify unique YearName excluding this ID
        var isUnique = await _unitOfWork.AcademicYears.IsYearNameUniqueAsync(profile.Id, request.YearName, request.Id, cancellationToken);
        if (!isUnique)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("yearName", "Academic year name must be unique.");
        }

        // 5. Update fields
        academicYear.YearName = request.YearName;
        academicYear.StartYear = request.StartYear;
        academicYear.EndYear = request.EndYear;
        academicYear.StartDate = request.StartDate ?? new DateTime(request.StartYear, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        academicYear.EndDate = request.EndDate ?? new DateTime(request.EndYear, 6, 30, 0, 0, 0, DateTimeKind.Utc);

        if (academicYear.IsImported)
        {
            if (request.ImportedCredits.HasValue)
                academicYear.ImportedCredits = request.ImportedCredits.Value;
            if (request.ImportedGpa10.HasValue)
                academicYear.ImportedGpa10 = request.ImportedGpa10.Value;
            if (request.ImportedGpa4.HasValue)
                academicYear.ImportedGpa4 = request.ImportedGpa4.Value;
        }

        _unitOfWork.AcademicYears.Update(academicYear);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
