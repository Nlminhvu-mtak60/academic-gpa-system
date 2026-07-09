# 15 — Version Roadmap

> **Document ID**: SRS-ROAD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Phase release plans, version lists, and milestone tables

---

## 1. Document Purpose

This document details the release timeline and version progression for the Academic GPA Management System. It coordinates features, testing, and deployment milestones across subsequent product versions.

---

## 2. Release Phases & Timelines

The development roadmap is divided into three key delivery phases.

```
Development Lifecycle Timeline
├── Phase 1: Requirements & Core Backend (Weeks 1-4)
│   ├── Environment setup & Database migrations
│   └── Authentication & Core Academic APIs
├── Phase 2: Frontend UI & Calculation Engine (Weeks 5-8)
│   ├── Responsive React Portal
│   └── Rounding, letter grades & Goal calculations
└── Phase 3: AI Advisor, Admin Console & Beta Launch (Weeks 9-12)
    ├── FastAPI integration with LLMs
    ├── Admin student directories
    └── Production launch & security auditing
```

---

## 3. Product Version Index

---

### v1.0.0-Alpha (Weeks 1 - 4): Core APIs
*   **Release Target**: Internal QA / Backend Sandbox.
*   **Key Deliverables**:
    *   ASP.NET Core API server running on dev environments.
    *   Database schema initialized in SQL Server via EF Core migrations.
    *   Auth service complete: User registration, login, token rotation, forgot password flow.
    *   Academic record CRUD endpoints (Years, Semesters, Courses, component scores).
    *   Draft GPA calculator endpoints (not yet rounded/optimized).
    *   Swagger API documentation fully active.

---

### v1.0.0-Beta (Weeks 5 - 8): UI & Calculations Integration
*   **Release Target**: Closed User Group (Selected University Students).
*   **Key Deliverables**:
    *   React SPA client layout complete with Light/Dark mode and English/Vietnamese language switcher.
    *   Integration of core APIs with the React portal.
    *   **GPA Calculation Engine fully verified**; passing all edge-case unit test suites.
    *   Student Dashboard rendering GPA trend charts, grade distribution graphs, and credit progress trackers.
    *   Goal Planner module ("What-If" simulator and required GPA calculator).
    *   Final Exam prediction engine.

---

### v1.0.0-RC (Release Candidate) (Weeks 9 - 10): Admin & AI Integration
*   **Release Target**: Open Beta / Staging Environment.
*   **Key Deliverables**:
    *   Python FastAPI microservice integration with LLMs (Google Gemini / OpenAI GPT).
    *   AI Chat interface in React client with rate limiting controls.
    *   Shared transcripts page (secure UUID-based routing, expiry checkers).
    *   In-app notification system (polling queues, unread badges).
    *   Admin console dashboard: student search grid, account locks, temporary passwords, broadcast notification panel.

---

### v1.0.0-Stable (Weeks 11 - 12): Launch
*   **Release Target**: General Availability (Production Launch).
*   **Key Deliverables**:
    *   Deployment to Production Cloud (Dockerized backend and frontend on AWS/Azure).
    *   Security hardening: full penetration testing, verification of BCrypt salt rounds, TLS-only enforcement.
    *   Verification of zero critical errors in calculations.
    *   User manuals and API documentation published.

---

### v1.1.0 (Post-MVP): Operations & Polish
*   **Release Target**: Q3 2026.
*   **Planned Features**:
    *   CSV/Excel bulk import of course configurations and scores.
    *   Exporting transcripts as formatted PDFs.
    *   In-app feedback collection tool for users to report advisor bugs or calculation questions.

---

### v2.0.0 (Post-MVP): Platform Integrations
*   **Release Target**: Q1 2027.
*   **Planned Features**:
    *   Canvas and Moodle LMS integrations using LTI 1.3 standards.
    *   React Native Mobile Applications for iOS and Android.
    *   AI Career mapping recommendations based on transcripts.
    *   Peer-to-peer tutoring matching.

---

## 4. Milestone Matrix

| Milestone ID | Description | Phase Target | Verification Method |
|---|---|---|---|
| **MS-01** | Database & Auth Ready | Week 2 | Integration tests check register/login endpoints |
| **MS-02** | Acad APIs Complete | Week 4 | Swagger testing of course/score CRUD operations |
| **MS-03** | Frontend Shell Running | Week 6 | UI review in Chrome/Safari (Responsiveness & Theme switch) |
| **MS-04** | GPA Calculator Certified| Week 7 | 100% success on all 6 core calculation test scenarios |
| **MS-05** | LLM Advisor Active | Week 9 | Chat panel queries return valid anonymized advisor responses |
| **MS-06** | Admin Dashboard Complete | Week 10 | Verify account lock enforcement and broadcast delivery |
| **MS-07** | Production Deployment | Week 11 | Docker health checks return success status |
| **MS-08** | Project Sign-off | Week 12 | Zero open P0 bugs, stakeholder acceptance |

---

*End of Document — Version Roadmap*
