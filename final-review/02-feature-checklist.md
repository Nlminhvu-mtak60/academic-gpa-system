# Feature Checklist — Implementation Progress

This document tracks the status of all specifications for the Academic GPA Management System. All modules have been fully implemented, tested, and approved.

---

## 1. Authentication & Session Management
- [x] Student registration with unique emails and password validation.
- [x] Secure login generating JWT access tokens (15-min expiration) and refresh tokens (7-day persistence).
- [x] Automatic session renewal via transparent refresh token endpoint.
- [x] Secure password hashing using PBKDF2 with unique salts.
- [x] Account locking mechanisms for security violations.

## 2. Academic & Grade Management
- [x] Multi-semester and academic year creation, editing, and soft deletion.
- [x] Course entry with credits, attendance, continuous, and final scores.
- [x] Automated grade calculations mapping 10-point grades to 4-point scales and letter grades (A, B+, B, C+, C, D+, D, F).
- [x] Handling of retake attempts (only higher grade counts in cumulative GPA calculations).

## 3. Goal Planner & Final Exam Prediction
- [x] Target GPA simulations based on remaining credits.
- [x] Feasibility checks indicating whether target GPAs are achievable.
- [x] Automatic final exam score predictions to reach target grades (e.g., Course Score 8.0, 9.0, or custom thresholds).

## 4. Notifications & Settings
- [x] System, academic, GPA milestones, and prediction alert notifications.
- [x] Notification read/unread status updates.
- [x] Settings dashboard with profile updates, theme selections (Light/Dark), and language choice (Vietnamese/English).

## 5. Admin Control Panel
- [x] View all system accounts with paginated search.
- [x] Lock/unlock student accounts.
- [x] Reset student credentials and edit student profile details.
- [x] General system statistics dashboard (active students, average GPA, total courses, system usage).

## 6. AI Academic Advisor
- [x] Interactive chat interface for academic advice.
- [x] Multi-perspective analyses (GPA trends, credit completion, strength/weakness vectors).
- [x] Tailored study recommendations (e.g. subjects needing attention, course loading suggestions).
- [x] Contextual memory within a thread.
