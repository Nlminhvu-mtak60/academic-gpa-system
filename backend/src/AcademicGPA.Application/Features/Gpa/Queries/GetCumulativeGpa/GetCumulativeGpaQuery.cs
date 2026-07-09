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

namespace AcademicGPA.Application.Features.Gpa.Queries.GetCumulativeGpa;

public record GetCumulativeGpaQuery : IRequest<CumulativeGpaDto>;

public class GetCumulativeGpaQueryHandler : IRequestHandler<GetCumulativeGpaQuery, CumulativeGpaDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGpaCalculator _gpaCalculator;

    public GetCumulativeGpaQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IGpaCalculator gpaCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<CumulativeGpaDto> Handle(GetCumulativeGpaQuery request, CancellationToken cancellationToken)
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

        // 3. Load all Academic Years, Semesters, Courses, and Scores
        var academicYears = await _context.AcademicYears
            .Include(ay => ay.Semesters.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Courses.Where(c => !c.IsDeleted))
                    .ThenInclude(c => c.Score)
            .Where(ay => ay.StudentProfileId == profile.Id && !ay.IsDeleted)
            .ToListAsync(cancellationToken);

        var allCourses = academicYears
            .SelectMany(ay => ay.Semesters)
            .SelectMany(s => s.Courses)
            .ToList();

        // 4. Filter to best attempts (deduplicating retakes)
        var bestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();
        var gradedBestAttempts = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();

        // 5. Calculate GPAs
        var cumulativeGpa10 = _gpaCalculator.CalculateGpa10(gradedBestAttempts);
        var cumulativeGpa4 = _gpaCalculator.CalculateGpa4(gradedBestAttempts);

        // 6. Calculate credit progress
        int totalCreditsCompleted = bestAttempts
            .Where(c => c.Score != null && c.Score.IsPass == true)
            .Sum(c => c.Credits);

        int totalCreditsRequired = profile.TotalRequiredCredits;
        decimal completionPercentage = 0;
        if (totalCreditsRequired > 0)
        {
            completionPercentage = Math.Round((decimal)totalCreditsCompleted / totalCreditsRequired * 100, 2, MidpointRounding.AwayFromZero);
        }

        return new CumulativeGpaDto(
            cumulativeGpa10,
            cumulativeGpa4,
            totalCreditsCompleted,
            totalCreditsRequired,
            completionPercentage
        );
    }
}
