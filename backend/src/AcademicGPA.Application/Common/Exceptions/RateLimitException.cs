using System;

namespace AcademicGPA.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a user exceeds a system rate limit, resulting in a 429 Too Many Requests response.
/// </summary>
public class RateLimitException : Exception
{
    public RateLimitException(string message)
        : base(message)
    {
    }
}
