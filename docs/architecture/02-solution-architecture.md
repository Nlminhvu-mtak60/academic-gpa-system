# 02 — Solution Architecture

> **Document ID**: ARC-SA-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Data pipelines and system communication patterns

---

## 1. Request-Response Lifecycle & Data Pipeline

This document maps out the precise flow of data and execution control as it traverses the solution boundary, starting from the client UI down to data persistence and external integrations.

```mermaid
graph TD
    %% Define components
    UI["React SPA Client<br/>(Tailwind + Vite)"]
    NGINX["Nginx reverse proxy / gateway<br/>(TLS termination, port routing)"]
    
    subgraph "ASP.NET Core Web API (.NET 9)"
        MW["1. Presentation: Middlewares<br/>(Exceptions, Rate Limits, JWT Guard)"]
        CTRL["2. Presentation: Controllers<br/>(Model Binding, Endpoint Mapping)"]
        VAL["3. Application: FluentValidators<br/>(Syntax Check)"]
        MED["4. Application: MediatR<br/>(Command / Query Handler dispatch)"]
        APP_SVC["5. Application: Services<br/>(Orchestration, Domain triggers)"]
        DOM_MOD["6. Domain: Core Entities<br/>(Pure Calculations, Rounding rules)"]
        DB_CTX["7. Infrastructure: DbContext / SQL Repositories<br/>(EF Core 9)"]
    end

    subgraph "External Targets"
        SQL[("SQL Server 2022")]
        AI_SVC["Python FastAPI AI Service"]
    end

    %% Flow arrows
    UI -->|HTTPS Request (JSON)| NGINX
    NGINX -->|Forward /api/v1/*| MW
    MW -->|Parse JWT / Validate session| CTRL
    CTRL -->|Extract Model DTO| VAL
    VAL -->|Valid Syntax| MED
    MED -->|Dispatch Request Handler| APP_SVC
    APP_SVC -->|Execute Business Calculations| DOM_MOD
    DOM_MOD -->|Calculated entities| DB_CTX
    DB_CTX -->|Execute Transaction| SQL
    
    APP_SVC -.->|Async HTTP POST (PII Anonymized)| AI_SVC
```

---

## 2. Layer-by-Layer Execution Flow

### Tier 1: Client Layer (Frontend SPA)
*   **Behavior**: When a student enters course scores, the state is managed in-memory. 
*   **Actions**: The client performs initial validation (verifying scores are between 0.0 and 10.0) and issues an asynchronous HTTP Request to `/api/v1/courses/{id}/scores` with the JSON payload.
*   **State transition**: Displays a loading spinner and handles success/error alerts.

### Tier 2: Gateway Layer (Nginx)
*   **Behavior**: Acts as a reverse proxy, SSL/TLS terminator, and static asset provider.
*   **Actions**: Rejects non-TLS requests, enforces security headers (HSTS, Content Security Policy), and routes `/api/v1/*` traffic to the local ASP.NET Core container port.

### Tier 3: Presentation Layer (ASP.NET Core API)
*   **Behavior**: Entry point of the server application.
*   **Actions**:
    *   **Custom Middlewares**: Catch-all global error interceptors, rate limits checks, and JWT authentication token parsers.
    *   **Controllers**: Routes mapped to API controllers. Extracted Request parameters are bound to validation models.

### Tier 4: Application Layer (MediatR & Core Handlers)
*   **Behavior**: Contains use case orchestration.
*   **Actions**:
    *   **Validators**: FluentValidation rules are run. If values are invalid, it aborts execution and returns an RFC 7807 validation error payload.
    *   **MediatR Pipeline**: Dispatches the action to its specific command or query handler. The handler queries the repository, coordinates changes, and triggers calculations.

### Tier 5: Domain Layer (Pure Business Rules)
*   **Behavior**: Dictates calculation correctness.
*   **Actions**: The entities compute attendance, continuous assessment, and final exam grades using the `GradeResult` value object. Rounds scores to 1-decimal place, checks classifications, and updates academic aggregates.

### Tier 6: Infrastructure Layer (EF Core & SQL Server)
*   **Behavior**: Relational persistence and physical writes.
*   **Actions**: EF Core tracks modifications, executes parameterized queries against the SQL Server database inside a transactional unit, and commits the records.

### Tier 7: AI Integration Subsystem
*   **Behavior**: Specialized calculations and predictions.
*   **Actions**: If the student requests study strategies or forecasts, the C# Application service calls the FastAPI Python microservice over HTTP. The FastAPI service processes the anonymized context, calls the LLM, and passes the parsed result back to C# for output delivery.

---

*End of Document — Solution Architecture*
