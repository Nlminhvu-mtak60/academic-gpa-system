# 13 — Exception Handling Design

> **Document ID**: ARC-BE-EXC-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Error interceptors and RFC 7807 JSON schemas

---

## 1. Exception Handling Architecture

The backend uses a centralized exception handling architecture. Rather than using boilerplate `try-catch` blocks inside controller actions, all exceptions are caught by a custom middleware registered in the ASP.NET Core request pipeline.

---

## 2. Global Exception Middleware (`ExceptionHandlingMiddleware`)

*   **Behavior**: Intercepts all system errors during request execution.
*   **Actions**:
    *   Determines the type of exception thrown.
    *   Logs the full stack trace securely to Serilog.
    *   Translates the exception into a standardized **RFC 7807 ProblemDetails** JSON response, hiding internal database details from the client.

---

## 3. Exception to HTTP Status Code Mapping

The middleware maps system exceptions to specific HTTP status codes:

| System Exception Class | HTTP Status Code | RFC 7807 Error Code | Description / Usage |
|:---|:---:|:---|:---|
| `ValidationException` | `400 Bad Request` | `VALIDATION_ERROR` | Request payload fails validation rules. |
| `UnauthorizedAccessException`| `401 Unauthorized` | `UNAUTHORIZED` | Missing or invalid access token. |
| `ForbiddenException` | `403 Forbidden` | `FORBIDDEN` | Accessing another student's record. |
| `NotFoundException` | `404 Not Found` | `NOT_FOUND` | The requested entity does not exist. |
| `BusinessRuleViolationException`| `422 Unprocessable` | `BUSINESS_RULE_VIOLATION` | Operation violates academic constraints. |
| `RateLimitExceededException`| `429 Too Many Requests`| `RATE_LIMIT_EXCEEDED` | Rate limit threshold exceeded. |
| *All other exceptions* | `500 Server Error` | `INTERNAL_SERVER_ERROR` | Unhandled runtime server error. |

---

## 4. RFC 7807 JSON Error Response Schema

#### Example: Validation Error Payload
```json
{
  "type": "https://errors.gpaapp.com/validation-error",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Failed to validate course credits input.",
  "instance": "/api/v1/courses",
  "errors": {
    "credits": [ "Credits must be an integer between 1 and 6." ]
  }
}
```

---

*End of Document — Exception Handling Design*
