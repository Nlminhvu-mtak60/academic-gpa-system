# 02 — Module Breakdown

> **Document ID**: PLN-MOD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Module specifications including objectives, dependency maps, risks, and concrete deliverables

---

## 1. Document Purpose

This document details the functional decomposition of the Academic GPA Management System into 18 independent modules. Each module is evaluated for technical complexity, development effort, dependencies, implementation risks, and specific deliverables.

---

## 2. Module Specifications

### Module 1: Foundation
*   **Objective**: Build the core system architecture, including solution organization, Entity Framework database contexts, shared utility functions, standard response wrappers, and global exception middlewares.
*   **Dependencies**: None.
*   **Estimated Complexity**: Low
*   **Estimated Development Effort**: 3 Days
*   **Risks**: Changes to common libraries or base classes later in the project can cause widespread refactoring.
*   **Deliverables**:
    *   `AcademicGPA.sln` solution file.
    *   Shared projects (`Domain`, `Application`, `Infrastructure`, `API`).
    *   Database context implementation with connection pool parameters.
    *   Global Exception Handler middleware.

### Module 2: Authentication
*   **Objective**: Manage user registration, verify user emails, and issue and refresh security tokens.
*   **Dependencies**: Foundation.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 5 Days
*   **Risks**: Security vulnerabilities if token validation or session expiration rules are misconfigured.
*   **Deliverables**:
    *   User and RefreshToken database tables.
    *   BCrypt password hashing handlers.
    *   Endpoints: `/api/v1/auth/register`, `/api/v1/auth/login`, `/api/v1/auth/refresh`.
    *   Email dispatcher integration for account verification.

### Module 3: Authorization
*   **Objective**: Enforce role-based access controls (RBAC) across public, Student, and Admin endpoints. Includes Google OAuth integrations.
*   **Dependencies**: Authentication.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 3 Days
*   **Risks**: Misconfigured middleware could allow unauthorized access to administrative features.
*   **Deliverables**:
    *   Custom authorization handlers for `Student` and `Admin` roles.
    *   Google token validation service.
    *   Endpoint: `/api/v1/auth/google`.
    *   Route authorization integration.

### Module 4: Student Management (Profile Module)
*   **Objective**: Maintain profile records for students including name, student code, major, university, and credits required for graduation.
*   **Dependencies**: Authentication, Authorization.
*   **Estimated Complexity**: Low
*   **Estimated Development Effort**: 3 Days
*   **Risks**: Data corruption or duplicate Student Codes if unique constraint mappings are not properly enforced.
*   **Deliverables**:
    *   `StudentProfile` database schema.
    *   Profile update handlers.
    *   Endpoint: `/api/v1/profile` (GET, PUT).

### Module 5: Academic Year
*   **Objective**: Define and structure academic years (e.g., "2024-2025") to group course records chronologically.
*   **Dependencies**: Student Management.
*   **Estimated Complexity**: Low
*   **Estimated Development Effort**: 2 Days
*   **Risks**: Overlapping year ranges if start/end year logic validation is missing.
*   **Deliverables**:
    *   `AcademicYear` entity mapping.
    *   Create, Read, Update, Delete (CRUD) endpoints for Academic Years.
    *   Sorting logic by start year.

### Module 6: Semester
*   **Objective**: Manage semesters within each academic year, ensuring a limit of 3 semesters per year (e.g., Fall, Spring, Summer).
*   **Dependencies**: Academic Year.
*   **Estimated Complexity**: Low
*   **Estimated Development Effort**: 2 Days
*   **Risks**: Bypassing the maximum 3 semesters per year restriction via parallel API requests.
*   **Deliverables**:
    *   `Semester` database schema.
    *   Business rules checking maximum semester limits.
    *   CRUD endpoints for semesters.

### Module 7: Course
*   **Objective**: Manage course listings under semesters, recording details like credit weight (1-6) and linking retaken courses to past attempts.
*   **Dependencies**: Semester.
*   **Estimated Complexity**: Low
*   **Estimated Development Effort**: 4 Days
*   **Risks**: Invalid credit ranges or cyclic references in retaken course mappings.
*   **Deliverables**:
    *   `Course` database entities.
    *   Course retake assignment handlers.
    *   CRUD endpoints for Courses under semesters.

### Module 8: Grade (Component Scores)
*   **Objective**: Record raw score components for courses (Attendance, Continuous, and Final Exam scores).
*   **Dependencies**: Course.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 4 Days
*   **Risks**: Inaccurate score entries or performance bottlenecks during bulk updates.
*   **Deliverables**:
    *   `Score` database entities.
    *   Validation checks for component weight splits (must sum to 100%).
    *   Score updating endpoints with audit logging.

