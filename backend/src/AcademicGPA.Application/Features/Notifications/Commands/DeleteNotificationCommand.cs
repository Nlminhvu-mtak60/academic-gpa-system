using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Exceptions;
using AcademicGPA.Application.Common.Interfaces;
using MediatR;

namespace AcademicGPA.Application.Features.Notifications.Commands;

public record DeleteNotificationCommand(Guid Id) : IRequest<bool>;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, bool>
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public DeleteNotificationCommandHandler(
        INotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var result = await _notificationService.DeleteNotificationAsync(userId, request.Id, cancellationToken);
        if (!result)
        {
            throw new NotFoundException("Notification", request.Id);
        }

        return true;
    }
}
