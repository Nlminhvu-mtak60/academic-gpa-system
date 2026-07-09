using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AcademicGPA.Application.Features.Admin.DTOs;

namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Service managing administrator controls for students, accounts, system metrics, and notification auditing.
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Retrieves aggregated system metrics for students, overall averages, and classifications.
    /// </summary>
    Task<AdminStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated, searchable list of students.
    /// </summary>
    Task<(IReadOnlyList<AdminStudentDto> Items, int TotalCount)> GetStudentsAsync(
        int page, int pageSize, string? search, bool? isActive, string? sortBy, string? sortOrder, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves detailed academic history, profile, and email metadata for a specific student.
    /// </summary>
    Task<AdminStudentDetailDto> GetStudentDetailsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>
    /// Locks a student user account, terminating all active sessions and recording the administrative lock reasons.
    /// </summary>
    Task LockStudentAsync(Guid studentId, string reason, string adminIpAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Unlocks a locked student account, allowing login operations again.
    /// </summary>
    Task UnlockStudentAsync(Guid studentId, string adminIpAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Resets the user's password to a cryptographically secure random plaintext password, forcing password change on next login.
    /// </summary>
    Task<string> ResetStudentPasswordAsync(Guid studentId, string adminIpAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Soft-deletes a student profile, invalidating all sessions and marking records as deleted.
    /// </summary>
    Task DeleteStudentAsync(Guid studentId, string adminIpAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Sends an in-app targeted notification to a student from an administrator.
    /// </summary>
    Task SendDirectNotificationAsync(Guid adminUserId, Guid recipientId, string title, string message, string type, CancellationToken cancellationToken);

    /// <summary>
    /// Broadcasts an announcement notification to all active system students.
    /// </summary>
    Task BroadcastNotificationAsync(Guid adminUserId, string title, string message, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of direct and broadcast notifications sent by administrators.
    /// </summary>
    Task<(IReadOnlyList<AdminNotificationHistoryDto> Items, int TotalCount)> GetNotificationHistoryAsync(
        int page, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the target student's name, code, university details, and required graduation credits.
    /// </summary>
    Task EditStudentInfoAsync(Guid studentId, EditStudentInfoDto dto, string adminIpAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of all users regardless of role.
    /// </summary>
    Task<(IReadOnlyList<AdminUserDto> Items, int TotalCount)> GetAllUsersAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of system activity logs for auditing.
    /// </summary>
    Task<(IReadOnlyList<UserActivityLogDto> Items, int TotalCount)> GetActivityLogsAsync(
        int page, int pageSize, Guid? userId, CancellationToken cancellationToken);

    /// <summary>
    /// Audits a user activity in the activity logs.
    /// </summary>
    Task LogActivityAsync(Guid userId, string activity, string ipAddress, CancellationToken cancellationToken);
}
