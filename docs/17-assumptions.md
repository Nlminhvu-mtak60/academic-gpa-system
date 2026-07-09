# 17 — Assumptions

> **Document ID**: SRS-ASS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Assumptions classification registry

---

## 1. Document Purpose

This document details the core planning assumptions made during the requirements analysis and architectural design of the Academic GPA Management System. If any of these assumptions are violated, the project timeline, cost, or technical feasibility may be impacted.

---

## 2. Categorized Assumptions

---

### 2.1 User Environment Assumptions
1.  **Modern Browser Standard**: We assume that users (both Students and Admins) access the system using modern browsers (Chrome, Edge, Safari, Firefox) released within the last 24 months. We assume no IE11 or legacy legacy browser compatibility is required.
2.  **Internet Connectivity**: We assume that the application is accessed with an active internet connection. Offline editing capabilities are out of scope for the MVP.
3.  **Basic Computer Literacy**: We assume students are familiar with entering values into web spreadsheets/input fields and downloading standard PDFs.

---

### 2.2 Integration & External Service Assumptions
1.  **Google OAuth 2.0 Stability**: We assume Google's Identity Provider services maintain high availability (99.9%+ uptime) and that their OAuth signature structures do not undergo breaking modifications during the development lifecycle.
2.  **LLM Provider Availability & Rate Limits**: We assume the chosen LLM API provider (Google Gemini or OpenAI) remains accessible and that the development account is provisioned with sufficient rate limits (Request-Per-Minute and Token-Per-Minute) to support active student testing.
3.  **SMTP Delivery Success**: We assume that the SMTP server configured for registration verification and password reset emails has a warmed reputation and will not route verification emails to spam folders.

---

### 2.3 Business & Academic Rules Assumptions
1.  **Standard Weight Distribution**: We assume the grading formula `CourseScore = Attendance * 0.1 + Continuous * 0.3 + Final * 0.6` applies broadly to all courses. If a specific course uses a different weight structure (e.g. 20% Attendance, 80% Final), the user must normalize their scores to fit this scale, or wait for the Phase 2 custom grading scale expansion.
2.  **Credit Integer Constraint**: We assume course credits are always positive integers (1 to 6). Decimal credits (e.g., 1.5 credits) are assumed not to exist in the target student population.
3.  **Single Active Goal**: We assume a student only needs to track one target cumulative GPA goal at a time. Multi-goal tracking (e.g., separating scholarship goals from graduation goals) is not supported in the MVP.

---

### 2.4 Project Resource Assumptions
1.  **FastAPI AI Isolation**: We assume the LLM prompting and analytics logic is hosted in a separate FastAPI container to keep the ASP.NET Core API clean and isolate Python dependency requirements.
2.  **Hosting Resources**: We assume that the target cloud environment supports Docker Compose or containerized orchestrators (e.g., Azure Container Apps, AWS ECS) for hosting the web API, client app, and FastAPI microservice.

---

*End of Document — Assumptions*
