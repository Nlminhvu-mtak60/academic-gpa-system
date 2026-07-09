using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.TranscriptImport.Commands.UndoImport;

public record UndoImportCommand(Guid BatchId) : IRequest;

public class UndoImportCommandHandler : IRequestHandler<UndoImportCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UndoImportCommandHandler(
        IApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UndoImportCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var profile = await _context.StudentProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == userId, cancellationToken);

        if (profile == null) throw new NotFoundException("StudentProfile", userId);

        var batch = await _unitOfWork.ImportBatches.GetByIdWithCoursesAsync(request.BatchId, cancellationToken);
        if (batch == null) throw new NotFoundException("ImportBatch", request.BatchId);

        if (batch.StudentProfileId != profile.Id)
        {
            throw new UnauthorizedAccessException("Not authorized to undo this batch.");
        }

        if (batch.IsUndone)
        {
            throw new InvalidOperationException("This batch has already been undone.");
        }

        foreach (var batchCourse in batch.ImportBatchCourses)
        {
            if (batchCourse.Course != null)
            {
                batchCourse.Course.IsDeleted = true; // Soft delete
                _unitOfWork.Courses.Update(batchCourse.Course);
            }
        }

        batch.IsUndone = true;
        _unitOfWork.ImportBatches.Update(batch);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
