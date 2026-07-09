using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Courses.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Courses.Commands.CreateCourse;

public record CreateCourseCommand(
    Guid SemesterId,
    string CourseCode,
    string CourseName,
    int Credits,
    bool IsRetake,
    Guid? OriginalCourseId
) : IRequest<CourseDto>;

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
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

public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, CourseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCourseCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CourseDto> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
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

        // 3. Verify that the Semester exists and belongs to the student profile
        var semester = await _unitOfWork.Semesters.GetByIdWithOwnerAsync(request.SemesterId, profile.Id, cancellationToken);
        if (semester == null)
        {
            throw new NotFoundException("Semester", request.SemesterId);
        }

        // 4. Verify unique CourseName per Semester
        var isUnique = await _unitOfWork.Courses.IsCourseNameUniqueAsync(request.SemesterId, request.CourseName, null, cancellationToken);
        if (!isUnique)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("courseName", "Course name must be unique within the semester.");
        }

        // 5. Validate original course reference for retakes
        Guid? originalCourseId = null;
        if (request.IsRetake && request.OriginalCourseId.HasValue)
        {
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

        // 6. Create Course entity
        var course = new Course
        {
            SemesterId = request.SemesterId,
            CourseCode = request.CourseCode.ToUpperInvariant(),
            CourseName = request.CourseName,
            Credits = request.Credits,
            IsRetake = request.IsRetake && originalCourseId.HasValue,
            OriginalCourseId = originalCourseId,
            IsDeleted = false
        };

        await _unitOfWork.Courses.AddAsync(course, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CourseDto(
            course.Id,
            course.CourseCode,
            course.CourseName,
            course.Credits,
            course.IsRetake,
            course.OriginalCourseId
        );
    }
}
