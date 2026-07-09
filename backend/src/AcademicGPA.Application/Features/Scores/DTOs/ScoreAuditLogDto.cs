using System;

namespace AcademicGPA.Application.Features.Scores.DTOs;

/// <summary>
/// Data transfer object representing a historical modification record of a course's score.
/// </summary>
public record ScoreAuditLogDto(
    string FieldChanged,
    string? OldValue,
    string? NewValue,
    DateTime ChangedAt
);
