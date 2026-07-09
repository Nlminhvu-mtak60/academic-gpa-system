# 05 — User Stories

> **Document ID**: SRS-US-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: "As a [role], I want [action], so that [benefit]"

---

## 1. Story Point Scale

| Points | Effort Level | Typical Duration |
|--------|-------------|------------------|
| 1 | Trivial | < 2 hours |
| 2 | Simple | 2–4 hours |
| 3 | Small | 0.5–1 day |
| 5 | Medium | 1–2 days |
| 8 | Large | 2–4 days |
| 13 | Extra Large | 4–7 days |
| 21 | Epic-sized (break down further) | 1–2 weeks |

---

## 2. Epic 1: Authentication & Account Management

### US-AUTH-01: Student Registration

| Attribute | Value |
|-----------|-------|
| **Story** | As a **new student**, I want to **register an account with my email and password**, so that **I can start tracking my academic performance**. |
| **Priority** | P0 |
| **Story Points** | 8 |
| **Acceptance Criteria** | See AC-AUTH-01 in [06-acceptance-criteria.md](./06-acceptance-criteria.md) |
| **Dependencies** | None |

### US-AUTH-02: Email Verification

| Attribute | Value |
|-----------|-------|
| **Story** | As a **newly registered student**, I want to **verify my email address via a confirmation link**, so that **my account is activated and secured**. |
| **Priority** | P0 |
| **Story Points** | 5 |
| **Dependencies** | US-AUTH-01 |

### US-AUTH-03: Student Login

| Attribute | Value |
|-----------|-------|
| **Story** | As a **registered student**, I want to **log in with my email and password**, so that **I can access my academic data**. |
| **Priority** | P0 |
| **Story Points** | 5 |
| **Dependencies** | US-AUTH-01, US-AUTH-02 |

### US-AUTH-04: Google Login

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **log in using my Google account**, so that **I can access the system quickly without remembering another password**. |
| **Priority** | P1 |
| **Story Points** | 8 |
| **Dependencies** | US-AUTH-03 |

### US-AUTH-05: Forgot Password Request

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student who forgot my password**, I want to **request a password reset link via email**, so that **I can regain access to my account**. |
| **Priority** | P0 |
| **Story Points** | 5 |
| **Dependencies** | US-AUTH-01 |

### US-AUTH-06: Reset Password

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student with a reset link**, I want to **set a new password**, so that **I can log in again**. |
| **Priority** | P0 |
| **Story Points** | 3 |
| **Dependencies** | US-AUTH-05 |

### US-AUTH-07: Token Refresh

| Attribute | Value |
|-----------|-------|
| **Story** | As a **logged-in student**, I want the **system to automatically refresh my session**, so that **I don't have to re-login every 15 minutes**. |
| **Priority** | P0 |
| **Story Points** | 5 |
| **Dependencies** | US-AUTH-03 |

### US-AUTH-08: Logout

| Attribute | Value |
|-----------|-------|
| **Story** | As a **logged-in student**, I want to **log out**, so that **my session is securely ended**. |
| **Priority** | P0 |
| **Story Points** | 2 |
| **Dependencies** | US-AUTH-03 |

### US-AUTH-09: Change Password

| Attribute | Value |
|-----------|-------|
| **Story** | As a **logged-in student**, I want to **change my password**, so that **I can maintain my account security**. |
| **Priority** | P0 |
| **Story Points** | 3 |
| **Dependencies** | US-AUTH-03 |

---

## 3. Epic 2: Profile Management

### US-PROFILE-01: View Profile

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **view my profile information**, so that **I can see my registered details**. |
| **Priority** | P0 |
| **Story Points** | 2 |

### US-PROFILE-02: Edit Profile

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **edit my profile (name, student code, university, major, enrollment year)**, so that **my information is accurate and up to date**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-PROFILE-03: Upload Avatar

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **upload a profile picture**, so that **my profile feels personalized**. |
| **Priority** | P2 |
| **Story Points** | 5 |

### US-PROFILE-04: Update Preferences

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **set my preferred language and theme**, so that **the application feels comfortable to use**. |
| **Priority** | P1 |
| **Story Points** | 3 |

