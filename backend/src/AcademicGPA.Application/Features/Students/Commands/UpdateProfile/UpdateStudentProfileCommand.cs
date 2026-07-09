using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Students.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Students.Commands.UpdateProfile;

public record UpdateStudentProfileCommand(
    string StudentCode,
    string UniversityName,
    string MajorName,
    int EnrollmentYear,
    int TotalRequiredCredits
) : IRequest<StudentProfileSubDto>;

public class UpdateStudentProfileCommandValidator : AbstractValidator<UpdateStudentProfileCommand>
{
    public UpdateStudentProfileCommandValidator()
    {
        RuleFor(x => x.StudentCode)
            .NotEmpty().WithMessage("Student Code is required.")
            .MaximumLength(50).WithMessage("Student Code cannot exceed 50 characters.")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Student Code must be alphanumeric.");

        RuleFor(x => x.UniversityName)
            .NotEmpty().WithMessage("University name is required.")
            .MaximumLength(200).WithMessage("University name cannot exceed 200 characters.");

        RuleFor(x => x.MajorName)
            .NotEmpty().WithMessage("Major name is required.")
            .MaximumLength(200).WithMessage("Major name cannot exceed 200 characters.");

        RuleFor(x => x.EnrollmentYear)
            .InclusiveBetween(2000, 2100).WithMessage("Enrollment year must be between 2000 and 2100.");

        RuleFor(x => x.TotalRequiredCredits)
            .InclusiveBetween(30, 300).WithMessage("Required graduation credits must be between 30 and 300.");
    }
}

public class UpdateStudentProfileCommandHandler : IRequestHandler<UpdateStudentProfileCommand, StudentProfileSubDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStudentProfileCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<StudentProfileSubDto> Handle(UpdateStudentProfileCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch current User ID
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        // 2. Fetch or create StudentProfile entity
        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        var isNewProfile = false;
        if (profile == null)
        {
            profile = new StudentProfile
            {
                UserId = userId
            };
            isNewProfile = true;
        }

        // 3. Verify studentCode uniqueness across other student profiles
        var isUnique = await _unitOfWork.Students.IsStudentCodeUniqueAsync(
            request.StudentCode, 
            isNewProfile ? null : profile.Id, 
            cancellationToken);

        if (!isUnique)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("studentCode", "Student Code is already registered to another user.");
        }

        // 4. Update fields
        profile.StudentCode = request.StudentCode;
        profile.UniversityName = request.UniversityName;
        profile.MajorName = request.MajorName;
        profile.EnrollmentYear = request.EnrollmentYear;
        profile.TotalRequiredCredits = request.TotalRequiredCredits;

        if (isNewProfile)
        {
            _context.StudentProfiles.Add(profile);
        }
        else
        {
            _context.StudentProfiles.Update(profile);
        }

        // 5. Save database updates
        await _context.SaveChangesAsync(cancellationToken);

        return new StudentProfileSubDto(
            profile.StudentCode,
            profile.UniversityName,
            profile.MajorName,
            profile.EnrollmentYear,
            profile.TotalRequiredCredits
        );
    }
}
