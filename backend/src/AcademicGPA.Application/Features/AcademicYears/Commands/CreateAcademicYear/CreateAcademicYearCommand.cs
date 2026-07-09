using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.AcademicYears.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.AcademicYears.Commands.CreateAcademicYear;

public record CreateAcademicYearCommand(
    string YearName,
    int StartYear,
    int EndYear
) : IRequest<AcademicYearDto>;

public class CreateAcademicYearCommandValidator : AbstractValidator<CreateAcademicYearCommand>
{
    public CreateAcademicYearCommandValidator()
    {
        RuleFor(x => x.YearName)
            .NotEmpty().WithMessage("Year name is required.")
            .MaximumLength(20).WithMessage("Year name cannot exceed 20 characters.");

        RuleFor(x => x.StartYear)
            .InclusiveBetween(2000, 2100).WithMessage("Start year must be between 2000 and 2100.");

        RuleFor(x => x.EndYear)
            .InclusiveBetween(2000, 2100).WithMessage("End year must be between 2000 and 2100.")
            .Must((cmd, endYear) => endYear >= cmd.StartYear && endYear <= cmd.StartYear + 1)
            .WithMessage("End year must be equal to start year or start year + 1.");
    }
}

public class CreateAcademicYearCommandHandler : IRequestHandler<CreateAcademicYearCommand, AcademicYearDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAcademicYearCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AcademicYearDto> Handle(CreateAcademicYearCommand request, CancellationToken cancellationToken)
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

        // 3. Verify unique YearName per Student Profile
        var isUnique = await _unitOfWork.AcademicYears.IsYearNameUniqueAsync(profile.Id, request.YearName, null, cancellationToken);
        if (!isUnique)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("yearName", "Academic year name must be unique.");
        }

        // 4. Default SortOrder calculation (fetch count of active academic years)
        var existingYearsCount = await _context.AcademicYears
            .CountAsync(ay => ay.StudentProfileId == profile.Id && !ay.IsDeleted, cancellationToken);

        // 5. Create AcademicYear entity
        var academicYear = new AcademicYear
        {
            StudentProfileId = profile.Id,
            YearName = request.YearName,
            StartYear = request.StartYear,
            EndYear = request.EndYear,
            StartDate = new DateTime(request.StartYear, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(request.EndYear, 6, 30, 0, 0, 0, DateTimeKind.Utc),
            Status = "Completed",
            IsCurrent = false,
            SortOrder = existingYearsCount,
            IsDeleted = false
        };

        await _unitOfWork.AcademicYears.AddAsync(academicYear, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AcademicYearDto(
            academicYear.Id,
            academicYear.YearName,
            academicYear.StartYear,
            academicYear.EndYear,
            academicYear.Status,
            academicYear.IsCurrent,
            0,
            0.00m,
            0.00m
        );
    }
}
