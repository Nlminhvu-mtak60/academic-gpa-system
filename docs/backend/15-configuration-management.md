# 15 — Configuration Management

> **Document ID**: ARC-BE-CONF-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Configuration options and environment variable structures

---

## 1. Strongly-Typed Settings (Options Pattern)

The backend uses ASP.NET Core's **Options Pattern** to bind configuration sections to strongly-typed classes. These classes are registered in the DI container and injected into services using `IOptions<T>` or `IOptionsSnapshot<T>` (for configurations that support runtime updates).

---

## 2. Configuration Options Schema

---

### 2.1 Core Configuration Options
The following strongly-typed options classes are defined in the Application layer:

*   **JwtOptions**:
    *   `Secret`: Signing key (loaded from environment variables).
    *   `Issuer`: Token issuer (`gpa-api-server`).
    *   `Audience`: Token audience (`gpa-client-app`).
    *   `ExpiryMinutes`: Token validity length (15).
*   **GoogleAuthOptions**:
    *   `ClientId`: Google OAuth client identifier.
    *   `ClientSecret`: Google OAuth client secret.
*   **SmtpOptions**:
    *   `Host` / `Port`: SMTP server connection settings.
    *   `Username` / `Password`: SMTP credentials.
    *   `FromEmail`: Sender email address.
*   **AiServiceOptions**:
    *   `BaseUrl`: FastAPI service endpoint URL.
    *   `ApiKey`: Shared key used to authenticate requests to the AI service.

---

## 3. Secrets Isolation Strategy

*   **Development**: Sensitive keys are stored in the developer's local User Secrets store (`secrets.json`) on their workstations. This file is kept outside the git workspace.
*   **Production**: Credentials are set as environment variables on the host system or injected from a key vault during container startup.

---

## 4. Startup Validation Rules

To prevent the application from running with invalid configurations, options validation is executed during container startup:
*   Classes implement data annotations (e.g., `[Required]`, `[Range]`) or use custom validation rules.
*   If a critical setting is missing (e.g. JWT secret key is null), the application fails startup and exits with an error message: "Configuration Validation Error: JWT Secret is required."

---

*End of Document — Configuration Management*
