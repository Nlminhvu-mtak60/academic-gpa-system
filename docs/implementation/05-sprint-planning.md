# 05 — Sprint Planning

> **Document ID**: PLN-SPR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Scrum development plan with sprint-level objectives, features, dependencies, and exit criteria

---

## 1. Document Purpose

This document translates the project phases into six two-week sprints. Each sprint includes a sprint goal, features, specific deliverables, dependencies, and exit criteria to guide development.

---

## 2. Sprint Schedule Overview

```
Week:   01   02   03   04   05   06   07   08   09   10   11   12
        [Sprint 1] [Sprint 2] [Sprint 3] [Sprint 4] [Sprint 5] [Sprint 6]
Sprint:   S01        S02        S03        S04        S05        S06
```

*   **Sprint 1**: Secure Foundations (Weeks 1 - 2)
*   **Sprint 2**: Academic Records & Auditing (Weeks 3 - 4)
*   **Sprint 3**: Calculations & Goals Engine (Weeks 5 - 6)
*   **Sprint 4**: Interactive Student Portal (Weeks 7 - 8)
*   **Sprint 5**: Security Boundaries, Admin & Sharing (Weeks 9 - 10)
*   **Sprint 6**: AI Advising & Production Hardening (Weeks 11 - 12)

---

## 3. Detailed Sprint Specifications

### Sprint 1: Secure Foundations
*   **Sprint Goal**: Establish the Clean Architecture solution structure, configure database migrations, and deploy authentication APIs.
*   **Features**:
    *   Initialize C# solutions (`Domain`, `Application`, `Infrastructure`, `API`) and the database schema.
    *   Create registration, login, and JWT token rotation pipelines.
*   **Deliverables**:
    *   EF Core migration scripts for user credentials.
    *   Endpoints: `/api/v1/auth/register`, `/api/v1/auth/login`, `/api/v1/auth/refresh`.
    *   Token validation middleware.
*   **Dependencies**: None.
*   **Exit Criteria**:
    *   Database connection pooling is verified.
    *   API returns valid JWT access and refresh tokens.
    *   Unit tests check token expiration and invalid credentials.

### Sprint 2: Academic Records & Auditing
*   **Sprint Goal**: Implement the student profile management service and build CRUD APIs for academic records with audit logging.
*   **Features**:
    *   Allow students to manage profiles (student code, major, university, credits).
    *   Create academic structure endpoints (Years, Semesters, Courses).
    *   Implement grade component score records and change tracking logs.
*   **Deliverables**:
    *   Database entities for years, semesters, courses, and component scores.
    *   CRUD API endpoints with FluentValidation rules.
    *   Audit log triggers tracking updates to score fields.
*   **Dependencies**: Sprint 1 completion.
*   **Exit Criteria**:
    *   Validation limits semester creation to 3 per year.
    *   Course score entries are checked for attendance, continuous, and final weights (summing to 100%).
    *   Audit logs record previous and new scores on updates.

### Sprint 3: Calculations & Goals Engine
*   **Sprint Goal**: Build and verify the GPA calculation algorithms, prediction engine, and target GPA back-calculators.
*   **Features**:
    *   Implement GPA conversion tables and letter grade mapping rules (A+ to F).
    *   Implement cumulative and semester GPA aggregation, handling course retakes.
    *   Create prediction systems for required final exams and semester GPAs.
*   **Deliverables**:
    *   `GpaCalculator` domain service with unit tests.
    *   Endpoints: `/api/v1/goals/gpa-target`, `/api/v1/prediction/final-exam`.
    *   "What-If" prediction models.
*   **Dependencies**: Sprint 2 completion.
*   **Exit Criteria**:
    *   100% test coverage on GPA calculations, matching the BR-CALC specifications.
    *   Calculation checks verify that only the highest attempt of a retaken course is included in the cumulative GPA.
    *   Required grade calculations return valid output ranges ($0.0 \le \text{Score} \le 10.0$).

### Sprint 4: Interactive Student Portal
*   **Sprint Goal**: Build the React Single Page Application (SPA), set up global state management, and integrate the dashboard and charts.
*   **Features**:
    *   Configure client routing, light/dark themes, and multi-language support (English/Vietnamese).
    *   Build the main dashboard with statistics widgets.
    *   Render GPA trend and grade distribution charts.
    *   Develop the interactive "What-If" simulator interface.
*   **Deliverables**:
    *   React client shell configured with Vite and Axios.
    *   Home dashboard rendering trend charts.
    *   Theme switcher and translation translation files.
    *   Interactive calculator panel.
*   **Dependencies**: Sprint 3 completion.
*   **Exit Criteria**:
    *   Axios interceptors manage token expiration and page refreshes without loss of state.
    *   All layouts are responsive across mobile, tablet, and desktop dimensions.
    *   All components follow the specified Color System and Typography design guidelines.

### Sprint 5: Security Boundaries, Admin & Sharing
*   **Sprint Goal**: Deploy public transcript sharing links, build the admin dashboard, and implement in-app notifications.
*   **Features**:
    *   Create secure UUID v4 links for transcript sharing, with expiration controls.
    *   Build the admin student directory (search, filter, and lock options).
    *   Develop the system alert polling queue (30s intervals).
*   **Deliverables**:
    *   Sharing endpoints: `/api/v1/share/` (POST, GET, DELETE).
    *   Admin user account directory panel.
    *   Polling notification header component in React.
*   **Dependencies**: Sprint 4 completion.
*   **Exit Criteria**:
    *   Public viewer displays student transcripts using the print styling template.
    *   Inactive or locked accounts are immediately blocked from obtaining new API tokens.
    *   Database cleanups successfully purge notifications older than 1 year.

### Sprint 6: AI Advising & Production Hardening
*   **Sprint Goal**: Connect the FastAPI GenAI microservice, enforce rate limits, complete security audits, and deploy to production.
*   **Features**:
    *   Deploy the Python FastAPI microservice to process LLM prompts.
    *   Implement the backend PII filter to anonymize student details.
    *   Limit chat requests to 20 messages per hour.
    *   Deploy the application using production Docker containers.
*   **Deliverables**:
    *   FastAPI application package.
    *   Anonymization middleware in the .NET Application layer.
    *   React chat sidebar panel.
    *   Docker Compose configuration files.
*   **Dependencies**: Sprint 5 completion.
*   **Exit Criteria**:
    *   The backend anonymizer filters out user names, emails, and student codes before requests reach FastAPI.
    *   Chat logs are deleted within 24 hours of a student deleting a conversation.
    *   Docker container health checks return success status on staging and production environments.

---

*End of Document — Sprint Planning*
