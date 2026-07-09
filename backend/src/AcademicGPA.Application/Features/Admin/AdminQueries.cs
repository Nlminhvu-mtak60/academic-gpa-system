using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Common.Interfaces;
using AcademicGPA.Application.Features.Admin.DTOs;
using MediatR;

namespace AcademicGPA.Application.Features.Admin;

public record GetAdminStatisticsQuery() : IRequest<AdminStatisticsDto>;

public record GetAdminStudentsQuery(
    int Page,
    int PageSize,
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortOrder
) : IRequest<(IReadOnlyList<AdminStudentDto> Items, int TotalCount)>;

public record GetAdminStudentDetailsQuery(Guid StudentId) : IRequest<AdminStudentDetailDto>;

public record GetAdminUsersQuery(
    int Page,
    int PageSize,
    string? Search
) : IRequest<(IReadOnlyList<AdminUserDto> Items, int TotalCount)>;

public record GetAdminActivityLogsQuery(
    int Page,
    int PageSize,
    Guid? UserId
) : IRequest<(IReadOnlyList<UserActivityLogDto> Items, int TotalCount)>;

public record GetAdminNotificationHistoryQuery(
    int Page,
    int PageSize
) : IRequest<(IReadOnlyList<AdminNotificationHistoryDto> Items, int TotalCount)>;

// Handlers

public class GetAdminStatisticsQueryHandler : IRequestHandler<GetAdminStatisticsQuery, AdminStatisticsDto>
{
    private readonly IAdminService _adminService;
    public GetAdminStatisticsQueryHandler(IAdminService adminService) => _adminService = adminService;
    public Task<AdminStatisticsDto> Handle(GetAdminStatisticsQuery request, CancellationToken cancellationToken)
        => _adminService.GetStatisticsAsync(cancellationToken);
}

public class GetAdminStudentsQueryHandler : IRequestHandler<GetAdminStudentsQuery, (IReadOnlyList<AdminStudentDto> Items, int TotalCount)>
{
    private readonly IAdminService _adminService;
    public GetAdminStudentsQueryHandler(IAdminService adminService) => _adminService = adminService;
    public Task<(IReadOnlyList<AdminStudentDto> Items, int TotalCount)> Handle(GetAdminStudentsQuery request, CancellationToken cancellationToken)
        => _adminService.GetStudentsAsync(request.Page, request.PageSize, request.Search, request.IsActive, request.SortBy, request.SortOrder, cancellationToken);
}

public class GetAdminStudentDetailsQueryHandler : IRequestHandler<GetAdminStudentDetailsQuery, AdminStudentDetailDto>
{
    private readonly IAdminService _adminService;
    public GetAdminStudentDetailsQueryHandler(IAdminService adminService) => _adminService = adminService;
    public Task<AdminStudentDetailDto> Handle(GetAdminStudentDetailsQuery request, CancellationToken cancellationToken)
        => _adminService.GetStudentDetailsAsync(request.StudentId, cancellationToken);
}

public class GetAdminUsersQueryHandler : IRequestHandler<GetAdminUsersQuery, (IReadOnlyList<AdminUserDto> Items, int TotalCount)>
{
    private readonly IAdminService _adminService;
    public GetAdminUsersQueryHandler(IAdminService adminService) => _adminService = adminService;
    public Task<(IReadOnlyList<AdminUserDto> Items, int TotalCount)> Handle(GetAdminUsersQuery request, CancellationToken cancellationToken)
        => _adminService.GetAllUsersAsync(request.Page, request.PageSize, request.Search, cancellationToken);
}

public class GetAdminActivityLogsQueryHandler : IRequestHandler<GetAdminActivityLogsQuery, (IReadOnlyList<UserActivityLogDto> Items, int TotalCount)>
{
    private readonly IAdminService _adminService;
    public GetAdminActivityLogsQueryHandler(IAdminService adminService) => _adminService = adminService;
    public Task<(IReadOnlyList<UserActivityLogDto> Items, int TotalCount)> Handle(GetAdminActivityLogsQuery request, CancellationToken cancellationToken)
        => _adminService.GetActivityLogsAsync(request.Page, request.PageSize, request.UserId, cancellationToken);
}

public class GetAdminNotificationHistoryQueryHandler : IRequestHandler<GetAdminNotificationHistoryQuery, (IReadOnlyList<AdminNotificationHistoryDto> Items, int TotalCount)>
{
    private readonly IAdminService _adminService;
    public GetAdminNotificationHistoryQueryHandler(IAdminService adminService) => _adminService = adminService;
    public Task<(IReadOnlyList<AdminNotificationHistoryDto> Items, int TotalCount)> Handle(GetAdminNotificationHistoryQuery request, CancellationToken cancellationToken)
        => _adminService.GetNotificationHistoryAsync(request.Page, request.PageSize, cancellationToken);
}
