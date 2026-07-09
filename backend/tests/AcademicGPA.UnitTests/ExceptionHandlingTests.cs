using System.Text;
using System.Text.Json;
using AcademicGPA.API.Middleware;
using AcademicGPA.Application.Common.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AcademicGPA.UnitTests;

public class ExceptionHandlingTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;

    public ExceptionHandlingTests()
    {
        _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
    }

    private async Task<(HttpContext Context, string ResponseBody)> RunMiddlewareWithException(Exception exceptionToThrow)
    {
        var middleware = new ExceptionHandlingMiddleware(
            next: (innerHttpContext) => throw exceptionToThrow,
            logger: _mockLogger.Object
        );

        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        await middleware.InvokeAsync(context);

        responseStream.Position = 0;
        using var reader = new StreamReader(responseStream, Encoding.UTF8);
        var body = await reader.ReadToEndAsync();

        return (context, body);
    }

    [Fact]
    public async Task InvokeAsync_WhenAppValidationException_ShouldReturn400AndErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is invalid" } }
        };
        var exception = new ValidationException(errors);

        // Act
        var (context, body) = await RunMiddlewareWithException(exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Contain("application/json");

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetProperty("email")[0].GetString().Should().Be("Email is invalid");
    }

    [Fact]
    public async Task InvokeAsync_WhenFluentValidationException_ShouldReturn400AndErrors()
    {
        // Arrange
        var failures = new[]
        {
            new FluentValidation.Results.ValidationFailure("Password", "Password is too short")
        };
        var exception = new FluentValidation.ValidationException(failures);

        // Act
        var (context, body) = await RunMiddlewareWithException(exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Contain("application/json");

        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetProperty("password")[0].GetString().Should().Be("Password is too short");
    }

    [Fact]
    public async Task InvokeAsync_WhenNotFoundException_ShouldReturn404()
    {
        // Arrange
        var exception = new NotFoundException("Course", "CS101");

        // Act
        var (context, body) = await RunMiddlewareWithException(exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetProperty("error")[0].GetString().Should().Contain("CS101");
    }

    [Fact]
    public async Task InvokeAsync_WhenForbiddenException_ShouldReturn403()
    {
        // Arrange
        var exception = new ForbiddenException("Access denied");

        // Act
        var (context, body) = await RunMiddlewareWithException(exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetProperty("error")[0].GetString().Should().Be("Access denied");
    }

    [Fact]
    public async Task InvokeAsync_WhenRateLimitException_ShouldReturn429()
    {
        // Arrange
        var exception = new RateLimitException("Too many requests");

        // Act
        var (context, body) = await RunMiddlewareWithException(exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetProperty("error")[0].GetString().Should().Be("Too many requests");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnprocessableEntityException_ShouldReturn422()
    {
        // Arrange
        var exception = new UnprocessableEntityException("Unprocessable payload");

        // Act
        var (context, body) = await RunMiddlewareWithException(exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetProperty("error")[0].GetString().Should().Be("Unprocessable payload");
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericException_ShouldReturn500()
    {
        // Arrange
        var exception = new Exception("Something terrible happened");

        // Act
        var (context, body) = await RunMiddlewareWithException(exception);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var doc = JsonDocument.Parse(body);
        doc.RootElement.GetProperty("success").GetBoolean().Should().BeFalse();
        doc.RootElement.GetProperty("errors").GetProperty("error")[0].GetString().Should().Be("An internal server error occurred.");
    }
}
