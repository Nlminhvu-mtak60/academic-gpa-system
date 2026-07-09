# 06 — Presentation Layer Design

> **Document ID**: ARC-BE-PRES-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Controllers configurations and API hosting setups

---

## 1. Web API Controllers Layout

The Presentation Layer (`AcademicGPA.API`) is the entry point of the server application.

### 1.1 ApiControllerBase Configuration
All API controllers inherit from a base controller class (`ApiControllerBase`):
*   Decorated with `[ApiController]` and `[Route("api/v1/[controller]")]` attributes.
*   Injects MediatR's `ISender` using property injection to keep controller constructors clean.

```csharp
[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
```

*   **Design Rule**: Controllers must only map routes and dispatch commands/queries via `Mediator.Send(command)`. They must contain zero business logic.

---

## 2. Middleware Execution Pipeline

HTTP requests pass through a sequence of custom middlewares registered in the application builder pipeline:

```
HTTP Request
   ├── 1. ExceptionHandlingMiddleware (Global Try-Catch Wrapper)
   ├── 2. SerilogRequestLogging (Audit logs headers and routes)
   ├── 3. CorsMiddleware (Checks whitelist domains)
   ├── 4. RateLimitingMiddleware (Enforces IP-based request limits)
   ├── 5. AuthenticationMiddleware (Validates JWT signatures)
   ├── 6. AuthorizationMiddleware (Checks user claims & policies)
   └── 7. Endpoint Routing (Executes Controller Action)
```

---

## 3. Custom Middlewares Specifications

### 3.1 ExceptionHandlingMiddleware
*   Wraps the entire request execution in a `try-catch` block.
*   Catches exceptions and maps them to standard **RFC 7807 ProblemDetails** JSON responses (see [Backend Architecture](./../architecture/04-backend-architecture.md#31-global-exception-handling)).

### 3.2 Request Logging Middleware
*   Logs request details (HTTP method, route, status code, execution duration).
*   Masks sensitive parameters (e.g. passwords, client secrets) before writing logs to Serilog.

---

## 4. Swagger / OpenAPI Customization
*   Enables Swagger generation to document the API.
*   Configures JWT Bearer authentication security definitions in Swagger UI, allowing developers to test protected routes directly from their browsers.

---

*End of Document — Presentation Layer Design*
