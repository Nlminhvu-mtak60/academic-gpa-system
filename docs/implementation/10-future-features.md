# 10 — Future Features

> **Document ID**: PLN-FUT-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Post-MVP roadmap detailing future versions, integrations, and architectural enhancements

---

## 1. Document Purpose

This document outlines the product roadmap and future features for the Academic GPA Management System beyond the v1.0 Minimum Viable Product (MVP) release. It provides a blueprint for subsequent enhancements, integrations, and architectural scaling tasks.

---

## 2. Post-MVP Feature Release Phases

```
+--------------------------------------------------------------------------------+
| VERSION 1.1.0: Usability & Export Options (Q3 2026)                            |
| - CSV/Excel Course Import & PDF Transcript Export                              |
+--------------------------------------------------------------------------------+
                                       |
                                       v
+--------------------------------------------------------------------------------+
| VERSION 1.2.0: Peer Connections & Study Tools (Q4 2026)                        |
| - Cohort Comparisons, Peer Tutoring & Group Planners                           |
+--------------------------------------------------------------------------------+
                                       |
                                       v
+--------------------------------------------------------------------------------+
| VERSION 2.0.0: Enterprise Platforms & Native Mobile (Q1 2027)                  |
| - LMS Integrations (Canvas/Moodle LTI), React Native Apps & AI Career Maps     |
+--------------------------------------------------------------------------------+
```

---

## 3. Future Feature Specifications

### 3.1 Version 1.1.0: Usability & Export Options
Focuses on user convenience, data entry efficiency, and document exporting capabilities.
*   **CSV/Excel Course Import**: Enable students to bulk-upload their academic history using templates. The backend will parse uploaded files, run validation checks on credit values and grading structures, and save valid records to the database.
*   **PDF Transcript Export**: Provide high-fidelity PDF exports of student transcripts. The system will use server-side rendering tools (e.g., QuestPDF or Puppeteer) to output PDFs that match official university transcripts and support localized Vietnamese/English templates.
*   **AI Advisor Feedback Panel**: Add user feedback controls (like thumb up/down buttons and text boxes) on AI messages. This will help collect training data to improve the FastAPI prompt templates and advisor models.

### 3.2 Version 1.2.0: Peer Connections & Study Tools
Focuses on peer cooperation and study planning features.
*   **Cohort Comparisons**: Allow students to opt-in to view anonymized GPA rankings and percentile standings within their major, department, or university year.
*   **Group Study Goals**: Enable students to form study groups, define group target GPAs, and track progress together.
*   **Peer Tutoring Matching**: Connect high-performing students with peers who need help in specific courses, using transcript histories to match tutors with students.

### 3.3 Version 2.0.0: Enterprise Platforms & Native Mobile
Focuses on platform expansion, native app development, and integration with academic systems.
*   **LMS Canvas / Moodle Integrations**: Integrate with university systems using LTI 1.3 standards. This allows students to sync their coursework and grades directly from Canvas, Blackboard, or Moodle, reducing the need for manual score entry.
*   **React Native Mobile Applications**: Deploy native applications for iOS and Android devices, sharing state structures, API layers, and translation files with the core React SPA web application.
*   **AI Career Mapping**: Expand the AI Advisor to map a student's transcript and GPA history to actual job descriptions and university career requirements, recommending specific courses to take.

---

## 4. Technical Debt & Scaling Initiatives

To support a growing user base, the engineering team has scheduled three technical enhancements for post-MVP execution:

1.  **Migrate Polling to WebSockets (SignalR)**  
    *   *MVP setup*: The frontend uses HTTP polling at 30-second intervals to check for notifications.
    *   *Enhancement*: Implement ASP.NET Core SignalR to replace polling with WebSockets. This will reduce unnecessary server load and support instant, real-time message delivery.
2.  **Distributed Caching (Redis)**  
    *   *MVP setup*: The application uses basic in-memory caching.
    *   *Enhancement*: Set up a Redis distributed cache cluster to cache database lookups (like translation keys, system configurations, and statistics calculations) across multiple server instances.
3.  **Database Partitioning**  
    *   *MVP setup*: Student profiles and grade tables reside in standard relational schemas.
    *   *Enhancement*: Configure horizontal partitioning on the `Scores` and `ScoreAuditLog` tables based on academic year keys to maintain query performance as the database grows.

---

*End of Document — Future Features*