---

## 4. Epic 3: Academic Structure Management

### US-ACAD-01: Create Academic Year

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **create an academic year**, so that **I can organize my semesters within it**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-ACAD-02: View Academic Years

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see a list of all my academic years with summary GPAs**, so that **I can get an overview of my academic history**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-ACAD-03: Edit Academic Year

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **edit an academic year's name and dates**, so that **I can correct mistakes**. |
| **Priority** | P0 |
| **Story Points** | 2 |

### US-ACAD-04: Delete Academic Year

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **delete an academic year and all its semesters/courses**, so that **I can remove incorrect data**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-ACAD-05: Create Semester

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **create a semester within an academic year**, so that **I can add courses to it**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-ACAD-06: View Semesters

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see all semesters in an academic year with their GPA summaries**, so that **I can track performance per semester**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-ACAD-07: Edit Semester

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **edit a semester's name**, so that **I can correct it if needed**. |
| **Priority** | P0 |
| **Story Points** | 2 |

### US-ACAD-08: Delete Semester

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **delete a semester and all courses within it**, so that **I can remove incorrect data**. |
| **Priority** | P0 |
| **Story Points** | 3 |

---

## 5. Epic 4: Course Management

### US-COURSE-01: Add Course

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **add a course to a semester with its code, name, and credits**, so that **I can track its scores and GPA contribution**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-COURSE-02: View Courses

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see all courses in a semester with their scores and grades**, so that **I can review my academic performance**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-COURSE-03: Edit Course

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **edit a course's details**, so that **I can fix errors in the course information**. |
| **Priority** | P0 |
| **Story Points** | 2 |

### US-COURSE-04: Delete Course

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **delete a course**, so that **I can remove it from my semester**. |
| **Priority** | P0 |
| **Story Points** | 2 |

### US-COURSE-05: Input Component Scores

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **input my attendance, continuous assessment, and final exam scores**, so that **the system calculates my course score and grade automatically**. |
| **Priority** | P0 |
| **Story Points** | 8 |
| **Notes** | This is the core feature. Must implement rounding rules and grade conversion accurately. |

### US-COURSE-06: View Score Audit History

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see the history of changes to my scores**, so that **I can track what I modified and when**. |
| **Priority** | P2 |
| **Story Points** | 3 |

---

## 6. Epic 5: GPA Calculation

### US-GPA-01: View Semester GPA (10-Scale)

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see my semester GPA on the 10-point scale**, so that **I know my semester performance**. |
| **Priority** | P0 |
| **Story Points** | 5 |

### US-GPA-02: View Semester GPA (4-Scale)

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see my semester GPA on the 4-point scale**, so that **I can compare with international standards**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-GPA-03: View Academic Year GPA

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see my GPA for an entire academic year**, so that **I can assess year-over-year performance**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-GPA-04: View Cumulative GPA

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see my cumulative GPA across all semesters**, so that **I know my overall academic standing**. |
| **Priority** | P0 |
| **Story Points** | 5 |

### US-GPA-05: View Academic Classification

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see my academic classification (Excellent, Good, Average, etc.)**, so that **I understand my standing according to Vietnamese university standards**. |
| **Priority** | P1 |
| **Story Points** | 2 |

---

## 7. Epic 6: Statistics & Analytics

### US-STATS-01: GPA Trend Chart

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see a line chart showing my GPA trend across semesters**, so that **I can visualize my academic trajectory**. |
| **Priority** | P0 |
| **Story Points** | 5 |

### US-STATS-02: Grade Distribution

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see a chart showing the distribution of my letter grades**, so that **I understand my grade profile**. |
| **Priority** | P1 |
| **Story Points** | 3 |

### US-STATS-03: Credit Progress

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see how many credits I've completed out of my total required**, so that **I can track my progress toward graduation**. |
| **Priority** | P1 |
| **Story Points** | 3 |

### US-STATS-04: Semester Comparison

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **compare the performance of two or more semesters side by side**, so that **I can identify improvement or decline**. |
| **Priority** | P1 |
| **Story Points** | 5 |

