using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Semesters.Commands.DeleteSemester;

public record DeleteSemesterCommand(Guid Id) : IRequest;

public class DeleteSemesterCommandHandler : IRequestHandler<DeleteSemesterCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSemesterCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteSemesterCommand request, CancellationToken cancellationToken)
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

        // 3. Fetch Semester and check ownership
        var semester = await _unitOfWork.Semesters.GetByIdWithOwnerAsync(request.Id, profile.Id, cancellationToken);
        if (semester == null)
        {
            throw new NotFoundException("Semester", request.Id);
        }

        // 4. Soft-delete Semester
        semester.IsDeleted = true;

        // 5. In future phases, soft-delete nested courses here if they exist:
        // (Courses are not yet implemented)

        _unitOfWork.Semesters.Update(semester);

        // 6. Triggers GPA recalculation for the student's remaining active semesters in future phases:
        // (GPA recalculation service/logic is not yet implemented)

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
