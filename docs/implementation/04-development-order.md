# 04 — Development Order

> **Document ID**: PLN-ORD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Sequential implementation phases with dependency justifications and stage gate requirements

---

## 1. Document Purpose

This document establishes the chronological sequence of implementation for the Academic GPA Management System. By organizing development into distinct phases, it minimizes integration risk, allows for early database testing, and ensures core calculation components are verified before building frontend views.

---

## 2. Chronological Implementation Phases

The project is structured into **six sequential development phases**. Below is the sequence overview, followed by detailed phase briefs.

```
+-------------------------------------------------------------+
| Phase 1: Core Foundation & Identity (Weeks 1 - 2)           |
| (Modules 1, 2, 3, 4)                                        |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
| Phase 2: Academic Record-Keeping (Weeks 3 - 4)              |
| (Modules 5, 6, 7, 8)                                        |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
| Phase 3: Calculations, Simulation & Goals (Weeks 5 - 6)     |
| (Modules 9, 12, 13)                                         |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
| Phase 4: Frontend Portal Shell & Dashboard (Weeks 7 - 8)    |
| (Modules 10, 11, 15)                                        |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
| Phase 5: Notifications, Share & Admin (Weeks 9 - 10)        |
| (Modules 14, 16, 17)                                        |
+-------------------------------------------------------------+
                               |
                               v
+-------------------------------------------------------------+
| Phase 6: GenAI Microservice & Advisor (Weeks 11 - 12)       |
| (Module 18)                                                 |
+-------------------------------------------------------------+
```

---

## 3. Phase Specifications

### Phase 1: Core Foundation & Identity
*   **Modules Included**: Foundation (M01), Authentication (M02), Authorization (M03), Student Management (M04).
*   **Duration**: Weeks 1 - 2.
*   **Sequence Justification**: Every subsequent domain module requires an authenticated context and database connection mapping. We must establish the JWT family rotation logic and user identity claims pipeline before exposing user profile fields or academic records.
*   **Stage Gate Exit Criteria**:
    *   Database connection pool initializes without errors.
    *   Integration tests successfully run registration, login, and Google token validation.
    *   Security middleware intercepts requests lacking valid Authorization headers.

### Phase 2: Academic Record-Keeping
*   **Modules Included**: Academic Year (M05), Semester (M06), Course (M07), Grade Components (M08).
*   **Duration**: Weeks 3 - 4.
*   **Sequence Justification**: To perform GPA calculations, the database must contain course records. Establishing the chronological heirarchy (Academic Year $\rightarrow$ Semester $\rightarrow$ Course $\rightarrow$ Component Scores) allows for comprehensive validation checks (such as the limit of 3 semesters per year and course retake mappings) before calculating grades.
*   **Stage Gate Exit Criteria**:
    *   Creation of a fourth semester in a single academic year is correctly blocked by validation rules.
    *   Course retakes map correctly to their original course records.
    *   Grade updates log audit records in the database.

### Phase 3: Calculations, Simulation & Goals
*   **Modules Included**: GPA Engine (M09), Goal Planner (M12), Prediction Engine (M13).
*   **Duration**: Weeks 5 - 6.
*   **Sequence Justification**: This phase contains the core mathematical logic. Rounding rules, GPA conversions, and required grade predictions must be fully verified and tested on the backend before building client dashboards and charts.
*   **Stage Gate Exit Criteria**:
    *   The backend test suite verifies that GPA rounding and grade mappings match the BR-CALC rules.
    *   Goal back-calculation logic successfully catches division-by-zero errors when there are no remaining semesters.
    *   Final exam prediction equations return valid targets for all letter grades.

### Phase 4: Frontend Portal Shell & Dashboard
*   **Modules Included**: Settings (M15), Dashboard (M10), Statistics (M11).
*   **Duration**: Weeks 7 - 8.
*   **Sequence Justification**: By this phase, the backend APIs are stabilized. We can now construct the React application container, set up the translation context and dark mode themes, and connect the frontend to the API layer to render dashboard charts.
*   **Stage Gate Exit Criteria**:
    *   Client handles dark/light theme switching and English/Vietnamese translations without visual bugs.
    *   Charts render student GPA history correctly across different screen sizes.
    *   Axios interceptors manage JWT refresh and token rotation seamlessly.

### Phase 5: Notifications, Share & Admin
*   **Modules Included**: Notification (M14), Share Transcript (M16), Admin Console (M17).
*   **Duration**: Weeks 9 - 10.
*   **Sequence Justification**: Security and data access must be finalized before implementing public sharing channels (using UUID v4) and administrative tools. This phase connects the system's operational features with support and sharing capabilities.
*   **Stage Gate Exit Criteria**:
    *   Expired public transcript links return a 404 error page.
    *   Admin locks restrict student logins immediately.
    *   System broadcast alerts appear in the student header banner within 30 seconds of dispatch.

### Phase 6: GenAI Microservice & Advisor
*   **Modules Included**: AI Advisor (M18).
*   **Duration**: Weeks 11 - 12.
*   **Sequence Justification**: The AI Advisor is the most complex component and requires context from the student's entire academic history (GPAs, goals, course paths). Isolating this integration to the final phase allows for clean data mockups during backend development, minimizes early API cost overhead, and protects sensitive user data through finalized anonymization filters.
*   **Stage Gate Exit Criteria**:
    *   The backend anonymizer filters out user PII before sending data to the FastAPI python microservice.
    *   The rate limiter limits requests to 20 messages per hour.
    *   Chat logs are deleted within 24 hours of a student deleting a conversation.

---

*End of Document — Development Order*
