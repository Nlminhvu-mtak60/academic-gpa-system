using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Scores.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Scores.Commands.UpdateScores;

public record UpdateScoresCommand(
    Guid CourseId,
    decimal? AttendanceScore,
    decimal? ContinuousScore,
    decimal? FinalExamScore
) : IRequest<ScoreDto>;

public class UpdateScoresCommandValidator : AbstractValidator<UpdateScoresCommand>
{
    public UpdateScoresCommandValidator()
    {
        RuleFor(x => x.AttendanceScore)
            .InclusiveBetween(0.0m, 10.0m).When(x => x.AttendanceScore.HasValue)
            .WithMessage("Attendance score must be between 0.0 and 10.0.");

        RuleFor(x => x.ContinuousScore)
            .InclusiveBetween(0.0m, 10.0m).When(x => x.ContinuousScore.HasValue)
            .WithMessage("Continuous assessment score must be between 0.0 and 10.0.");

        RuleFor(x => x.FinalExamScore)
            .InclusiveBetween(0.0m, 10.0m).When(x => x.FinalExamScore.HasValue)
            .WithMessage("Final exam score must be between 0.0 and 10.0.");
    }
}

public class UpdateScoresCommandHandler : IRequestHandler<UpdateScoresCommand, ScoreDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGpaCalculator _gpaCalculator;
    private readonly INotificationService _notificationService;

    public UpdateScoresCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IGpaCalculator gpaCalculator,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _gpaCalculator = gpaCalculator;
        _notificationService = notificationService;
    }

    public async Task<ScoreDto> Handle(UpdateScoresCommand request, CancellationToken cancellationToken)
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

        // 3. Verify Course existence & ownership (Load only Courses and Scores directly)
        var allCourses = await _context.Courses
            .Include(c => c.Score)
            .Where(c => c.Semester.AcademicYear.StudentProfileId == profile.Id 
                && !c.IsDeleted 
                && !c.Semester.IsDeleted 
                && !c.Semester.AcademicYear.IsDeleted)
            .ToListAsync(cancellationToken);

        var course = allCourses.FirstOrDefault(c => c.Id == request.CourseId);
        if (course == null)
        {
            throw new NotFoundException("Course", request.CourseId);
        }

        // Calculate old cumulative GPA for milestone/goal tracking (In-memory)
        var oldBestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();
        var oldGradedBestAttempts = oldBestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
        var oldGpa10 = _gpaCalculator.CalculateGpa10(oldGradedBestAttempts);

        // 4. Round inputs to nearest 0.5 if provided
        decimal? roundedAttendance = request.AttendanceScore.HasValue
            ? Math.Round(request.AttendanceScore.Value * 2, MidpointRounding.AwayFromZero) / 2
            : (decimal?)null;

        decimal? roundedContinuous = request.ContinuousScore.HasValue
            ? Math.Round(request.ContinuousScore.Value * 2, MidpointRounding.AwayFromZero) / 2
            : (decimal?)null;

        decimal? roundedFinal = request.FinalExamScore.HasValue
            ? Math.Round(request.FinalExamScore.Value * 2, MidpointRounding.AwayFromZero) / 2
            : (decimal?)null;

        // 5. Fetch or create Score entity (In-memory)
        var scoreEntity = course.Score;
        bool isNew = scoreEntity == null;
        if (isNew)
        {
            scoreEntity = new Score { CourseId = request.CourseId };
            course.Score = scoreEntity; // Link in memory for updated GPA calculation
        }

        // 6. Track Audit Logs
        var auditLogs = new List<ScoreAuditLog>();
        void AuditIfChanged(string fieldName, decimal? oldVal, decimal? newVal)
        {
            if (oldVal != newVal)
            {
                auditLogs.Add(new ScoreAuditLog
                {
                    CourseId = request.CourseId,
                    FieldChanged = fieldName,
                    OldValue = oldVal?.ToString("0.0"),
                    NewValue = newVal?.ToString("0.0"),
                    ChangedAt = DateTime.UtcNow
                });
            }
        }

        AuditIfChanged("AttendanceScore", isNew ? null : scoreEntity.AttendanceScore, roundedAttendance);
        AuditIfChanged("ContinuousScore", isNew ? null : scoreEntity.ContinuousScore, roundedContinuous);
        AuditIfChanged("FinalExamScore", isNew ? null : scoreEntity.FinalExamScore, roundedFinal);

        // 7. Update Score Entity component fields
        scoreEntity.AttendanceScore = roundedAttendance;
        scoreEntity.ContinuousScore = roundedContinuous;
        scoreEntity.FinalExamScore = roundedFinal;

        // 8. Calculate course results if all components are present
        if (roundedAttendance.HasValue && roundedContinuous.HasValue && roundedFinal.HasValue)
        {
            scoreEntity.CourseScore = _gpaCalculator.CalculateCourseScore(roundedAttendance, roundedContinuous, roundedFinal);
            if (scoreEntity.CourseScore.HasValue)
            {
                var gradeRes = _gpaCalculator.MapToGradeResult(scoreEntity.CourseScore.Value);
                scoreEntity.LetterGrade = gradeRes.LetterGrade;
                scoreEntity.Gpa4Value = gradeRes.Gpa4Value;
                scoreEntity.AcademicClassification = gradeRes.AcademicClassification;
                scoreEntity.IsPass = gradeRes.IsPass;
            }
        }
        else
        {
            scoreEntity.CourseScore = null;
            scoreEntity.LetterGrade = null;
            scoreEntity.Gpa4Value = null;
            scoreEntity.AcademicClassification = null;
            scoreEntity.IsPass = null;
        }

        scoreEntity.UpdatedAt = DateTime.UtcNow;

        // 9. Queue Score changes
        if (isNew)
        {
            await _unitOfWork.Scores.AddAsync(scoreEntity, cancellationToken);
        }
        else
        {
            _unitOfWork.Scores.Update(scoreEntity);
        }

        foreach (var auditLog in auditLogs)
        {
            await _unitOfWork.ScoreAuditLogs.AddAsync(auditLog, cancellationToken);
        }

        // 10. Queue Academic Notification (saveChanges: false)
        await _notificationService.SendNotificationAsync(
            userId,
            "Grades Updated",
            $"Component scores for course '{course.CourseName}' have been updated: " +
            $"Attendance: {roundedAttendance?.ToString("0.0") ?? "-"}, " +
            $"Continuous: {roundedContinuous?.ToString("0.0") ?? "-"}, " +
            $"Final Exam: {roundedFinal?.ToString("0.0") ?? "-"}. " +
            $"Course Score: {scoreEntity.CourseScore?.ToString("0.0") ?? "-"}.",
            "Academic",
            cancellationToken,
            saveChanges: false
        );

        // 11. Calculate new GPA in-memory and check milestones / goals
        var newBestAttempts = _gpaCalculator.FilterBestAttempts(allCourses).ToList();
        var newGradedBestAttempts = newBestAttempts.Where(c => c.Score != null && c.Score.CourseScore.HasValue).ToList();
        var newGpa10 = _gpaCalculator.CalculateGpa10(newGradedBestAttempts);

        // Check milestones
        var milestones = new[] { 7.0m, 8.0m, 9.0m };
        foreach (var milestone in milestones)
        {
            if ((!oldGpa10.HasValue || oldGpa10.Value < milestone) && (newGpa10.HasValue && newGpa10.Value >= milestone))
            {
                string classification = milestone switch
                {
                    9.0m => "Excellent (Xuất sắc)",
                    8.0m => "Very Good (Giỏi)",
                    7.0m => "Good (Khá)",
                    _ => ""
                };
                await _notificationService.SendNotificationAsync(
                    userId,
                    "GPA Milestone Achieved!",
                    $"Congratulations! Your cumulative GPA has reached {newGpa10.Value:0.00}, crossing the {milestone:0.1} ({classification}) milestone!",
                    "GpaMilestone",
                    cancellationToken,
                    saveChanges: false
                );
            }
        }

        // Check active target goal
        var activeGoal = await _context.AcademicGoals
            .FirstOrDefaultAsync(ag => ag.StudentProfileId == profile.Id && ag.IsActive && !ag.IsAchieved, cancellationToken);

        if (activeGoal != null && newGpa10.HasValue && newGpa10.Value >= activeGoal.TargetCumulativeGpa10)
        {
            activeGoal.IsAchieved = true;
            _context.AcademicGoals.Update(activeGoal);

            await _notificationService.SendNotificationAsync(
                userId,
                "Goal Achieved!",
                $"Congratulations! You have achieved your target cumulative GPA goal of {activeGoal.TargetCumulativeGpa10:0.00} with a current cumulative GPA of {newGpa10.Value:0.00}!",
                "Goal",
                cancellationToken,
                saveChanges: false
            );
        }

        // 12. Save all queued changes in a single database transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ScoreDto(
            scoreEntity.AttendanceScore,
            scoreEntity.ContinuousScore,
            scoreEntity.FinalExamScore,
            scoreEntity.CourseScore,
            scoreEntity.LetterGrade,
            scoreEntity.Gpa4Value,
            scoreEntity.AcademicClassification,
            scoreEntity.IsPass,
            scoreEntity.UpdatedAt
        );
    }
}
