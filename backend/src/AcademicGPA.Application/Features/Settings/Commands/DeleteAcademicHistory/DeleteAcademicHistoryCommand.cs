using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Settings.Commands.DeleteAcademicHistory;

public record DeleteAcademicHistoryCommand(Guid StudentProfileId) : IRequest;

public class DeleteAcademicHistoryCommandHandler : IRequestHandler<DeleteAcademicHistoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAcademicHistoryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAcademicHistoryCommand request, CancellationToken cancellationToken)
    {
        var academicYears = await _unitOfWork.AcademicYears
            .GetQueryable()
            .Where(y => y.StudentProfileId == request.StudentProfileId)
            .ToListAsync(cancellationToken);

        foreach (var year in academicYears)
        {
            _unitOfWork.AcademicYears.Delete(year);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
