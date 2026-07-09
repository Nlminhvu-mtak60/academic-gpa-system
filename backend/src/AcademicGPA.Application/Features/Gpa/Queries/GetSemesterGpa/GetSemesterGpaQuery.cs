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

namespace AcademicGPA.Application.Features.Gpa.Queries.GetSemesterGpa;

public record GetSemesterGpaQuery(Guid SemesterId) : IRequest<SemesterGpaDto>;

public class GetSemesterGpaQueryHandler : IRequestHandler<GetSemesterGpaQuery, SemesterGpaDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGpaCalculator _gpaCalculator;

    public GetSemesterGpaQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IGpaCalculator gpaCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<SemesterGpaDto> Handle(GetSemesterGpaQuery request, CancellationToken cancellationToken)
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

        // 3. Load Semester with Courses and Scores
        var semester = await _context.Semesters
            .Include(s => s.Courses.Where(c => !c.IsDeleted))
                .ThenInclude(c => c.Score)
            .FirstOrDefaultAsync(s => s.Id == request.SemesterId 
                && s.AcademicYear.StudentProfileId == profile.Id
                && !s.IsDeleted 
                && !s.AcademicYear.IsDeleted, cancellationToken);

        if (semester == null)
        {
            throw new NotFoundException("Semester", request.SemesterId);
        }

        var activeCourses = semester.Courses.ToList();
        var gradedCourses = activeCourses.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();

        var gpa10 = _gpaCalculator.CalculateGpa10(gradedCourses);
        var gpa4 = _gpaCalculator.CalculateGpa4(gradedCourses);

        int totalCredits = activeCourses.Sum(c => c.Credits);
        int passedCredits = activeCourses.Where(c => c.Score != null && c.Score.IsPass == true).Sum(c => c.Credits);
        int failedCredits = activeCourses.Where(c => c.Score != null && c.Score.IsPass == false).Sum(c => c.Credits);

        return new SemesterGpaDto(
            semester.Id,
            gpa10,
            gpa4,
            totalCredits,
            passedCredits,
            failedCredits
        );
    }
}
