# 14 — MVP Scope

> **Document ID**: SRS-MVP-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Scope Definition and MoSCoW prioritization tables

---

## 1. Document Purpose

This document defines the boundary of the Minimum Viable Product (MVP) for the Academic GPA Management System. It specifies which features are in-scope for the v1.0 release, which are deferred, and the criteria required to declare the MVP ready for launch.

---

## 2. MoSCoW Prioritization Framework

To maximize development efficiency, features are classified into:
*   **Must Have (M)**: Critical core features without which the system cannot function.
*   **Should Have (S)**: Important features that add significant value but can be deferred if timeline constraints require it.
*   **Could Have (C)**: Desirable enhancements that are easy to implement if resources permit.
*   **Won't Have (W)**: Features explicitly excluded from the v1.0 release.

---

## 3. Scope Definition Table

| Module | Feature Description | Category | In MVP? | Detail / Notes |
|:---|:---|:---:|:---:|:---|
| **Auth** | Email / Password Registration | Must Have | Yes | Requires verification link |
| **Auth** | Email / Password Login | Must Have | Yes | Security rate limits applied |
| **Auth** | Google OAuth 2.0 Login | Should Have| Yes | Streamlines login flow |
| **Auth** | Forgot / Reset Password | Must Have | Yes | Token validation via email |
| **Profile** | Student Detail Input | Must Have | Yes | StudentCode, University, Major, required credits |
| **Profile** | UI Preferences (Theme, Language) | Should Have| Yes | Vietnamese/English and Light/Dark mode |
| **Profile** | Custom Avatar Upload | Could Have | No | Deferred; standard fallback initials used |
| **Records** | Academic Year Creation | Must Have | Yes | Grouping by year (e.g. "2024-2025") |
| **Records** | Semester Creation | Must Have | Yes | Max 3 semesters per year validation |
| **Records** | Course Creation & Score Entry | Must Have | Yes | Credits 1-6; Attendance, Continuous, Final |
| **Records** | Course Retake Linking | Should Have| Yes | Only highest attempt counts for GPA |
| **Records** | Course Import via CSV | Could Have | No | Deferred to Phase 2 |
| **Calculations**| Semester & Cumulative GPA | Must Have | Yes | 10-scale and 4-scale calculations |
| **Calculations**| Rounding & Letter Mapping | Must Have | Yes | Nearest 0.5 for components, BR-CALC conversion |
| **Calculations**| Academic Classification | Should Have| Yes | Excellent / Good / Average labels |
| **Statistics** | GPA Trend Line Chart | Must Have | Yes | Visualizing progress across semesters |
| **Statistics** | Grade Distribution Chart | Should Have| Yes | Bar/Pie charts of grade distributions |
| **Statistics** | Credit Completion Tracker | Should Have| Yes | Progress bar toward degree credits |
| **Planner** | Target Cumulative GPA setting | Must Have | Yes | Goal definition |
| **Planner** | Required Semester GPA calculation| Must Have | Yes | Mathematical feasibility check |
| **Planner** | "What-If" Simulation | Should Have| Yes | Simulator to adjust component scores |
| **Prediction** | Inverse Exam Score Calculator | Must Have | Yes | Find required Final exam score for grade |
| **AI Advisor** | Chat panel with LLM Advisor | Should Have| Yes | Rate limited to 20 messages/hour |
| **AI Advisor** | Chat History persistence | Could Have | Yes | Up to 50 active chats saved in database |
| **Transcripts**| Shared Transcript Link | Should Have| Yes | Unguessable UUID v4 public view |
| **Transcripts**| Shared Link Expiry & Revocation | Should Have| Yes | Invalidates link access |
| **Transcripts**| Export to PDF | Could Have | No | Deferred; print-to-PDF styles implemented |
| **Notifs** | In-app notification panel | Should Have| Yes | Alert list and unread badge count |
| **Notifs** | Email alert dispatch | Could Have | No | Deferred; notifications are in-app only |
| **Admin** | Student Accounts Grid | Must Have | Yes | Search, filter, sorting capabilities |
| **Admin** | Lock / Unlock Accounts | Must Have | Yes | Terminate student login access |
| **Admin** | Reset Student Password | Must Have | Yes | Generate temporary credentials |
| **Admin** | Send Announcement Broadcast | Should Have| Yes | Deliver system messages to all students |

---

## 4. MVP Release Criteria

Before the v1.0 application is deployed to production, it must meet the following criteria:

### 4.1 Functional Correctness
- All **Must Have** and **Should Have** features must pass functional testing.
- The **GPA Calculation Engine** must achieve 100% test coverage and pass all edge-case verification tests (see [Business Rules](./04-business-rules.md#13-calculation-verification-test-cases)).

### 4.2 Security Standards
- Password hashing must use BCrypt with salt rounds $\ge 12$.
- Session validation must utilize JWT and Refresh Token rotation (RTR).
- All API endpoints must enforce input validation, parameter binding, and role-based access checks.
- Sensitive environment configurations (database credentials, API keys) must be loaded securely via Environment Variables, never hardcoded.

### 4.3 Performance Standards
- Standard API endpoints must respond in $< 200\text{ms}$ under normal load.
- AI Advisor queries must return responses in $< 5\text{s}$ (utilizing streaming if possible).
- Frontend Initial Page Load time must be $< 3\text{s}$ on a standard 3G/4G cellular connection.

### 4.4 Usability & Accessibility Standards
- Fully responsive interface that works on desktop ($1920\times1080$), tablet ($1024\times768$), and mobile ($375\times812$) screens.
- Screen styling must support both Light and Dark modes.
- UI localization must support English and Vietnamese, with translation text managed in JSON translation files.

---

*End of Document — MVP Scope*