### US-STATS-05: Strongest/Weakest Subjects

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see my highest and lowest scoring courses**, so that **I know my academic strengths and weaknesses**. |
| **Priority** | P2 |
| **Story Points** | 3 |

---

## 8. Epic 7: Goal Planner

### US-GOAL-01: Set GPA Goal

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **set a target cumulative GPA**, so that **I have a clear academic goal to work toward**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-GOAL-02: Calculate Required GPA

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **know what semester GPA I need to achieve my target cumulative GPA**, so that **I can plan my study effort**. |
| **Priority** | P0 |
| **Story Points** | 5 |

### US-GOAL-03: What-If Simulation

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **simulate hypothetical course scores and see the impact on my GPA**, so that **I can make informed decisions about my study priorities**. |
| **Priority** | P1 |
| **Story Points** | 8 |

### US-GOAL-04: Track Goal Progress

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see my progress toward my GPA goal with a visual indicator**, so that **I stay motivated**. |
| **Priority** | P1 |
| **Story Points** | 3 |

---

## 9. Epic 8: Final Exam Prediction

### US-PREDICT-01: Predict Final Exam Score

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **know the minimum final exam score I need to achieve a specific grade**, so that **I can set realistic exam targets**. |
| **Priority** | P0 |
| **Story Points** | 5 |

### US-PREDICT-02: Multi-Scenario View

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see the required final exam score for ALL possible letter grades at once**, so that **I can understand my full range of outcomes**. |
| **Priority** | P1 |
| **Story Points** | 3 |

---

## 10. Epic 9: AI Academic Advisor

### US-AI-01: Start AI Chat

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **start a conversation with the AI academic advisor**, so that **I can get personalized academic guidance**. |
| **Priority** | P1 |
| **Story Points** | 8 |

### US-AI-02: Send Message and Get Response

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **send a message to the AI advisor and receive an intelligent response based on my academic data**, so that **I get relevant, actionable advice**. |
| **Priority** | P1 |
| **Story Points** | 13 |

### US-AI-03: View Past Conversations

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see a list of my past AI conversations**, so that **I can revisit previous advice**. |
| **Priority** | P2 |
| **Story Points** | 3 |

### US-AI-04: Read Conversation History

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **read the full message history of a past conversation**, so that **I can recall the advice given**. |
| **Priority** | P2 |
| **Story Points** | 2 |

### US-AI-05: Delete Conversation

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **delete an AI conversation**, so that **I can keep my conversation list clean**. |
| **Priority** | P2 |
| **Story Points** | 1 |

---

## 11. Epic 10: Notifications

### US-NOTIF-01: Receive Notifications

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **receive notifications from administrators and the system**, so that **I stay informed about important updates**. |
| **Priority** | P1 |
| **Story Points** | 5 |

### US-NOTIF-02: View Notification List

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see all my notifications in a list**, so that **I can review messages I've received**. |
| **Priority** | P1 |
| **Story Points** | 3 |

### US-NOTIF-03: Mark Notifications as Read

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **mark notifications as read individually or all at once**, so that **I can manage my notification inbox**. |
| **Priority** | P1 |
| **Story Points** | 2 |

### US-NOTIF-04: See Unread Count

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see the number of unread notifications as a badge**, so that **I know when there are new messages without opening the notification page**. |
| **Priority** | P1 |
| **Story Points** | 2 |

---

## 12. Epic 11: Transcript Sharing

### US-TRANSCRIPT-01: View My Transcript

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **view a formatted academic transcript of all my courses and GPAs**, so that **I can review my complete academic record**. |
| **Priority** | P1 |
| **Story Points** | 5 |

### US-TRANSCRIPT-02: Generate Share Link

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **generate a shareable link to my transcript**, so that **I can share it with employers or other institutions**. |
| **Priority** | P1 |
| **Story Points** | 5 |

### US-TRANSCRIPT-03: Public Transcript View

