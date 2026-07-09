using AcademicGPA.Domain.Enums;

namespace AcademicGPA.Domain.Entities;

/// <summary>
/// Represents a user within the system, covering both students and administrators.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Email address of the user (acts as unique identifier for logins).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Salted and hashed password using BCrypt.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Assigned role defining route authorization boundaries.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Student;

    /// <summary>
    /// Indicates whether the account is unlocked and active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates whether the email has been verified.
    /// </summary>
    public bool IsEmailVerified { get; set; } = false;

    /// <summary>
    /// External identifier for Google Single Sign-On integrations.
    /// </summary>
    public string? GoogleId { get; set; }

    /// <summary>
    /// URL of the user's avatar image.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Language code preference (e.g. "vi", "en").
    /// </summary>
    public string PreferredLanguage { get; set; } = "vi";

    /// <summary>
    /// Visual interface theme preference ("light", "dark").
    /// </summary>
    public string PreferredTheme { get; set; } = "light";

    /// <summary>
    /// Record creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Record last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last successful login timestamp.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Timestamp when account was locked. Null if active.
    /// </summary>
    public DateTime? LockedAt { get; set; }

    /// <summary>
    /// Explanation of why the account was locked by an administrator.
    /// </summary>
    public string? LockReason { get; set; }
 
    /// <summary>
    /// Indicates whether the account has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Indicates whether the user must change their password on next login.
    /// </summary>
    public bool ForcePasswordChange { get; set; } = false;

    /// <summary>
    /// Navigation property containing rotating refresh token histories.
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Navigation property to the student profile. Null if user is an administrator.
    /// </summary>
    public StudentProfile? StudentProfile { get; set; }

    /// <summary>
    /// Navigation property to the user's preferences settings.
    /// </summary>
    public UserSettings? UserSettings { get; set; }
}
