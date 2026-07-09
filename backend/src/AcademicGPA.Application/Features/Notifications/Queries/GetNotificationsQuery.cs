using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Notifications.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Notifications.Queries;

public record GetNotificationsQuery(
    int Page = 1,
    int PageSize = 10,
    bool? UnreadOnly = null
) : IRequest<NotificationsListDto>;

public record NotificationsListDto(
    IReadOnlyList<NotificationDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, NotificationsListDto>
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public GetNotificationsQueryHandler(
        INotificationService notificationService,
        ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    public async Task<NotificationsListDto> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userIdStr = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        int page = request.Page <= 0 ? 1 : request.Page;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var items = await _notificationService.GetNotificationsAsync(userId, page, pageSize, request.UnreadOnly, cancellationToken);
        var totalCount = await _notificationService.GetTotalNotificationsCountAsync(userId, request.UnreadOnly, cancellationToken);

        return new NotificationsListDto(items, totalCount, page, pageSize);
    }
}