### Module 9: GPA Engine ⭐ CRITICAL
*   **Objective**: Convert component scores to course grades, map scores to letter marks (A+ to F), and calculate semester and cumulative GPAs on both 10-scale and 4-scale.
*   **Dependencies**: Grade.
*   **Estimated Complexity**: High
*   **Estimated Development Effort**: 6 Days
*   **Risks**: Edge cases in rounding (BR-CALC rules) or failing to exclude retaken courses from GPA counts.
*   **Deliverables**:
    *   `GpaCalculator` domain service.
    *   Letter grade mapping tables.
    *   Calculations for Semester, Year, and Cumulative GPA.
    *   Unit tests covering all calculation scenarios.

### Module 10: Dashboard
*   **Objective**: Build the student home screen showing GPA stats, upcoming targets, and recent alerts.
*   **Dependencies**: GPA Engine, Notification.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 5 Days
*   **Risks**: Excessive database queries if summary metrics are not loaded efficiently.
*   **Deliverables**:
    *   Unified API endpoint: `/api/v1/dashboard/summary`.
    *   Home page interface in React with light/dark theme.
    *   Loading skeletons and error handling states.

### Module 11: Statistics
*   **Objective**: Render visual charts representing academic progress, grade distributions, and total credits earned.
*   **Dependencies**: GPA Engine.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 4 Days
*   **Risks**: Large calculation lags or issues rendering charts on smaller screens.
*   **Deliverables**:
    *   API endpoint: `/api/v1/statistics`.
    *   GPA trend line chart in React.
    *   Grade distribution bar/pie charts.
    *   Progress trackers for degree credit completion.

### Module 12: Goal Planner
*   **Objective**: Enable students to set target cumulative GPAs and calculate the required GPA for remaining semesters.
*   **Dependencies**: GPA Engine.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 4 Days
*   **Risks**: Division-by-zero errors if there are no remaining semesters left in the academic plan.
*   **Deliverables**:
    *   `GpaGoal` database schema.
    *   Back-calculation algorithms for target GPAs.
    *   "What-If" simulator interface.
    *   API endpoint: `/api/v1/goals`.

### Module 13: Prediction Engine
*   **Objective**: Calculate the required Final Exam score for a course to achieve a desired letter grade, based on existing attendance and continuous scores.
*   **Dependencies**: Grade.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 3 Days
*   **Risks**: Mathematical overflows or returning impossible score targets (e.g., $> 10$ or $< 0$).
*   **Deliverables**:
    *   Predictive scoring calculator service.
    *   API endpoint: `/api/v1/prediction/final-exam`.
    *   UI panel for exam score calculations.

### Module 14: Notification
*   **Objective**: Manage in-app notification alerts, badges, and periodic polling queues.
*   **Dependencies**: Foundation, Authorization.
*   **Estimated Complexity**: Low
*   **Estimated Development Effort**: 3 Days
*   **Risks**: High server load from frequent client polling requests.
*   **Deliverables**:
    *   `Notification` database table.
    *   API endpoints: `/api/v1/notifications` (GET, PATCH).
    *   React header badge with polling configuration (30s intervals).

### Module 15: Settings
*   **Objective**: Manage localization (English and Vietnamese) and UI theme modes (Light and Dark).
*   **Dependencies**: Student Management.
*   **Estimated Complexity**: Low
*   **Estimated Development Effort**: 2 Days
*   **Risks**: State mismatches during page reloads.
*   **Deliverables**:
    *   Application configuration services.
    *   Translation file loaders (`en.json`, `vi.json`).
    *   React state synchronization handlers for theme and locale.

### Module 16: Share (Transcript Sharing)
*   **Objective**: Generate secure public links (using UUID v4 keys) to share transcripts, with expiration and revocation controls.
*   **Dependencies**: GPA Engine.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 3 Days
*   **Risks**: Data leaks if expired links remain accessible or if internal IDs are exposed.
*   **Deliverables**:
    *   `SharedTranscript` entity.
    *   Endpoints: `/api/v1/share` (POST, DELETE), `/api/v1/share/{uuid}` (public GET).
    *   Public viewer interface matching print styles.

### Module 17: Admin
*   **Objective**: Allow administrators to monitor user counts, search student directories, lock/unlock accounts, reset passwords, and send announcements.
*   **Dependencies**: Authentication, Authorization, Notification.
*   **Estimated Complexity**: Medium
*   **Estimated Development Effort**: 5 Days
*   **Risks**: Accidental privilege escalation or administrative actions leaking into student routes.
*   **Deliverables**:
    *   Admin user database credentials and management scripts.
    *   Student listing directory API with filter and sort capabilities.
    *   Account locking and broadcast endpoints.
    *   Admin dashboard interface.

### Module 18: AI Advisor
*   **Objective**: Provide an interactive chat assistant for academic advice, using the FastAPI Python microservice to communicate with LLM APIs.
*   **Dependencies**: GPA Engine, Foundation.
*   **Estimated Complexity**: High
*   **Estimated Development Effort**: 7 Days
*   **Risks**: Leaking student PII to LLMs, high API costs, or API timeout errors.
*   **Deliverables**:
    *   FastAPI Python gateway application.
    *   Data anonymization filter in the .NET Application layer.
    *   Rate limiting service (max 20 requests/hour).
    *   React chat sidebar panel.

---

*End of Document — Module Breakdown*
