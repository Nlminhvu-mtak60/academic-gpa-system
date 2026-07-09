using System;

namespace AcademicGPA.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated, resulting in a 422 Unprocessable Entity response.
/// </summary>
public class UnprocessableEntityException : Exception
{
    public UnprocessableEntityException(string message)
        : base(message)
    {
    }
}
