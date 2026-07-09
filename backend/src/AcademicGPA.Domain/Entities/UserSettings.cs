using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents student visual and notification preferences.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Unique identifier for the user settings configuration.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identifier of the linked user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Language code preference (e.g. "vi", "en").
    /// </summary>
    public string PreferredLanguage { get; set; } = "vi";

    /// <summary>
    /// Theme preference ("light", "dark", "system").
    /// </summary>
    public string PreferredTheme { get; set; } = "light";

    /// <summary>
    /// Flag to enable/disable System notifications.
    /// </summary>
    public bool ReceiveSystem { get; set; } = true;

    /// <summary>
    /// Flag to enable/disable Academic grade notifications.
    /// </summary>
    public bool ReceiveAcademic { get; set; } = true;

    /// <summary>
    /// Flag to enable/disable Academic Goal notifications.
    /// </summary>
    public bool ReceiveGoal { get; set; } = true;

    /// <summary>
    /// Flag to enable/disable GPA Milestone notifications.
    /// </summary>
    public bool ReceiveGpaMilestone { get; set; } = true;

    /// <summary>
    /// Date when preference record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when preferences were last modified.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
