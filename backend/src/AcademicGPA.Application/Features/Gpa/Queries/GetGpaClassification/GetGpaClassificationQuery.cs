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

namespace AcademicGPA.Application.Features.Gpa.Queries.GetGpaClassification;

public record GetGpaClassificationQuery : IRequest<GpaClassificationDto>;

public class GetGpaClassificationQueryHandler : IRequestHandler<GetGpaClassificationQuery, GpaClassificationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGpaCalculator _gpaCalculator;

    public GetGpaClassificationQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IGpaCalculator gpaCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<GpaClassificationDto> Handle(GetGpaClassificationQuery request, CancellationToken cancellationToken)
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

        // 4. Calculate Cumulative GPA
        var bestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();
        var gradedBestAttempts = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
        var cumulativeGpa10 = _gpaCalculator.CalculateGpa10(gradedBestAttempts);

        // 5. Determine Classification according to thresholds
        string classEn = "Poor";
        string classVn = "Kém";
        decimal minThreshold = 0.0m;

        if (cumulativeGpa10.HasValue)
        {
            var gpa = cumulativeGpa10.Value;
            if (gpa >= 9.0m)
            {
                classEn = "Excellent";
                classVn = "Xuất sắc";
                minThreshold = 9.0m;
            }
            else if (gpa >= 8.0m)
            {
                classEn = "Very Good";
                classVn = "Giỏi";
                minThreshold = 8.0m;
            }
            else if (gpa >= 7.0m)
            {
                classEn = "Good";
                classVn = "Khá";
                minThreshold = 7.0m;
            }
            else if (gpa >= 6.5m)
            {
                classEn = "Average Good";
                classVn = "Trung bình khá";
                minThreshold = 6.5m;
            }
            else if (gpa >= 5.0m)
            {
                classEn = "Average";
                classVn = "Trung bình";
                minThreshold = 5.0m;
            }
            else if (gpa >= 4.0m)
            {
                classEn = "Weak";
                classVn = "Yếu";
                minThreshold = 4.0m;
            }
            else
            {
                classEn = "Poor";
                classVn = "Kém";
                minThreshold = 0.0m;
            }
        }

        return new GpaClassificationDto(
            cumulativeGpa10,
            classEn,
            classVn,
            minThreshold
        );
    }
}
