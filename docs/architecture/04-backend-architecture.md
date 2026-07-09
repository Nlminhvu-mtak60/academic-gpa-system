# 04 — Backend Architecture

> **Document ID**: ARC-BA-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: C# / .NET 9 architectural patterns and execution wrappers

---

## 1. Clean Architecture Components Design

This document details the backend architectural design of the Academic GPA Management System built on **.NET 9**.

```
ASP.NET Core Web API Execution flow:
HTTP Request 
   ├── Presentation: ExceptionMiddleware (Global Interceptor)
   ├── Presentation: RateLimitingMiddleware
   ├── Presentation: JwtAuthMiddleware
   └── Presentation: Controllers (Route/Parameter Binding)
          └── Application: FluentValidation check (Validators)
                 └── Application: Handler (MediatR Command/Query)
                        ├── Domain: Aggregate Entities (Formulas, rounding)
                        └── Infrastructure: SQL Repositories (EF Core Transactions)
```

---

## 2. Core Components Specification

### 2.1 Controllers (Presentation Layer)
*   **Behavior**: Controllers inherit from `ApiControllerBase` and are decorated with the `[ApiController]` and `[Route("api/v1/[controller]")]` attributes.
*   **Design Rule**: Controllers must contain zero business logic. They act purely as dispatchers that receive HTTP requests, invoke MediatR's `ISender` to forward commands/queries, and return appropriate status codes.

### 2.2 DTOs (Data Transfer Objects)
*   **Behavior**: Simple record structures used as API contracts.
*   **Design Rule**: DTOs are declared as immutable C# `record` types to prevent state mutability during transport.
*   **Mapping**: AutoMapper (or Mapster) mapping profiles map domain entities to outward-facing DTOs in the Application Layer.

### 2.3 Validators (Application Layer)
*   **Behavior**: Form syntax validation.
*   **Design Rule**: Business payload inputs must have a matching `AbstractValidator<T>` mapping (using **FluentValidation**). These run inside the MediatR Pipeline Behavior before execution hits the handler, throwing a `ValidationException` if rules are violated.

### 2.4 MediatR Handlers (Application Layer)
*   **Behavior**: Use-case handlers implementing `IRequestHandler<TRequest, TResponse>`.
*   **Design Rule**: Commands write data to the DB; Queries read data. Handlers are decoupled from specific SQL configurations, accessing data through injected interfaces (e.g. `ICourseRepository`).

### 2.5 Domain Entities & Value Objects (Domain Layer)
*   **Behavior**: Enterprise business rules.
*   **Design Rule**:
    *   **Entities**: Have identities (GUIDs) and track lifecycle state. Must protect invariants (e.g. a `Semester` cannot accept a 4th course if a hard limit is set, and a `Course` calculates its own grade using component scores).
    *   **Value Objects**: Identifiable only by their properties (e.g. `GradeResult`). Value objects are immutable.

### 2.6 Repositories & DbContext (Infrastructure Layer)
*   **Behavior**: Persistence layer.
*   **Design Rule**: The Application layer accesses database layers through repository interfaces (e.g., `IGenericRepository<T>`). Entity configurations are written using Entity Framework Core's **Fluent API** (overriding `OnModelCreating`) to isolate schema specifications from class annotations.

---

## 3. Cross-Cutting Services Design

### 3.1 Global Exception Handling
*   **Implementation**: Done via a custom `ExceptionHandlingMiddleware` registered in the Program pipeline.
*   **Behavior**: Intercepts all system errors and transforms them into standard **RFC 7807 ProblemDetails** JSON responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Course credits must be between 1 and 6.",
  "instance": "/api/v1/courses",
  "errors": {
    "credits": [ "Course credits must be between 1 and 6." ]
  }
}
```

*   **Exceptions Mapping**:
    *   `ValidationException` $\rightarrow$ `400 Bad Request` with validation arrays.
    *   `UnauthorizedAccessException` $\rightarrow$ `401 Unauthorized`.
    *   `ForbiddenAccessException` $\rightarrow$ `403 Forbidden`.
    *   `NotFoundException` $\rightarrow$ `404 Not Found`.
    *   `BusinessRuleViolationException` $\rightarrow$ `422 Unprocessable Entity`.
    *   All unhandled exceptions $\rightarrow$ `500 Internal Server Error` (with stack traces logged to Serilog but hidden from client payloads).

### 3.2 Logging (Serilog)
*   **Behavior**: Structured logging.
*   **Design Rule**: Initialize Serilog in `Program.cs`. Logs are formatted in JSON and written to two sinks: Console (for container debugging) and Rolling File (with 30-day retention).
*   **PII Check**: Ensure that request payloads containing passwords, emails, or names are stripped before logs are committed.

### 3.3 Dependency Injection (DI)
*   **Behavior**: Dependency wiring.
*   **Design Rule**: Each clean architecture layer exposes a `DependencyInjection` helper extension class (e.g. `AddApplicationServices()`, `AddInfrastructureServices(IConfiguration config)`), which registers interfaces with their concrete classes in the IoC container.

### 3.4 Caching Strategy
*   **Behavior**: Performance optimization.
*   **Design Rule**:
    *   **Static Data**: Grading tables and translations are stored in-memory using ASP.NET Core `IMemoryCache` (Cache-Aside pattern) with a 24-hour absolute expiration.
    *   **Student GPAs**: Cumulative GPAs are cached per Student ID, invalidated automatically whenever a transaction modifies course scores.

### 3.5 Background Jobs (Quartz.NET)
*   **Behavior**: Housekeeping schedules.
*   **Design Rule**: Quartz.NET manages asynchronous system jobs:
    *   `PurgeExpiredNotificationsJob`: Deletes read notifications older than 1 year. Runs daily at 02:00 AM.
    *   `PurgeExpiredSharedTranscriptsJob`: Identifies and flags expired `SHARED_TRANSCRIPTS` records. Runs hourly.

---

*End of Document — Backend Architecture*
