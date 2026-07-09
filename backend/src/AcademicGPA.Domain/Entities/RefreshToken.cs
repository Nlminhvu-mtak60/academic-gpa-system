namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a rotating refresh token associated with a user session.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Unique identifier for the refresh token.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key referencing the associated User.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Cryptographically secure token string.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the new token that replaced this one during rotation.
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Token expiration timestamp.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// IP address of the client device that requested this token.
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// Timestamp when this token was explicitly revoked.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Navigation property to the owner user.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Indicates whether the token has passed its expiration time.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Indicates whether the token has been revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt != null;

    /// <summary>
    /// Indicates whether the token is currently active and usable.
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;
}
