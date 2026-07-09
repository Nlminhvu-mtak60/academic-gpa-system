using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Scores.DTOs;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Scores.Queries.GetScoreAuditLogs;

public record GetScoreAuditLogsQuery(Guid CourseId) : IRequest<IReadOnlyList<ScoreAuditLogDto>>;

public class GetScoreAuditLogsQueryHandler : IRequestHandler<GetScoreAuditLogsQuery, IReadOnlyList<ScoreAuditLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public GetScoreAuditLogsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ScoreAuditLogDto>> Handle(GetScoreAuditLogsQuery request, CancellationToken cancellationToken)
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

        // 4. Fetch Audit Logs
        var logs = await _unitOfWork.ScoreAuditLogs.GetByCourseIdAsync(request.CourseId, cancellationToken);

        return logs.Select(log => new ScoreAuditLogDto(
            log.FieldChanged,
            log.OldValue,
            log.NewValue,
            log.ChangedAt
        )).ToList();
    }
}
