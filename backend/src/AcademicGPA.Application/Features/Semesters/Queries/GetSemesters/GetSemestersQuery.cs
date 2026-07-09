using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Semesters.DTOs;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Semesters.Queries.GetSemesters;

public record GetSemestersQuery(Guid AcademicYearId) : IRequest<IReadOnlyList<SemesterDto>>;

public class GetSemestersQueryHandler : IRequestHandler<GetSemestersQuery, IReadOnlyList<SemesterDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGpaCalculator _gpaCalculator;

    public GetSemestersQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IGpaCalculator gpaCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<IReadOnlyList<SemesterDto>> Handle(GetSemestersQuery request, CancellationToken cancellationToken)
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
            return Array.Empty<SemesterDto>();
        }

        // 3. Verify that the AcademicYear exists and belongs to the student profile
        var academicYear = await _unitOfWork.AcademicYears.GetByIdWithOwnerAsync(request.AcademicYearId, profile.Id, cancellationToken);
        if (academicYear == null)
        {
            throw new NotFoundException("AcademicYear", request.AcademicYearId);
        }

        // 4. Fetch semesters
        var semesters = await _unitOfWork.Semesters.GetByAcademicYearIdAsync(request.AcademicYearId, cancellationToken);

        // 5. Map to DTOs
        var results = new List<SemesterDto>();
        foreach (var s in semesters)
        {
            if (s.IsImported)
            {
                results.Add(new SemesterDto(
                    s.Id,
                    s.SemesterName,
                    s.SortOrder,
                    s.ImportedCredits,
                    s.ImportedGpa10,
                    s.ImportedGpa4,
                    s.IsImported,
                    s.ImportedCredits,
                    s.ImportedGpa10,
                    s.ImportedGpa4
                ));
            }
            else
            {
                var semCourses = s.Courses.ToList();
                var bestAttempts = _gpaCalculator.FilterBestAttempts(semCourses).ToList();
                var gradedBestAttempts = bestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();

                var completedCredits = bestAttempts
                    .Where(c => c.Score != null && c.Score.IsPass == true)
                    .Sum(c => c.Credits);

                var gpa10 = _gpaCalculator.CalculateGpa10(gradedBestAttempts) ?? 0.00m;
                var gpa4 = _gpaCalculator.CalculateGpa4(gradedBestAttempts) ?? 0.00m;

                results.Add(new SemesterDto(
                    s.Id,
                    s.SemesterName,
                    s.SortOrder,
                    completedCredits,
                    gpa10,
                    gpa4,
                    s.IsImported,
                    s.ImportedCredits,
                    s.ImportedGpa10,
                    s.ImportedGpa4
                ));
            }
        }
        return results;
    }
}
