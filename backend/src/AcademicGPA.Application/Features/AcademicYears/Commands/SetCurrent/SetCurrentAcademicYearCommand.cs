using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.AcademicYears.Commands.SetCurrent;

public record SetCurrentAcademicYearCommand(Guid Id) : IRequest<Unit>;

public class SetCurrentAcademicYearCommandHandler : IRequestHandler<SetCurrentAcademicYearCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public SetCurrentAcademicYearCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetCurrentAcademicYearCommand request, CancellationToken cancellationToken)
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

        // 3. Verify target Academic Year existence and ownership
        var targetYear = await _unitOfWork.AcademicYears.GetByIdWithOwnerAsync(request.Id, profile.Id, cancellationToken);
        if (targetYear == null)
        {
            throw new NotFoundException("AcademicYear", request.Id);
        }

        // 4. Update current active status inside a transaction
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var academicYears = await _unitOfWork.AcademicYears.GetByStudentProfileIdAsync(profile.Id, cancellationToken);

            foreach (var ay in academicYears)
            {
                if (ay.Id == targetYear.Id)
                {
                    ay.IsCurrent = true;
                    ay.Status = "Current";
                }
                else
                {
                    ay.IsCurrent = false;
                    ay.Status = "Completed";
                }
                _unitOfWork.AcademicYears.Update(ay);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        return Unit.Value;
    }
}
