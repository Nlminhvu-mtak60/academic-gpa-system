# 13 — Future Expansion

> **Document ID**: SRS-FUT-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Roadmap matrix and expansion design briefs

---

## 1. Document Purpose

This document outlines the strategic roadmap and future enhancements for the Academic GPA Management System post-MVP (Version 1.0). Defining these integration hooks and modular extensions early ensures that the initial software architecture is built to support scale and clean integrations.

---

## 2. Future Expansion Roadmap

The long-term vision spans four core categories: **Integrations**, **Platform Reach**, **AI Advisory Scaling**, and **Social Collaboration**.

```
Post-MVP Feature Roadmap
├── Phase 2: System Integrations & Mobile (v1.5)
│   ├── LMS Integrations (Canvas, Moodle API)
│   ├── Mobile Apps (React Native wrappers)
│   └── Multi-University Grading Scale Engine
├── Phase 3: Advanced AI & Career Planning (v2.0)
│   ├── Predictive Career Pathway Mapper
│   ├── Interactive Study Guide Generator
│   └── Automated LLM Tutoring Modules
└── Phase 4: Peer Collaboration & Mentorship (v2.5)
    ├── Peer-to-Peer Study Matchmaker
    ├── Shared Academic Study Rooms
    └── Anonymous Benchmark Analytics
```

---

## 3. Detailed Expansion Briefs

---

### 3.1 Learning Management System (LMS) Integrations (Phase 2)
Manually typing grades can be tedious. Integrating directly with university portals improves accuracy and user engagement.

*   **Integration Mechanisms**:
    *   **LTI (Learning Tools Interoperability)**: Compliance with LTI 1.3 standards to run inside Canvas or Moodle.
    *   **OAuth Scraped Connectors**: Direct API sync tools for Canvas and Moodle REST APIs, allowing users to enter their LMS credentials to pull course lists, credits, and component scores.
*   **Architectural Hook**: The `AcadRecordModule` is designed to receive external imports. We will expose an `IExternalImportService` interface to implement various LMS adapters.

---

### 3.2 Mobile Platform Expansion (Phase 2)
Although the web app is fully responsive, native notifications and offline access are highly desirable for students.

*   **Approach**:
    *   Re-use the React-TypeScript business logic layer inside a **React Native** application, share API calls and state management.
    *   Utilize native OS notifications (APNS/FCM) instead of background API polling for real-time notifications.
    *   **Offline Cache**: Use SQLite or WatermelonDB to store transcripts on-device, enabling GPA viewing without an active internet connection.

---

### 3.3 Multi-University Custom Grading Scales (Phase 2)
The MVP assumes a standard Vietnamese grading system (10-point scale mapped to A+/F and GPA-4). However, other universities use alternative systems (e.g., 100-point scales, 5.0 GPA scales, ECTS credits).

*   **Execution**:
    *   Transform `GradeResult` value object from a static class to a database-driven entity `GradingScaleConfig`.
    *   Students select their university configuration during onboarding.
    *   GPA Recalculation Engine loads the selected `GradingScaleConfig` dynamically, updating formula coefficients and letter mapping thresholds.

---

### 3.4 AI Career & Course Path Recommendations (Phase 3)
Expanding the AI Advisor from a reactive Q&A bot to a proactive career coach.

*   **Features**:
    *   **Career Mapping**: Analyze transcript trends (e.g., strong performance in Software Engineering, weaker in Database Systems) and recommend matching industry roles.
    *   **Course Recommendation**: Suggest elective courses that align with the student's career aspirations and historical performance.
    *   **Lacking Skill Analyzer**: Identify specific components or subjects where the student underperforms and recommend targeted micro-credentials or study materials.

---

### 3.5 Peer-to-Peer Academic Mentorship (Phase 4)
Adding a social component where high-performing students can assist struggling peers.

*   **Features**:
    *   **Opt-in Tutors**: Students with an A+ or A in a course can mark themselves as peer tutors.
    *   **Mentorship Matchmaker**: Students struggling with a course (D+ or below, or low continuous scores) are suggested matching tutors within their major.
    *   **Study Leaderboards**: Gamified leaderboards to encourage high GPA achievement, using anonymous usernames to protect privacy.
    *   **Comparative Analytics**: Allow students to see how they rank compared to their cohort average in similar courses (e.g., "Your Math 101 score is in the top 15% of your major").

---

## 4. Design Guidelines for Extensibility

To ensure these features can be added without significant rewrites of the core application, the MVP development must adhere to the following clean coding guidelines:

1.  **Strict Interface Separation**: Never couple components directly to external services. Always inject interfaces (e.g., `IEmailService`, `IAiService`) so that services can be swapped (e.g., moving from local SMTP to SendGrid or changing LLM providers).
2.  **Modular Database Schemas**: Keep tables normalized. Use reference tables for scales and configs rather than hardcoding logic in SQL queries.
3.  **Clean Separation of Presentation and Logic**: Keep React components dry. Exclude calculations or business logic from JSX templates. Business rules must reside in dedicated utilities (`/utils/gradeConverter.ts`) and hooks, which can easily be ported to React Native.

---

*End of Document — Future Expansion*
