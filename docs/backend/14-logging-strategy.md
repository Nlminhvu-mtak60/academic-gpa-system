# 14 — Logging Strategy

> **Document ID**: ARC-BE-LOG-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Logging configurations and PII masking rules

---

## 1. Structured Logging with Serilog

The backend uses **Serilog** to write structured, machine-readable logs in JSON format. This simplifies debugging and allows log aggregation in cloud environments.

---

## 2. Logging Destinations (Sinks)

Logs are dispatched to three targets:
1.  **Console Sink**: Styled console output for local development.
2.  **Rolling File Sink**: Writes logs to daily rotating files with a 30-day retention limit.
3.  **Cloud Sink (Application Insights / Seq)**: Dispatches structured JSON log streams to cloud monitoring services in staging and production.

---

## 3. Log Properties & Correlation IDs

Every log entry includes correlation properties to trace requests across services:
*   `CorrelationId`: A unique request ID injected by the middleware.
*   `UserId` / `StudentProfileId`: Identifies the authenticated user who initiated the request.
*   `RequestPath` / `RequestMethod`: HTTP route and method details.

---

## 4. Personal Identifying Information (PII) Masking

To comply with data privacy regulations (GDPR and Vietnam Decree 13/2023/ND-CP), the logging pipeline must prevent sensitive data from being written to logs:

### 4.1 Masking Implementation
*   **Request Body Masking**: A custom Serilog Destructuring Policy intercepts log arguments.
*   **Sensitive Properties**: Properties containing passwords (`password`, `currentPassword`, `newPassword`), auth codes (`authCode`), or access tokens are masked with asterisks (`***`) before being written to log sinks.
*   **Log message structure**:
    ```
    // Correct structured log
    Log.Information("Processed user login request for Email: {Email}", email);
    ```

---

*End of Document — Logging Strategy*
