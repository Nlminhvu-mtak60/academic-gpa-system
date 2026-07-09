# End-to-End (E2E) Manual Test Report (Phase 10)

This document maps out the manual testing workflows, test cases, and validation results across the student portal and administrative panel.

---

## 1. Student Portal Flow Verification

### Authentication & Registration
- **Test Case E2E-ST-01**: Register new student account.
  - *Steps*: Fill valid details on `/register`, verify success message, redirect to `/login`.
  - *Result*: **Pass** (correct default settings initialized).
- **Test Case E2E-ST-02**: Profile completion.
  - *Steps*: Go to settings, update student code, university name, and graduation credits.
  - *Result*: **Pass** (validates alphanumeric constraints).

### Academic Year & Grade Management
- **Test Case E2E-ST-03**: Create academic year & semester.
  - *Steps*: Create academic year `2024-2025`, add `Semester 1`.
  - *Result*: **Pass**.
- **Test Case E2E-ST-04**: Course creation & grade inputs.
  - *Steps*: Add Course `CS101` (3 credits), set scores (9.0, 8.5, 9.0).
  - *Result*: **Pass** (Calculates GPA 10 = 8.9, GPA 4 = 3.7, Letter Grade = A, Pass = True).
- **Test Case E2E-ST-05**: Retake GPA Calculation.
  - *Steps*: Add another attempt of `CS101` with score (10.0, 10.0, 10.0), verify original attempt is excluded from cumulative GPA.
  - *Result*: **Pass** (GPA correctly recalculates with second attempt).

### Goal Planner & What-If Simulator
- **Test Case E2E-ST-06**: Set GPA target goal.
  - *Steps*: Set target GPA to 8.5. Verify required GPA remaining calculation update.
  - *Result*: **Pass** (Updates feasibility message dynamically).
- **Test Case E2E-ST-07**: Simulate scores.
  - *Steps*: Input simulation scores on dashboard course list, verify target variance update without database updates.
  - *Result*: **Pass**.

---

## 2. Admin Panel Flow Verification

### Student & Account Management
- **Test Case E2E-AD-01**: Search and filter students.
  - *Steps*: Search student list by student code or email. Filter by status.
  - *Result*: **Pass**.
- **Test Case E2E-AD-02**: Lock and unlock student accounts.
  - *Steps*: Lock a student account, verify login attempts are blocked with 403 Forbidden. Unlock account, verify login succeeds.
  - *Result*: **Pass**.

### Notification Management
- **Test Case E2E-AD-03**: Broadcast Notification.
  - *Steps*: Write a system broadcast message, verify all logged-in students receive the banner immediately.
  - *Result*: **Pass**.
