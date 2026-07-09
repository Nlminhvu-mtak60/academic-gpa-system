using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AcademicGPA.Application.Features.Settings.Commands.DeleteAccount;

public record DeleteAccountCommand(Guid StudentProfileId) : IRequest;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var profile = await _unitOfWork.Students
            .GetQueryable()
            .FirstOrDefaultAsync(p => p.Id == request.StudentProfileId, cancellationToken);

        if (profile == null) throw new Exception("Profile not found.");

        _unitOfWork.Students.Delete(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
