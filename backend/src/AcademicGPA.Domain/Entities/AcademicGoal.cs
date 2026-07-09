using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a student's target cumulative GPA goal.
/// Only one goal can be active at a time (IsActive = true).
/// Historical goals are retained for reference.
/// </summary>
public class AcademicGoal
{
    /// <summary>
    /// Unique identifier for the goal.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key referencing the associated StudentProfile.
    /// </summary>
    public Guid StudentProfileId { get; set; }

    /// <summary>
    /// Target cumulative GPA on 10-scale (0.00 to 10.00).
    /// </summary>
    public decimal TargetCumulativeGpa10 { get; set; }

    /// <summary>
    /// Target cumulative GPA on 4-scale (auto-computed from GPA10 mapping).
    /// </summary>
    public decimal TargetCumulativeGpa4 { get; set; }

    /// <summary>
    /// Optional notes about the goal (e.g. "Target for scholarship").
    /// Maximum 500 characters.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether the student has achieved this target GPA.
    /// Auto-set to true when cumulative GPA meets or exceeds target (BR-GOAL-003).
    /// </summary>
    public bool IsAchieved { get; set; }

    /// <summary>
    /// Whether this is the currently active goal.
    /// Only one goal per student can be active at a time (BR-GOAL-002).
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the parent StudentProfile.
    /// </summary>
    public StudentProfile StudentProfile { get; set; } = null!;
}
