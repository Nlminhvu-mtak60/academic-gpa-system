using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.AcademicYears.Commands.DeleteAcademicYear;

public record DeleteAcademicYearCommand(Guid Id) : IRequest<Unit>;

public class DeleteAcademicYearCommandHandler : IRequestHandler<DeleteAcademicYearCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAcademicYearCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteAcademicYearCommand request, CancellationToken cancellationToken)
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

        // 3. Fetch the Academic Year owned by student
        var academicYear = await _unitOfWork.AcademicYears.GetByIdWithOwnerAsync(request.Id, profile.Id, cancellationToken);
        if (academicYear == null)
        {
            throw new NotFoundException("AcademicYear", request.Id);
        }

        // 4. Validate that it does not contain semesters (compatibility check)
        var hasSemesters = await _unitOfWork.AcademicYears.HasSemestersAsync(request.Id, cancellationToken);
        if (hasSemesters)
        {
            throw new AcademicGPA.Application.Common.Exceptions.ValidationException("id", "Cannot delete an academic year that contains semesters.");
        }

        // 5. Soft-delete the academic year
        academicYear.IsDeleted = true;
        
        _unitOfWork.AcademicYears.Update(academicYear);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
