using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Courses.DTOs;
using AcademicGPA.Application.Features.Scores.DTOs;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Courses.Queries.GetCourses;

public record GetCoursesQuery(Guid SemesterId) : IRequest<IReadOnlyList<CourseDto>>;

public class GetCoursesQueryHandler : IRequestHandler<GetCoursesQuery, IReadOnlyList<CourseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public GetCoursesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
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
            return Array.Empty<CourseDto>();
        }

        // 3. Verify that the Semester exists and belongs to the student profile
        var semester = await _unitOfWork.Semesters.GetByIdWithOwnerAsync(request.SemesterId, profile.Id, cancellationToken);
        if (semester == null)
        {
            throw new NotFoundException("Semester", request.SemesterId);
        }

        // 4. Fetch courses
        var courses = await _unitOfWork.Courses.GetBySemesterIdAsync(request.SemesterId, cancellationToken);

        // 5. Map to DTOs
        return courses.Select(c => new CourseDto(
            c.Id,
            c.CourseCode,
            c.CourseName,
            c.Credits,
            c.IsRetake,
            c.OriginalCourseId,
            c.Score != null ? new ScoreDto(
                c.Score.AttendanceScore,
                c.Score.ContinuousScore,
                c.Score.FinalExamScore,
                c.Score.CourseScore,
                c.Score.LetterGrade,
                c.Score.Gpa4Value,
                c.Score.AcademicClassification,
                c.Score.IsPass,
                c.Score.UpdatedAt
            ) : null
        )).ToList();
    }
}
