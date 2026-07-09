using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Scores.DTOs;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Scores.Queries.GetScores;

public record GetScoresQuery(Guid CourseId) : IRequest<ScoreDto>;

public class GetScoresQueryHandler : IRequestHandler<GetScoresQuery, ScoreDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public GetScoresQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ScoreDto> Handle(GetScoresQuery request, CancellationToken cancellationToken)
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

        // 3. Verify Course existence & ownership
        var course = await _unitOfWork.Courses.GetByIdWithOwnerAsync(request.CourseId, profile.Id, cancellationToken);
        if (course == null)
        {
            throw new NotFoundException("Course", request.CourseId);
        }

        // 4. Fetch Score details
        var score = await _unitOfWork.Scores.GetByCourseIdAsync(request.CourseId, cancellationToken);
        if (score == null)
        {
            return new ScoreDto(null, null, null, null, null, null, null, null, null);
        }

        return new ScoreDto(
            score.AttendanceScore,
            score.ContinuousScore,
            score.FinalExamScore,
            score.CourseScore,
            score.LetterGrade,
            score.Gpa4Value,
            score.AcademicClassification,
            score.IsPass,
            score.UpdatedAt
        );
    }
}
