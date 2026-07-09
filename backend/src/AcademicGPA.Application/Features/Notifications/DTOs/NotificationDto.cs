using System;

namespace AcademicGPA.Application.Features.Notifications.DTOs;

/// <summary>
/// Data transfer object for a student notification.
/// </summary>
public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt
);
