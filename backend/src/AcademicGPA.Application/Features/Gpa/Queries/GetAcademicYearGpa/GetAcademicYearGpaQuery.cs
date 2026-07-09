using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Gpa.DTOs;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Gpa.Queries.GetAcademicYearGpa;

public record GetAcademicYearGpaQuery(Guid AcademicYearId) : IRequest<AcademicYearGpaDto>;

public class GetAcademicYearGpaQueryHandler : IRequestHandler<GetAcademicYearGpaQuery, AcademicYearGpaDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGpaCalculator _gpaCalculator;

    public GetAcademicYearGpaQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IGpaCalculator gpaCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<AcademicYearGpaDto> Handle(GetAcademicYearGpaQuery request, CancellationToken cancellationToken)
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

        // 3. Load Academic Year with Semesters, Courses and Scores
        var year = await _context.AcademicYears
            .Include(ay => ay.Semesters.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Score)
            .FirstOrDefaultAsync(ay => ay.Id == request.AcademicYearId 
                && ay.StudentProfileId == profile.Id
                && !ay.IsDeleted, cancellationToken);

        if (year == null)
        {
            throw new NotFoundException("AcademicYear", request.AcademicYearId);
        }

        var allCourses = year.Semesters.SelectMany(s => s.Courses).ToList();
        var gradedCourses = allCourses.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();

        var gpa10 = _gpaCalculator.CalculateGpa10(gradedCourses);
        var gpa4 = _gpaCalculator.CalculateGpa4(gradedCourses);

        int totalCredits = allCourses.Sum(c => c.Credits);
        int passedCredits = allCourses.Where(c => c.Score != null && c.Score.IsPass == true).Sum(c => c.Credits);
        int failedCredits = allCourses.Where(c => c.Score != null && c.Score.IsPass == false).Sum(c => c.Credits);

        return new AcademicYearGpaDto(
            year.Id,
            gpa10,
            gpa4,
            totalCredits,
            passedCredits,
            failedCredits
        );
    }
}
