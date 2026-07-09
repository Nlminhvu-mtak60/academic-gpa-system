using System;
using System.Collections.Generic;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents an AI Advisor chat conversation thread.
/// </summary>
public class Conversation
{
    /// <summary>
    /// Unique identifier for the conversation.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Student-defined title of the conversation.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key referencing the User who owns the conversation.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Owner user navigation property.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp (e.g. when a new message is posted).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Chronological list of chat messages.
    /// </summary>
    public ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}
