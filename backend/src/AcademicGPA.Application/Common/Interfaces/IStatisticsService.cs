using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Statistics.DTOs;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service interface for retrieving student academic statistics and analytics.
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// Retrieves GPA metrics organized chronologically by semester.
    /// </summary>
    Task<IReadOnlyList<GpaTrendDto>> GetGpaTrendAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves total count of earned letter grades.
    /// </summary>
    Task<GradeDistributionDto> GetGradeDistributionAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves credit completion progress data.
    /// </summary>
    Task<CreditProgressDto> GetCreditProgressAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Analyzes performance to identify the student's strongest and weakest courses.
    /// </summary>
    Task<StrengthsWeaknessesDto> GetStrengthsWeaknessesAsync(Guid userId, CancellationToken cancellationToken);
}
