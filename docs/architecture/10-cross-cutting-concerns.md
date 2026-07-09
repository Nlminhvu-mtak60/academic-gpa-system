# 10 — Cross-cutting Concerns

> **Document ID**: ARC-CCC-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Core system configurations and service parameters

---

## 1. Document Purpose

This document details how cross-cutting concerns—operational tasks that affect multiple layers of the application—are implemented across the system. Standardizing these behaviors prevents code duplication and ensures system consistency.

---

## 2. Structured System Configurations

---

### 2.1 Configuration Management & Secrets Isolation
*   **Layer Rules**: Configurations are stored in hierarchical settings files and environment variables.
*   **Files**:
    *   `appsettings.json`: Static, non-sensitive parameters (e.g. CORS whitelist domains, token lifetime values).
    *   `appsettings.Development.json`: Local development settings.
*   **Secrets Storage**: Sensitive keys (such as DB connection strings, JWT signing keys, and Google client secrets) must be loaded from **Environment Variables** in production or local User Secrets (`secrets.json`) during development. Under no circumstances are credentials committed to Git.

---

### 2.2 Dependency Injection Lifetimes (.NET IoC)
Service registrations must follow explicit lifetime rules to prevent memory leaks and threading issues:

1.  **Transient (`AddTransient<T, U>`)**: Used for lightweight, stateless operations. A new instance is created for every request (e.g., mapping profiles, validator rule classes).
2.  **Scoped (`AddScoped<T, U>`)**: Used for stateful services within an HTTP request boundary. The instance is shared across classes within the same request lifecycle (e.g., repository wrappers, DB Contexts, handlers).
3.  **Singleton (`AddSingleton<T, U>`)**: The service instance is created once and shared globally. Used for stateless utilities, cache clients, or background scheduling registries.

---

### 2.3 Comprehensive Validation Architecture
The system uses a double-sided validation strategy:

*   **Frontend Validation (Zod + React Hook Form)**: Verifies data formats in the browser (e.g. checking email formats, ensuring password length is $\ge 8$, and blocking string inputs on score fields).
*   **Backend Validation (FluentValidation + Pipeline Behavior)**: Ensures input integrity on the server. If validation fails, it throws a `ValidationException`, which is caught by the middleware and returned as an RFC 7807 error payload.

---

### 2.4 Localization (i18n)
*   **Frontend**: Handled via `react-i18next`. Static text assets are stored in JSON locale dictionaries.
*   **Backend**: The API reads the `Accept-Language` HTTP request header. If the header specifies `vi` or `en`, error messages and advisor prompts are localized accordingly.

---

### 2.5 Monitoring & Health Checks
*   **Endpoints**:
    *   `/health/live` (Liveness probe): Returns `200 OK` if the container process is running. Used by orchestrators to monitor container health.
    *   `/health/ready` (Readiness probe): Verifies that dependencies (SQL Server, FastAPI) are reachable. Returns `200 OK` if all services are online, or `503 Service Unavailable` if a dependency fails.
*   **Prometheus Metrics**: Exposes endpoints (`/metrics`) to track API request rates, database query latencies, and server memory utilization.

---

*End of Document — Cross-cutting Concerns*
