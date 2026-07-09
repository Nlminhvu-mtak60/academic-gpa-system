# 05 — Infrastructure Layer Design

> **Document ID**: ARC-BE-INF-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Persistence DbContext design and external service clients

---

## 1. Persistence & Entity Framework Core

The Infrastructure Layer (`AcademicGPA.Infrastructure`) manages SQL Server database persistence using **Entity Framework Core 9**.

### 1.1 DbContext Design (`ApplicationDbContext`)
*   Implements the `IApplicationDbContext` interface defined in the Application layer, keeping the database layer decoupled.
*   Enforces transaction boundaries, exposing `BeginTransactionAsync()`, `CommitTransactionAsync()`, and `RollbackTransactionAsync()`.
*   **Audit Fields Automation**: Overrides `SaveChangesAsync` to set tracking metadata (`CreatedAt`, `UpdatedAt`) automatically on modified entities before writing to the database.
*   **Global Query Filters**: Configures soft-delete filters:
    ```csharp
    modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
    ```

### 1.2 Entity Configurations (Fluent API)
To keep entity classes clean, mapping metadata (table names, column types, check constraints, foreign keys) is isolated inside a dedicated `/Configurations` folder:
*   Classes inherit from `IEntityTypeConfiguration<T>`.
*   Example: `CourseConfiguration.cs` configures the relation to Semesters and maps decimal precision rules.

---

## 2. External Service Clients

The Infrastructure layer implements the integration interfaces defined in the Application layer:

### 2.1 SMTP Email Client (`EmailService`)
*   Implements `IEmailService`.
*   Manages transactional email delivery (verification codes, password reset links) using SMTP configurations.

### 2.2 Google Identity Client (`GoogleAuthService`)
*   Implements `IGoogleAuthService`.
*   Validates Google id tokens and authorization codes against Google's API endpoints.

### 2.3 AI Advisor API Client (`AiAdvisorService`)
*   Implements `IAiAdvisorService`.
*   Manages HTTP REST calls to the FastAPI Python microservice. It uses `HttpClient` with retry policies (using **Polly**) to handle transient network drops.

---

## 3. Cryptographic Services

*   **Password Hashing**: Implemented using `BCrypt.Net-Next`. Passwords are encrypted using standard salt rounds ($\ge 12$).
*   **JWT Token Generator**: Generates cryptographically secure access tokens signed with HMAC SHA-256 keys.

---

*End of Document — Infrastructure Layer Design*
