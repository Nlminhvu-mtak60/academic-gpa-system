using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.TranscriptImport.DTOs;
using AcademicGPA.Domain.Entities;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.TranscriptImport.Commands.ConfirmImport;

public record ConfirmTranscriptImportCommand(
    Guid SemesterId,
    List<ImportedCourseDto> Courses,
    string SourceType,
    Guid? GradeScaleId = null
) : IRequest<Guid>;

public class ConfirmTranscriptImportCommandHandler : IRequestHandler<ConfirmTranscriptImportCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGpaCalculator _gpaCalculator;

    public ConfirmTranscriptImportCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IGpaCalculator gpaCalculator)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<Guid> Handle(ConfirmTranscriptImportCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null) throw new NotFoundException("StudentProfile", userId);

        var semester = await _unitOfWork.Semesters.GetByIdAsync(request.SemesterId, cancellationToken);
        if (semester == null) throw new NotFoundException("Semester", request.SemesterId);

        // Verify ownership (simplified)
        var academicYear = await _unitOfWork.AcademicYears.GetByIdAsync(semester.AcademicYearId, cancellationToken);
        if (academicYear == null || academicYear.StudentProfileId != profile.Id)
        {
            throw new UnauthorizedAccessException("Not authorized to import to this semester.");
        }

        // Create ImportBatch
        var batch = new ImportBatch
        {
            StudentProfileId = profile.Id,
            SemesterId = semester.Id,
            SourceType = request.SourceType,
            CourseCount = request.Courses.Count
        };

        await _context.ImportBatches.AddAsync(batch, cancellationToken);

        Domain.Entities.GradeScale? gradeScale = null;
        if (request.GradeScaleId.HasValue)
        {
            gradeScale = await _context.GradeScales
                .Include(gs => gs.GradeScaleEntries)
                .FirstOrDefaultAsync(gs => gs.Id == request.GradeScaleId.Value, cancellationToken);
        }
        else
        {
            gradeScale = await _context.GradeScales
                .Include(gs => gs.GradeScaleEntries)
                .FirstOrDefaultAsync(gs => gs.IsDefault, cancellationToken);
        }

        // Create Courses and Scores
        foreach (var dto in request.Courses)
        {
            var course = new Course
            {
                SemesterId = semester.Id,
                CourseCode = "IMP-" + Guid.NewGuid().ToString("N")[..8].ToUpper(), // Auto-generate if not provided
                CourseName = dto.CourseName,
                Credits = dto.Credits,
                IsRetake = false,
                IsDeleted = false
            };

            await _unitOfWork.Courses.AddAsync(course, cancellationToken);

            var score = new Score
            {
                CourseId = course.Id,
                CourseScore = dto.FinalScore,
                // Assign component scores if applicable. In a real app, map correctly.
                ContinuousScore = dto.ComponentScores.TryGetValue("TX", out var tx) ? tx : 
                                  dto.ComponentScores.TryGetValue("KT", out var kt) ? kt : null,
                FinalExamScore = dto.ComponentScores.TryGetValue("CK", out var ck) ? ck : 
                                 dto.ComponentScores.TryGetValue("Thi", out var thi) ? thi : null
            };

            if (score.CourseScore.HasValue)
            {
                var gradeResult = _gpaCalculator.MapToGradeResult(score.CourseScore.Value, gradeScale);
                score.LetterGrade = gradeResult.LetterGrade;
                score.Gpa4Value = gradeResult.Gpa4Value;
                score.AcademicClassification = gradeResult.AcademicClassification;
                score.IsPass = gradeResult.IsPass;
            }

            await _unitOfWork.Scores.AddAsync(score, cancellationToken);

            // Link batch and course
            await _context.ImportBatchCourses.AddAsync(new ImportBatchCourse
            {
                ImportBatchId = batch.Id,
                CourseId = course.Id
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return batch.Id;
    }
}
