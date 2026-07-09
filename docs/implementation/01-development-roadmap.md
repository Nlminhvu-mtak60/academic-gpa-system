# 01 — Development Roadmap

> **Document ID**: PLN-RDMP-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: High-level execution strategy, implementation phases, and core developer constraints

---

## 1. Document Purpose

This document outlines the high-level execution strategy and chronological roadmap for the development of the Academic GPA Management System. It establishes the sequential progression of phases from initial environment setup to final launch, and defines the core engineering constraints and implementation rules that must govern all future coding tasks.

---

## 2. Core Implementation Rules

To ensure code quality, architectural integrity, and reliability across the lifetime of the project, all developers and automated pipelines must strictly adhere to the following rules:

*   **Rule 1: Never Skip Tests**  
    Every bug fix, domain service change, application handler, and React view component must be accompanied by relevant test suites. Any pull request containing modified code without matching tests will be automatically blocked.
*   **Rule 2: Never Violate Clean Architecture**  
    The backend architecture must respect the strict inward-flowing dependency boundary:  
    `Domain` $\leftarrow$ `Application` $\leftarrow$ `Infrastructure` $\leftarrow$ `Presentation (API)`.  
    No repository implementation details, database models, or external infrastructure components (e.g., direct HttpClient calls, EF Core namespace imports) may leak into the Domain or Application layers.
*   **Rule 3: Never Bypass Validation**  
    No business entity can be saved to the database without complete verification. All requests passing through the API must trigger MediatR validation pipelines using `FluentValidation` validator classes. Client-side forms must enforce frontend schemas matching these backend rules.
*   **Rule 4: Never Duplicate Business Logic**  
    The **GPA Calculation Engine** and its associated conversion matrix are the single source of truth for all grades. Under no circumstances should GPA letter mapping or decimal rounding logic be duplicated or rewritten in controller actions, database triggers, or React UI views.
*   **Rule 5: Always Update Documentation**  
    If a change modifies an API payload contract, database schema, configuration key, or environment variable, the corresponding design documents, data dictionary, and Swagger annotations must be updated in the same pull request.
*   **Rule 6: Always Maintain API Contract**  
    APIs must enforce strict semantic versioning. Path prefixes (`/api/v1/...`) must remain consistent, and response structures must match the defined schemas to ensure front-and-back compatibility during decoupled staging.

---

## 3. Phased Execution Lifecycle

The development lifecycle spans **12 calendar weeks**, divided into four logical phases. This schedule manages technical risk by finalizing core services and integrations before introducing complex GenAI interfaces.

```
+--------------------------------------------------------------------------------+
| WEEKS 1-4: Phase 1 (Backend Foundation, Identity Services & Core Academic APIs) |
+--------------------------------------------------------------------------------+
                                       |
                                       v
+--------------------------------------------------------------------------------+
| WEEKS 5-8: Phase 2 (React Shell, Calculation Engine Integration & Dashboard)   |
+--------------------------------------------------------------------------------+
                                       |
                                       v
+--------------------------------------------------------------------------------+
| WEEKS 9-10: Phase 3 (FastAPI AI microservice, Admin Console & Sharing System)  |
+--------------------------------------------------------------------------------+
                                       |
                                       v
+--------------------------------------------------------------------------------+
| WEEKS 11-12: Phase 4 (Security Hardening, Load Testing & Production Release)  |
+--------------------------------------------------------------------------------+
```

### Phase 1: Core Backend & Data Infrastructure (Weeks 1 - 4)
*   **Focus**: Establishing the database schema, data access models, central routing, user management, and baseline API testing.
*   **Key Activities**:
    1.  Initialize database projects and Entity Framework Core migrations.
    2.  Implement JWT generation, validation middleware, and Google OAuth payload parsing.
    3.  Develop REST endpoints for Years, Semesters, Courses, and component scores.
    4.  Deliver Swagger/OpenAPI documentation.

### Phase 2: Client Portal & Core Calculation (Weeks 5 - 8)
*   **Focus**: Building the React Single Page Application (SPA), integrating the API layer, and ensuring the absolute accuracy of the GPA Calculation Engine.
*   **Key Activities**:
    1.  Create the client routing system, multi-theme context, and multi-language translations.
    2.  Write unit tests for the **GPA Calculation Engine** using the BR-CALC specification.
    3.  Implement the React dashboard with visual charts showing GPA trends and credit progress.
    4.  Deliver the "What-If" simulator and final exam prediction panels.

### Phase 3: AI Advisor, Admin & Sharing Channels (Weeks 9 - 10)
*   **Focus**: Integrating the FastAPI AI microservice, setting up administrative tools, and finalizing sharing capabilities.
*   **Key Activities**:
    1.  Deploy the Python FastAPI service, securing it with local API keys.
    2.  Create the anonymizer service in the .NET Application layer to sanitize chat request context.
    3.  Build the student transcript public viewer with expiring UUID paths.
    4.  Develop the Admin student portal, account locks, and system broadcast notifications.

### Phase 4: Verification, Security Audits & Deploy (Weeks 11 - 12)
*   **Focus**: Quality assurance, security hardening, user acceptance testing, and final launch.
*   **Key Activities**:
    1.  Perform vulnerability scanning, checking BCrypt hashing and JWT signature rules.
    2.  Run performance benchmarks on database indexes.
    3.  Deploy production Docker images to cloud servers (Azure/AWS).
    4.  Conduct dry-run releases and hand over system documentation.

---

*End of Document — Development Roadmap*
