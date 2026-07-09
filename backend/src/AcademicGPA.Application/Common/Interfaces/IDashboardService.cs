using System;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Dashboard.DTOs;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service interface for retrieving student dashboard data.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Retrieves consolidated summary data for the student dashboard.
    /// </summary>
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(Guid userId, CancellationToken cancellationToken);
}
