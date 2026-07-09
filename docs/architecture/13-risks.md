# 13 — Architectural Risks

> **Document ID**: ARC-RISK-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Risk register with architectural mitigations

---

## 1. Document Purpose

This document identifies and analyzes architectural, technical, and maintainability risks inherent in the selected solution design. It outlines proactive mitigations to protect system integrity as the codebase grows.

---

## 2. Technical & Architectural Risk Register

---

### RS-ARC-01: Clean Architecture Over-Engineering & Complexity
*   **Description**: Clean architecture separates code into multiple projects (`Domain`, `Application`, `Infrastructure`, `API`). Developers may struggle with the boilerplate required to implement simple CRUD operations (e.g. creating a Command, a Query, a Handler, and multiple DTOs just to read one table).
*   **Impact**: Slower feature delivery during early sprints.
*   **Mitigation**:
    1.  **Code Scaffolding**: Provide template files and CLI generators to bootstrap feature slices.
    2.  **Simple CRUD bypass**: For simple operations that do not contain complex business rules, use direct EF Core queries in handlers instead of writing redundant repository wrappers.

---

### RS-PERF-01: Database Lock Contention on GPA Recalculation
*   **Description**: GPA recalculations are triggered on every score update, recalculating aggregates across a student's entire academic history. Under high concurrent write loads (e.g. at the end of a semester), this can cause database lock contention.
*   **Impact**: Slow API response times and database timeouts.
*   **Mitigation**:
    1.  **Indexed Aggregations**: Ensure foreign keys are indexed and use optimized SQL views for GPA queries.
    2.  **Optimistic Concurrency**: Use row version tokens (`rowversion`) in SQL Server to handle concurrent score modifications gracefully.

---

### RS-SEC-01: Leakage of Anonymized Academic Records
*   **Description**: When querying the AI advisor, student records are sent to external APIs (OpenAI/Gemini). If the anonymization layer fails, student names or emails could be leaked to third-party logs.
*   **Impact**: Privacy violations and compliance failures.
*   **Mitigation**:
    1.  **Strict Serialization Contracts**: Use dedicated anonymized DTOs for the AI service payload, completely excluding the `User` database entity.
    2.  **Service Isolation**: Run the anonymization process in the C# Application layer, validating the payload structure before it leaves the server boundary.

---

### RS-MAINT-01: Monorepo Dependency Drift
*   **Description**: The frontend and backend reside in the same repository. Over time, dependencies (npm packages and NuGet packages) may drift, causing version conflicts and build failures.
*   **Impact**: Build breaks and security vulnerabilities in production.
*   **Mitigation**:
    1.  **Dependabot Alerts**: Configure Dependabot to scan the repository daily, raising pull requests to patch vulnerable dependencies automatically.
    2.  **Locked Package Versions**: Enforce strict package lock files (`package-lock.json` and `Directory.Packages.props` for NuGet) to ensure reproducible builds across environments.

---

*End of Document — Architectural Risks*
