using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Courses.Commands.UpdateCourse;

public record UpdateCourseCommand(
    Guid Id,
    string CourseCode,
    string CourseName,
    int Credits,
    bool IsRetake,
    Guid? OriginalCourseId
) : IRequest;

public class UpdateCourseCommandValidator : AbstractValidator<UpdateCourseCommand>
{
    public UpdateCourseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Course ID is required.");

        RuleFor(x => x.CourseCode)
            .NotEmpty().WithMessage("Course code is required.")
            .MaximumLength(20).WithMessage("Course code cannot exceed 20 characters.")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Course code must be alphanumeric.");

        RuleFor(x => x.CourseName)
            .NotEmpty().WithMessage("Course name is required.")
            .MaximumLength(200).WithMessage("Course name cannot exceed 200 characters.");

        RuleFor(x => x.Credits)
            .InclusiveBetween(1, 6).WithMessage("Credits must be between 1 and 6.");

        RuleFor(x => x.OriginalCourseId)
            .NotEmpty().When(x => x.IsRetake).WithMessage("Original course is required when marking as retake.");
    }
}

public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCourseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
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

        // 3. Fetch Course and check ownership
        var course = await _unitOfWork.Courses.GetByIdWithOwnerAsync(request.Id, profile.Id, cancellationToken);
        if (course == null)
        {
            throw new NotFoundException("Course", request.Id);
        }

        // 4. Verify unique CourseName in the Semester (excluding current Course ID)
        var isUnique = await _unitOfWork.Courses.IsCourseNameUniqueAsync(course.SemesterId, request.CourseName, course.Id, cancellationToken);
        if (!isUnique)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("courseName", "Course name must be unique within the semester.");
        }

        // 5. Validate original course reference for retakes
        Guid? originalCourseId = null;
        if (request.IsRetake && request.OriginalCourseId.HasValue)
        {
            // Do not allow self-referencing retakes!
            if (request.OriginalCourseId.Value == course.Id)
            {
                throw new AcademicGPA.Application.Common.Exceptions.ValidationException("originalCourseId", "A course cannot be a retake of itself.");
            }

            var isValidOriginal = await _unitOfWork.Courses.IsValidOriginalCourseAsync(
                request.OriginalCourseId.Value, 
                profile.Id, 
                request.CourseCode, 
                cancellationToken);

            if (!isValidOriginal)
            {
                throw new AcademicGPA.Application.Common.Exceptions.ValidationException("originalCourseId", "Invalid original course selected for retake.");
            }

            originalCourseId = request.OriginalCourseId;
        }

        // 6. Update Course properties
        course.CourseCode = request.CourseCode.ToUpperInvariant();
        course.CourseName = request.CourseName;
        course.Credits = request.Credits;
        course.IsRetake = request.IsRetake && originalCourseId.HasValue;
        course.OriginalCourseId = originalCourseId;
        course.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Courses.Update(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
