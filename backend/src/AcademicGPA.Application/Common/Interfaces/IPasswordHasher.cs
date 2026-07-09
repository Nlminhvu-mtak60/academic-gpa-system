namespace AcademicGPA.Application.Common.Interfaces;

/// <summary>
/// Cryptographic hashing service for encrypting and verifying user passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using salt-rounds.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies if a plain-text password matches a hashed password.
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);
}
