# 06 — Milestones

> **Document ID**: PLN-MS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Milestones registry with release dates, target verification methods, and stakeholder sign-off criteria

---

## 1. Document Purpose

This document defines the major milestones for the Academic GPA Management System. It establishes verification methods and sign-off criteria for each milestone to ensure technical stability throughout the development process.

---

## 2. Milestone Timeline Registry

The project tracks seven major milestones. The chart below illustrates the chronological target dates:

```
Week:   02          04          06          08          10          11          12
        [MS-01]     [MS-02]     [MS-03]     [MS-04]     [MS-05]     [MS-06]     [MS-07]
Stage:  Identity    Records     GPA Engine  Dashboard   Admin/Share AI Advisor  Launch
```

---

## 3. Milestone Definition Details

### MS-01: Core Architecture & Identity
*   **Target Completion**: Week 2 (End of Sprint 1)
*   **Release Scope**: Internal Alpha (Sandbox)
*   **Detailed Deliverables**:
    *   Initialize the Clean Architecture .NET solution and EF Core project structure.
    *   Deploy user registration, login, and JWT token rotation endpoints.
*   **Verification Methods**:
    *   Run automated integration tests verifying register/login API calls.
    *   Verify that requests lacking authorization headers are blocked with a `401 Unauthorized` status.
*   **Stakeholder Sign-off Criteria**:
    *   Backend developers confirm database connection stability.
    *   Security lead approves the JWT family rotation logic.

### MS-02: Academic Records API & Auditing
*   **Target Completion**: Week 4 (End of Sprint 2)
*   **Release Scope**: Sandbox API complete
*   **Detailed Deliverables**:
    *   Deploy CRUD endpoints for Academic Years, Semesters, Courses, and component scores.
    *   Configure database audit logs for score adjustments.
*   **Verification Methods**:
    *   Validate API request payload checking via Swagger.
    *   Verify that updates to course score fields generate a matching audit log record in the database.
*   **Stakeholder Sign-off Criteria**:
    *   Lead QA validates that the API restricts semesters to a maximum of 3 per year.
    *   Database administrator confirms audit log performance.

### MS-03: Certified GPA Calculation Engine
*   **Target Completion**: Week 6 (End of Sprint 3)
*   **Release Scope**: Core Engine complete
*   **Detailed Deliverables**:
    *   Implement and verify the GPA calculation, grade conversion, and target planners.
*   **Verification Methods**:
    *   Run unit tests covering the GPA conversions.
    *   Confirm that calculations aggregate GPAs correctly, excluding retaken courses.
*   **Stakeholder Sign-off Criteria**:
    *   **Academic advisor verifies and signs off on the rounding rules (BR-CALC).**
    *   Calculation engine achieving 100% test coverage.

### MS-04: Student Dashboard UI & Theme Shell
*   **Target Completion**: Week 8 (End of Sprint 4)
*   **Release Scope**: Closed Beta (Selected Students)
*   **Detailed Deliverables**:
    *   Build the React Single Page Application (SPA), integrating layout elements, language locales (VI/EN), and theme preferences (Light/Dark).
    *   Connect client components to the API layer to render dashboard charts.
*   **Verification Methods**:
    *   Test theme and language switching across multiple browser environments.
    *   Confirm that dashboard charts load dynamic student data without UI lags.
*   **Stakeholder Sign-off Criteria**:
    *   UI/UX designer signs off on responsiveness and layout consistency.
    *   Beta tester group completes core user flows without encountering blocking errors.

### MS-05: Admin Console & Transcript Sharing
*   **Target Completion**: Week 10 (End of Sprint 5)
*   **Release Scope**: Staging Environment (Release Candidate)
*   **Detailed Deliverables**:
    *   Build the transcript sharing module, including temporary UUID v4 paths and expiration logic.
    *   Build the Admin portal student directory, lock toggle, and broadcast controls.
*   **Verification Methods**:
    *   Verify that access to expired transcript links is blocked.
    *   Confirm that locked accounts are prevented from logging in or obtaining new API tokens.
*   **Stakeholder Sign-off Criteria**:
    *   Product Manager verifies admin tool functions.
    *   Security audit confirms that shared links do not expose internal database IDs.

### MS-06: FastAPI AI Advisor Integration
*   **Target Completion**: Week 11 (Sprint 6 Mid-point)
*   **Release Scope**: Integration Complete
*   **Detailed Deliverables**:
    *   Deploy the Python FastAPI GenAI microservice.
    *   Build the chat sidebar interface in React.
    *   Configure the backend PII filter and rate limiting rules (max 20 messages/hour).
*   **Verification Methods**:
    *   Intercept outbound requests to verify that student names, emails, and codes are stripped.
    *   Confirm that the API blocks requests when user chat counts exceed 20 within a rolling 60-minute window.
*   **Stakeholder Sign-off Criteria**:
    *   Security lead confirms data privacy compliance.
    *   AI engineer signs off on prompt template styling and chatbot response times.

### MS-07: General Availability (GA) Release
*   **Target Completion**: Week 12 (End of Sprint 6)
*   **Release Scope**: Production Launch (Public Release)
*   **Detailed Deliverables**:
    *   Package frontend and backend services into Docker containers and deploy to cloud environments.
    *   Complete performance profiling and final security checks.
*   **Verification Methods**:
    *   Execute load tests simulating 1,000 concurrent user sessions.
    *   Verify SSL/TLS enforcement across all endpoints.
*   **Stakeholder Sign-off Criteria**:
    *   Engineering Manager confirms zero open P0/P1 bugs.
    *   All stakeholders approve release, and system documentation is finalized.

---

*End of Document — Milestones*
