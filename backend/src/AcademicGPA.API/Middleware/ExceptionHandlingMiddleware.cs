using System.Text.Json;
using AcademicGPA.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AcademicGPA.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request execution");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            FluentValidation.ValidationException fluentEx => CreateValidationResponse(context, fluentEx),
            ValidationException appEx => CreateAppValidationResponse(context, appEx),
            NotFoundException notFoundEx => CreateErrorResponse(context, StatusCodes.Status404NotFound, notFoundEx.Message),
            UnprocessableEntityException unprocessableEx => CreateErrorResponse(context, StatusCodes.Status422UnprocessableEntity, unprocessableEx.Message),
            RateLimitException rateLimitEx => CreateErrorResponse(context, StatusCodes.Status429TooManyRequests, rateLimitEx.Message),
            ForbiddenException forbiddenEx => CreateErrorResponse(context, StatusCodes.Status403Forbidden, forbiddenEx.Message),
            UnauthorizedAccessException unauthorizedEx => CreateErrorResponse(context, StatusCodes.Status401Unauthorized, unauthorizedEx.Message),
            _ => CreateErrorResponse(context, StatusCodes.Status500InternalServerError, "An internal server error occurred.")
        };

        var options = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase 
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private static ApiResponse CreateValidationResponse(HttpContext context, FluentValidation.ValidationException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ApiResponse(false, null, errors);
    }

    private static ApiResponse CreateAppValidationResponse(HttpContext context, ValidationException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return new ApiResponse(false, null, exception.Errors);
    }

    private static ApiResponse CreateErrorResponse(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        var errors = new Dictionary<string, string[]>
        {
            { "error", new[] { message } }
        };
        return new ApiResponse(false, null, errors);
    }

    private record ApiResponse(bool Success, object? Data, object? Errors)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }
}
