using System;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a message exchange within an AI Advisor conversation thread.
/// </summary>
public class ConversationMessage
{
    /// <summary>
    /// Unique identifier for the message.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key referencing the parent Conversation thread.
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// Parent conversation navigation property.
    /// </summary>
    public Conversation Conversation { get; set; } = null!;

    /// <summary>
    /// Sender role designation: "user" or "assistant".
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Text content payload of the message.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
