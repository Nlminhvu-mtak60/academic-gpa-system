using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Courses.DTOs;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Courses.Queries.GetEligibleOriginalCourses;

public record GetEligibleOriginalCoursesQuery(string CourseCode) : IRequest<IReadOnlyList<CourseDto>>;

public class GetEligibleOriginalCoursesQueryHandler : IRequestHandler<GetEligibleOriginalCoursesQuery, IReadOnlyList<CourseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public GetEligibleOriginalCoursesQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CourseDto>> Handle(GetEligibleOriginalCoursesQuery request, CancellationToken cancellationToken)
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

        // 3. Fetch courses
        var courses = await _unitOfWork.Courses.GetEligibleOriginalCoursesAsync(profile.Id, request.CourseCode, cancellationToken);

        // 4. Map to DTOs
        return courses.Select(c => new CourseDto(
            c.Id,
            c.CourseCode,
            c.CourseName,
            c.Credits,
            c.IsRetake,
            c.OriginalCourseId
        )).ToList();
    }
}
