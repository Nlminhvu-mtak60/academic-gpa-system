using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.AcademicYears.DTOs;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.AcademicYears.Queries.GetAcademicYears;

public record GetAcademicYearsQuery : IRequest<IReadOnlyList<AcademicYearDto>>;

public class GetAcademicYearsQueryHandler : IRequestHandler<GetAcademicYearsQuery, IReadOnlyList<AcademicYearDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGpaCalculator _gpaCalculator;

    public GetAcademicYearsQueryHandler(
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

    public async Task<IReadOnlyList<AcademicYearDto>> Handle(GetAcademicYearsQuery request, CancellationToken cancellationToken)
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
            // If student profile hasn't been created yet, return empty list
            return Array.Empty<AcademicYearDto>();
        }

        // 3. Fetch academic years
        var academicYears = await _unitOfWork.AcademicYears.GetByStudentProfileIdAsync(profile.Id, cancellationToken);

        // 4. Map to DTOs
        var results = new List<AcademicYearDto>();
        foreach (var ay in academicYears)
        {
            if (ay.IsImported)
            {
                results.Add(new AcademicYearDto(
                    ay.Id,
                    ay.YearName,
                    ay.StartYear,
                    ay.EndYear,
                    ay.Status,
                    ay.IsCurrent,
                    ay.ImportedCredits,
                    ay.ImportedGpa10,
                    ay.ImportedGpa4,
                    ay.IsImported,
                    ay.ImportedCredits,
                    ay.ImportedGpa10,
                    ay.ImportedGpa4
                ));
            }
            else
            {
                decimal totalGpa10Weight = 0m;
                decimal totalGpa4Weight = 0m;
                int totalGradedCredits = 0;
                int totalCompletedCredits = 0;

                foreach (var sem in ay.Semesters)
                {
                    if (sem.IsImported)
                    {
                        totalGpa10Weight += sem.ImportedGpa10 * sem.ImportedCredits;
                        totalGpa4Weight += sem.ImportedGpa4 * sem.ImportedCredits;
                        totalGradedCredits += sem.ImportedCredits;
                        totalCompletedCredits += sem.ImportedCredits;
                    }
                    else
                    {
                        var semCourses = sem.Courses.ToList();
                        var semBestAttempts = _gpaCalculator.FilterBestAttempts(semCourses).ToList();
                        var semGraded = semBestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
                        
                        var semCompleted = semBestAttempts.Where(c => c.Score != null && c.Score.IsPass == true).Sum(c => c.Credits);
                        totalCompletedCredits += semCompleted;

                        var semGpa10 = _gpaCalculator.CalculateGpa10(semGraded) ?? 0.00m;
                        var semGpa4 = _gpaCalculator.CalculateGpa4(semGraded) ?? 0.00m;
                        var semGradedCredits = semGraded.Sum(c => c.Credits);

                        totalGpa10Weight += semGpa10 * semGradedCredits;
                        totalGpa4Weight += semGpa4 * semGradedCredits;
                        totalGradedCredits += semGradedCredits;
                    }
                }

                var completedCredits = totalCompletedCredits;
                var gpa10 = totalGradedCredits > 0 ? Math.Round(totalGpa10Weight / totalGradedCredits, 2, MidpointRounding.AwayFromZero) : 0.00m;
                var gpa4 = totalGradedCredits > 0 ? Math.Round(totalGpa4Weight / totalGradedCredits, 2, MidpointRounding.AwayFromZero) : 0.00m;

                results.Add(new AcademicYearDto(
                    ay.Id,
                    ay.YearName,
                    ay.StartYear,
                    ay.EndYear,
                    ay.Status,
                    ay.IsCurrent,
                    completedCredits,
                    gpa10,
                    gpa4,
                    ay.IsImported,
                    ay.ImportedCredits,
                    ay.ImportedGpa10,
                    ay.ImportedGpa4
                ));
            }
        }
        return results;
    }
}
