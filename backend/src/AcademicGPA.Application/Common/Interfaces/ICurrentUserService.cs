namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Exposes claims profile details from the current execution context.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current logged-in user identifier. Null if anonymous.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's email address.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's role designation.
    /// </summary>
    string? Role { get; }
}