| Attribute | Value |
|-----------|-------|
| **Story** | As a **visitor with a share link**, I want to **view a student's transcript without logging in**, so that **I can review their academic performance**. |
| **Priority** | P1 |
| **Story Points** | 3 |

### US-TRANSCRIPT-04: Revoke Share Link

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **revoke a previously shared link**, so that **I can control who has access to my transcript**. |
| **Priority** | P1 |
| **Story Points** | 2 |

### US-TRANSCRIPT-05: Manage Share Links

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **see all my active share links with their view counts and expiry dates**, so that **I can manage access to my transcript**. |
| **Priority** | P1 |
| **Story Points** | 3 |

---

## 13. Epic 12: UI Preferences

### US-PREF-01: Toggle Theme

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **switch between dark and light mode**, so that **I can use the application comfortably in different lighting conditions**. |
| **Priority** | P1 |
| **Story Points** | 5 |

### US-PREF-02: Switch Language

| Attribute | Value |
|-----------|-------|
| **Story** | As a **student**, I want to **switch the interface language between Vietnamese and English**, so that **I can use the application in my preferred language**. |
| **Priority** | P1 |
| **Story Points** | 8 |

---

## 14. Epic 13: Admin — Student Management

### US-ADMIN-01: Admin Login

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **log in to the admin panel**, so that **I can manage the system**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-ADMIN-02: View Student List

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **see a searchable, filterable list of all registered students**, so that **I can find and manage student accounts**. |
| **Priority** | P0 |
| **Story Points** | 5 |

### US-ADMIN-03: View Student Detail

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **view a student's profile and complete academic records (read-only)**, so that **I can review their performance**. |
| **Priority** | P0 |
| **Story Points** | 5 |

### US-ADMIN-04: Lock Student Account

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **lock a student's account with a reason**, so that **I can prevent access for policy violations**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-ADMIN-05: Unlock Student Account

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **unlock a previously locked student account**, so that **the student can regain access**. |
| **Priority** | P0 |
| **Story Points** | 2 |

### US-ADMIN-06: Reset Student Password

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **reset a student's password to a temporary password**, so that **students who cannot use the forgot password flow can still access their accounts**. |
| **Priority** | P0 |
| **Story Points** | 3 |

### US-ADMIN-07: Delete Student Account

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **soft-delete a student account**, so that **it is no longer accessible but data is preserved for auditing**. |
| **Priority** | P1 |
| **Story Points** | 3 |

---

## 15. Epic 14: Admin — Statistics & Notifications

### US-ADMIN-08: Admin Dashboard Statistics

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **see aggregate statistics on the dashboard (total students, active/locked, GPA distribution)**, so that **I can monitor the overall health of the platform**. |
| **Priority** | P1 |
| **Story Points** | 8 |

### US-ADMIN-09: Send Notification to Student

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **send a notification to a specific student**, so that **I can communicate important information to them**. |
| **Priority** | P1 |
| **Story Points** | 3 |

### US-ADMIN-10: Broadcast Notification

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **send a notification to all active students**, so that **I can make announcements efficiently**. |
| **Priority** | P1 |
| **Story Points** | 5 |

### US-ADMIN-11: View Sent Notifications

| Attribute | Value |
|-----------|-------|
| **Story** | As an **admin**, I want to **see a history of all notifications I've sent**, so that **I can track my communications**. |
| **Priority** | P1 |
| **Story Points** | 3 |

---

## 16. Story Points Summary

| Epic | Stories | Total Points |
|------|---------|-------------|
| Authentication | 9 | 44 |
| Profile | 4 | 13 |
| Academic Structure | 8 | 22 |
| Course Management | 6 | 21 |
| GPA Calculation | 5 | 18 |
| Statistics | 5 | 19 |
| Goal Planner | 4 | 19 |
| Exam Prediction | 2 | 8 |
| AI Advisor | 5 | 27 |
| Notifications | 4 | 12 |
| Transcript Sharing | 5 | 18 |
| UI Preferences | 2 | 13 |
| Admin — Students | 7 | 24 |
| Admin — Stats & Notifications | 4 | 19 |
| **TOTAL** | **70** | **277** |

---

*End of Document — User Stories*
