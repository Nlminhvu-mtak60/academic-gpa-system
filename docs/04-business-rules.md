# 04 — Business Rules

> **Document ID**: SRS-BR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review

---

## 1. Document Purpose

This document defines all business rules governing the Academic GPA Management System. Business rules are the domain-specific logic that dictates how data is processed, validated, and calculated. These rules are the source of truth for implementation.

---

## 2. Business Rule Categories

| Category | Prefix | Count |
|----------|--------|-------|
| Authentication Rules | BR-AUTH | 12 |
| Academic Structure Rules | BR-ACAD | 5 |
| Course Rules | BR-COURSE | 4 |
| Score Calculation Rules | BR-CALC | 14 |
| Goal Planning Rules | BR-GOAL | 3 |
| Prediction Rules | BR-PREDICT | 2 |
| AI Advisor Rules | BR-AI | 4 |
| Notification Rules | BR-NOTIF | 3 |
| Transcript Rules | BR-TRANS | 3 |
| Admin Rules | BR-ADMIN | 4 |

---

## 3. Authentication Rules (BR-AUTH)

### BR-AUTH-001: Email Uniqueness

| Attribute | Value |
|-----------|-------|
| **Rule** | Each email address can only be associated with one account in the system. |
| **Enforcement** | Database unique constraint + application validation |
| **Error** | "An account with this email already exists." |

### BR-AUTH-002: Password Complexity

| Attribute | Value |
|-----------|-------|
| **Rule** | Passwords must meet all of the following criteria: minimum 8 characters, at least 1 uppercase letter (A–Z), at least 1 lowercase letter (a–z), at least 1 digit (0–9), at least 1 special character (!@#$%^&*()-_+=). |
| **Enforcement** | Server-side validation + client-side real-time feedback |
| **Error** | Specific message indicating which criteria is not met |

### BR-AUTH-003: Name Validation

| Attribute | Value |
|-----------|-------|
| **Rule** | First name and last name must be 1–50 characters. May contain Unicode characters (Vietnamese diacritics). Must not contain digits or special characters. |
| **Regex** | `^[\p{L}\s'-]{1,50}$` |

### BR-AUTH-004: Email Verification Required

| Attribute | Value |
|-----------|-------|
| **Rule** | A user account cannot log in until their email address is verified. Verification tokens expire after 24 hours. |
| **Enforcement** | Login check: if `IsEmailVerified == false`, return error "Please verify your email before logging in." |

### BR-AUTH-005: Locked Account Prevention

| Attribute | Value |
|-----------|-------|
| **Rule** | A locked account (IsActive == false) cannot log in, regardless of correct credentials. |
| **Error** | "Your account has been locked. Please contact the administrator." |

### BR-AUTH-006: Login Response Security

| Attribute | Value |
|-----------|-------|
| **Rule** | Login error messages must not reveal whether the email exists in the system. |
| **Error** | Always: "Invalid email or password." (never "Email not found" or "Wrong password") |

### BR-AUTH-007: Session Token Policy

| Attribute | Value |
|-----------|-------|
| **Rule** | Access tokens expire after 15 minutes. Refresh tokens expire after 7 days. A refresh token can only be used once (rotation). |

### BR-AUTH-008: Google OAuth Auto-Verification

| Attribute | Value |
|-----------|-------|
| **Rule** | Accounts created via Google OAuth are automatically email-verified (Google has already verified the email). |

### BR-AUTH-009: Forgot Password Anti-Enumeration

| Attribute | Value |
|-----------|-------|
| **Rule** | The forgot password endpoint always returns a success message, regardless of whether the email exists. Reset tokens expire after 1 hour. |

### BR-AUTH-010: Password Reset Invalidation

| Attribute | Value |
|-----------|-------|
| **Rule** | When a password is reset, all existing refresh tokens for that user are revoked, forcing re-authentication on all devices. |

### BR-AUTH-011: Token Family Revocation

| Attribute | Value |
|-----------|-------|
| **Rule** | If a revoked refresh token is presented for refresh, the system shall revoke ALL tokens in that token family (all descendants of the original token). This indicates a potential token theft. |
| **Reason** | Security: detects refresh token reuse attacks |

### BR-AUTH-012: Password Change Distinction

| Attribute | Value |
|-----------|-------|
| **Rule** | New password must differ from current password. The system should prevent reuse of the last 3 passwords (optional, recommended). |

---

## 4. Academic Structure Rules (BR-ACAD)

### BR-ACAD-001: Academic Year Naming

| Attribute | Value |
|-----------|-------|
| **Rule** | Academic year names must be unique per student. Recommended format: "YYYY-YYYY" (e.g., "2024-2025"). Start year must be ≤ end year. End year must be start year or start year + 1. |
| **Examples** | Valid: "2024-2025", "2025-2025" (single-year program). Invalid: "2025-2024", "2024-2026". |

### BR-ACAD-002: Semester Limit

| Attribute | Value |
|-----------|-------|
| **Rule** | Each academic year can have a maximum of 3 semesters (Semester 1, Semester 2, Summer/Semester 3). |
| **Error** | "Maximum of 3 semesters per academic year." |

### BR-ACAD-003: Semester Name Uniqueness

| Attribute | Value |
|-----------|-------|
| **Rule** | Semester names must be unique within the same academic year. |
| **Error** | "A semester with this name already exists in this academic year." |

### BR-ACAD-004: Cascade Soft Delete

| Attribute | Value |
|-----------|-------|
| **Rule** | Deleting an academic year soft-deletes all semesters within it. Deleting a semester soft-deletes all courses within it. After any deletion, cumulative GPA must be recalculated. |

### BR-ACAD-005: Data Ownership

| Attribute | Value |
|-----------|-------|
| **Rule** | A student can only view, create, edit, and delete their own academic years, semesters, and courses. Any attempt to access another student's data shall return 403 Forbidden. |

---

## 5. Course Rules (BR-COURSE)

### BR-COURSE-001: Credit Range

| Attribute | Value |
|-----------|-------|
| **Rule** | Course credits must be an integer between 1 and 6 (inclusive). |
| **Common Values** | 1 credit (lab), 2 credits (minor), 3 credits (standard), 4 credits (intensive), 5-6 credits (thesis/capstone) |

### BR-COURSE-002: Course Code Format

| Attribute | Value |
|-----------|-------|
| **Rule** | Course code must be 1–20 characters. May contain letters, digits, and hyphens. Case-insensitive for display but stored as entered. |
| **Examples** | "CS101", "MATH-201", "ENG2001" |

### BR-COURSE-003: Retake Course Linking

| Attribute | Value |
|-----------|-------|
| **Rule** | A course marked as "retake" must reference the original course. Both courses remain visible in the transcript. Only the highest-scoring attempt is used for GPA calculation. |
| **Process** | When calculating GPA, if a course has retake(s), use max(original score, retake score). |

### BR-COURSE-004: Soft Delete Behavior

| Attribute | Value |
|-----------|-------|
| **Rule** | Deleted courses are excluded from all GPA calculations but retained in the database. Deleted courses are not visible in the student UI. Deleted courses are visible to admins (marked as deleted). |

---

## 6. Score Calculation Rules (BR-CALC) ⭐ CRITICAL

> These rules define the core scoring logic of the system. Implementation MUST match these rules exactly.

### BR-CALC-001: Course Score Formula

| Attribute | Value |
|-----------|-------|
| **Rule** | Course Score = Attendance × 0.1 + Continuous Assessment × 0.3 + Final Exam × 0.6 |
| **Formula** | `CourseScore = A × 0.1 + C × 0.3 + F × 0.6` |
| **Components** | Attendance (10%), Continuous Assessment (30%), Final Exam (60%) |
| **Precondition** | All three component scores must be present for calculation. If any component is missing, course score is NULL (not calculated). |

### BR-CALC-002: Attendance and Continuous Score Rounding

| Attribute | Value |
|-----------|-------|
| **Rule** | Attendance and Continuous Assessment scores are rounded to the nearest 0.5. |
| **Method** | Round to nearest 0.5: `rounded = Math.Round(value * 2, MidpointRounding.AwayFromZero) / 2` |
| **Examples** | |

| Input | Rounded |
|-------|---------|
| 7.0 | 7.0 |
| 7.1 | 7.0 |
| 7.2 | 7.0 |
| 7.24 | 7.0 |
| 7.25 | 7.5 |
| 7.3 | 7.5 |
| 7.5 | 7.5 |
| 7.7 | 7.5 |
| 7.75 | 8.0 |
| 7.8 | 8.0 |

### BR-CALC-003: Final Exam Score Rounding

| Attribute | Value |
|-----------|-------|
| **Rule** | Final Exam score is rounded to the nearest 0.5. |
| **Method** | Same as BR-CALC-002 |

### BR-CALC-004: Course Score Rounding

| Attribute | Value |
|-----------|-------|
| **Rule** | The calculated course score is rounded to one decimal place. |
| **Method** | Standard rounding: `Math.Round(value, 1, MidpointRounding.AwayFromZero)` |
| **Examples** | |

| Calculated | Rounded |
|------------|---------|
| 7.04 | 7.0 |
| 7.05 | 7.1 |
| 7.14 | 7.1 |
| 7.15 | 7.2 |
| 7.95 | 8.0 |

### BR-CALC-005: Letter Grade Conversion ⭐

| Attribute | Value |
|-----------|-------|
| **Rule** | Course score (10-scale) maps to letter grade and GPA-4 value as follows: |

| Course Score Range | Letter Grade | GPA-4 Value | Classification (VN) |
|-------------------|--------------|-------------|---------------------|
| 9.0 – 10.0 | A+ | 4.0 | Xuất sắc |
| 8.5 – 8.9 | A | 3.7 | Giỏi |
| 8.0 – 8.4 | B+ | 3.5 | Khá giỏi |
| 7.0 – 7.9 | B | 3.0 | Khá |
| 6.5 – 6.9 | C+ | 2.5 | Trung bình khá |
| 5.5 – 6.4 | C | 2.0 | Trung bình |
| 5.0 – 5.4 | D+ | 1.5 | Trung bình yếu |
| 4.0 – 4.9 | D | 1.0 | Yếu |
| 0.0 – 3.9 | F | 0.0 | Không đạt |

| **Boundary Conditions** | Score of exactly 4.0 → D (not F). Score of exactly 9.0 → A+ (not A). All boundaries are inclusive on the lower end. |

### BR-CALC-006: Semester GPA Formula (10-Scale)

| Attribute | Value |
|-----------|-------|
| **Rule** | Semester GPA (10-scale) is the weighted average of all course scores in the semester, weighted by credits. |
| **Formula** | `SemesterGPA₁₀ = Σ(CourseScore_i × Credits_i) / Σ(Credits_i)` |
| **Precision** | Rounded to 2 decimal places |
| **Exclusions** | Courses without all three component scores. Soft-deleted courses. |

### BR-CALC-007: Retake Handling in GPA

| Attribute | Value |
|-----------|-------|
| **Rule** | When a course has been retaken, only the highest course score across all attempts is used in GPA calculation. The credits are counted only once. |
| **Example** | Course "CS101" (3 credits): Original score 4.5 (D), Retake score 7.2 (B). GPA uses 7.2 × 3 credits. |

### BR-CALC-008: Cumulative GPA Formula

| Attribute | Value |
|-----------|-------|
| **Rule** | Cumulative GPA is calculated across ALL non-deleted courses in ALL non-deleted semesters in ALL non-deleted academic years. |
| **Formula** | `CumulativeGPA₁₀ = Σ(BestCourseScore_i × Credits_i) / Σ(Credits_i)` |
| **Note** | "BestCourseScore" accounts for retake handling (BR-CALC-007) |

### BR-CALC-009: Semester GPA Formula (4-Scale)

| Attribute | Value |
|-----------|-------|
| **Rule** | Semester GPA (4-scale) is the weighted average of GPA-4 values for all courses, weighted by credits. |
| **Formula** | `SemesterGPA₄ = Σ(GPA4Value_i × Credits_i) / Σ(Credits_i)` |
| **Precision** | Rounded to 2 decimal places |

### BR-CALC-010: Academic Classification

| Attribute | Value |
|-----------|-------|
| **Rule** | Academic classification is determined by cumulative GPA (10-scale). |

| GPA Range (10-scale) | Classification (EN) | Classification (VN) |
|----------------------|--------------------|--------------------|
| 9.0 – 10.0 | Excellent | Xuất sắc |
| 8.0 – 8.9 | Very Good | Giỏi |
| 7.0 – 7.9 | Good | Khá |
| 5.0 – 6.9 | Average | Trung bình |
| 4.0 – 4.9 | Below Average | Yếu |
| 0.0 – 3.9 | Fail | Kém |

### BR-CALC-011: Credit Completion

| Attribute | Value |
|-----------|-------|
| **Rule** | A course is considered "passed" (credits earned) if the course score ≥ 4.0 (letter grade D or above). Failed courses (F, score < 4.0) do not earn credits. Retaken courses: credits counted only once (for the best passing attempt). |

### BR-CALC-012: Score Input Range

| Attribute | Value |
|-----------|-------|
| **Rule** | All component scores (Attendance, Continuous, Final Exam) must be between 0.0 and 10.0 inclusive. Input precision: up to 1 decimal place (e.g., 7.5, 8.0, 9.3). |

### BR-CALC-013: Calculation Trigger

| Attribute | Value |
|-----------|-------|
| **Rule** | GPA recalculation is triggered automatically whenever: a score component is added or updated, a course is added or deleted, a semester is added or deleted, an academic year is deleted, a retake course is added or its score changes. |

### BR-CALC-014: Empty Data Handling

| Attribute | Value |
|-----------|-------|
| **Rule** | If a semester has no graded courses, its GPA is displayed as "–" (not 0.0). If no semesters have graded courses, cumulative GPA is displayed as "–". Division by zero (zero total credits) returns null/undefined, displayed as "–". |

---

## 7. Goal Planning Rules (BR-GOAL)

### BR-GOAL-001: Required GPA Feasibility

| Attribute | Value |
|-----------|-------|
| **Rule** | If the calculated required semester GPA exceeds 10.0, the system shall display "Goal is not achievable with the specified credits." If the required GPA is ≤ 0, the system shall display "Goal is already achieved." |

### BR-GOAL-002: Goal Overwrite

| Attribute | Value |
|-----------|-------|
| **Rule** | A student can have only one active GPA goal at a time. Setting a new goal replaces the previous one (but the history is retained). |

### BR-GOAL-003: Goal Auto-Achievement

| Attribute | Value |
|-----------|-------|
| **Rule** | When the student's cumulative GPA meets or exceeds the target goal, the goal is automatically marked as "Achieved" and a system notification is generated. |

---

## 8. Prediction Rules (BR-PREDICT)

### BR-PREDICT-001: Final Score Prediction Formula

| Attribute | Value |
|-----------|-------|
| **Rule** | Required Final Exam Score = (TargetCourseScore − Attendance × 0.1 − Continuous × 0.3) / 0.6 |
| **Formula** | `RequiredFinal = (Target - A × 0.1 - C × 0.3) / 0.6` |
| **Rounding** | Result rounded to nearest 0.5 (same as BR-CALC-003) |
| **Feasibility** | If RequiredFinal > 10.0: "Impossible". If RequiredFinal < 0.0: "Guaranteed". |

### BR-PREDICT-002: Target Score Boundaries

| Attribute | Value |
|-----------|-------|
| **Rule** | For multi-scenario prediction, the minimum course score for each grade is: |

| Target Grade | Minimum Course Score |
|-------------|---------------------|
| A+ | 9.0 |
| A | 8.5 |
| B+ | 8.0 |
| B | 7.0 |
| C+ | 6.5 |
| C | 5.5 |
| D+ | 5.0 |
| D | 4.0 |

---

## 9. AI Advisor Rules (BR-AI)

### BR-AI-001: Rate Limiting

| Attribute | Value |
|-----------|-------|
| **Rule** | Each student is limited to 20 AI messages per hour. Exceeding the limit returns: "You've reached the message limit. Please try again in [remaining time]." |

### BR-AI-002: Data Privacy

| Attribute | Value |
|-----------|-------|
| **Rule** | Only the following data is sent to the AI service: course scores, GPA values, credit counts, course names, academic classification, goal information. NEVER sent: student name, email, student code, university name, or any PII. |

### BR-AI-003: Response Language

| Attribute | Value |
|-----------|-------|
| **Rule** | The AI response language matches the student's preferred language setting. If preferred language is Vietnamese, the system prompt instructs the AI to respond in Vietnamese. |

### BR-AI-004: Conversation Limits

| Attribute | Value |
|-----------|-------|
| **Rule** | Each student can have a maximum of 50 active conversations. Message length: 1–2000 characters. AI response: maximum 2000 tokens. Conversations inactive for 90 days are auto-archived (soft delete). |

---

## 10. Notification Rules (BR-NOTIF)

### BR-NOTIF-001: Broadcast Delivery

| Attribute | Value |
|-----------|-------|
| **Rule** | Broadcast notifications create individual notification records for each active student. Locked/deleted accounts do not receive broadcast notifications. |

### BR-NOTIF-002: Notification Retention

| Attribute | Value |
|-----------|-------|
| **Rule** | Notifications are retained for 1 year. After 1 year, read notifications are auto-purged. Unread notifications are retained until read or manually deleted. |

### BR-NOTIF-003: Polling Interval

| Attribute | Value |
|-----------|-------|
| **Rule** | Client polls for new notifications every 30 seconds when the application is in the foreground. No polling when the application tab is inactive (visibility API). |

---

## 11. Transcript Rules (BR-TRANS)

### BR-TRANS-001: Share Link Format

| Attribute | Value |
|-----------|-------|
| **Rule** | Share links use UUID v4 tokens: `https://domain.com/shared/{uuid}`. Tokens are cryptographically random and unguessable. |

### BR-TRANS-002: Share Link Expiry

| Attribute | Value |
|-----------|-------|
| **Rule** | Share links can have: no expiry, 7-day expiry, 30-day expiry, or custom date. Expired links return: "This shared transcript has expired." |

### BR-TRANS-003: Share Link Limit

| Attribute | Value |
|-----------|-------|
| **Rule** | A student can have a maximum of 10 active (non-revoked, non-expired) share links. Creating an 11th link requires revoking an existing one. |

---

## 12. Admin Rules (BR-ADMIN)

### BR-ADMIN-001: Admin Data Access

| Attribute | Value |
|-----------|-------|
| **Rule** | Admins can view student data (profiles, academic records) in read-only mode. Admins cannot create, edit, or delete student academic data (courses, scores, semesters). |

### BR-ADMIN-002: Admin Account Creation

| Attribute | Value |
|-----------|-------|
| **Rule** | Admin accounts cannot be created through the registration flow. Admin accounts are seeded via database migration or created by existing admins. The system must have at least one admin account at all times. |

### BR-ADMIN-003: Self-Protection

| Attribute | Value |
|-----------|-------|
| **Rule** | An admin cannot lock or delete their own account. An admin cannot reset their own password via the admin panel (use normal change password). |

### BR-ADMIN-004: Temporary Password Policy

| Attribute | Value |
|-----------|-------|
| **Rule** | Admin-generated temporary passwords: 12 characters, mix of uppercase + lowercase + digits + special characters. Student must change temporary password on first login. Temporary password expires after 72 hours if not used. |

---

## 13. Calculation Verification Test Cases

> These test cases MUST pass before the GPA calculation engine is considered complete.

### Test Case 1: Basic Course Score Calculation

```
Input:
  Attendance = 8.0
  Continuous = 7.0
  Final = 6.5

Rounded:
  Attendance = 8.0 (no change)
  Continuous = 7.0 (no change)
  Final = 6.5 (no change)

Calculation:
  8.0 × 0.1 + 7.0 × 0.3 + 6.5 × 0.6
  = 0.8 + 2.1 + 3.9
  = 6.8

Course Score = 6.8
Letter Grade = C+
GPA-4 = 2.5
```

### Test Case 2: Rounding Scenarios

```
Input:
  Attendance = 7.3
  Continuous = 8.7
  Final = 5.2

Rounded:
  Attendance = 7.5  (7.3 → nearest 0.5 = 7.5)
  Continuous = 8.5  (8.7 → nearest 0.5 = 8.5)
  Final = 5.0       (5.2 → nearest 0.5 = 5.0)

Calculation:
  7.5 × 0.1 + 8.5 × 0.3 + 5.0 × 0.6
  = 0.75 + 2.55 + 3.0
  = 6.3

Course Score = 6.3
Letter Grade = C
GPA-4 = 2.0
```

### Test Case 3: Semester GPA Calculation

```
Courses:
  CS101: Score = 8.5, Credits = 3
  MATH201: Score = 7.2, Credits = 4
  ENG101: Score = 9.0, Credits = 2

Semester GPA (10-scale):
  = (8.5×3 + 7.2×4 + 9.0×2) / (3 + 4 + 2)
  = (25.5 + 28.8 + 18.0) / 9
  = 72.3 / 9
  = 8.03

Semester GPA₁₀ = 8.03

Semester GPA (4-scale):
  CS101: A = 3.7
  MATH201: B = 3.0
  ENG101: A+ = 4.0

  = (3.7×3 + 3.0×4 + 4.0×2) / (3 + 4 + 2)
  = (11.1 + 12.0 + 8.0) / 9
  = 31.1 / 9
  = 3.46

Semester GPA₄ = 3.46
```

### Test Case 4: Retake Course Handling

```
Original Course:
  CS101 (Sem 1): Score = 3.5 (F), Credits = 3

Retake Course:
  CS101 (Sem 2): Score = 7.0 (B), Credits = 3

GPA Calculation:
  Use score = max(3.5, 7.0) = 7.0
  Credits counted = 3 (once, not 6)
```

### Test Case 5: Final Exam Prediction

```
Given:
  Attendance = 8.0 (rounded: 8.0)
  Continuous = 7.0 (rounded: 7.0)
  Target Grade = B (minimum 7.0)

Required Final:
  = (7.0 - 8.0×0.1 - 7.0×0.3) / 0.6
  = (7.0 - 0.8 - 2.1) / 0.6
  = 4.1 / 0.6
  = 6.833...
  Rounded to 0.5 = 7.0

Result: "You need at least 7.0 on the final exam to get a B."
```

### Test Case 6: Edge Cases

```
All zeros:
  A=0, C=0, F=0 → Score=0.0, Grade=F, GPA4=0.0

All tens:
  A=10, C=10, F=10 → Score=10.0, Grade=A+, GPA4=4.0

Boundary at 4.0:
  Score=4.0 → Grade=D (NOT F), GPA4=1.0

Boundary at 3.9:
  Score=3.9 → Grade=F, GPA4=0.0

Boundary at 9.0:
  Score=9.0 → Grade=A+ (NOT A), GPA4=4.0
```

---

*End of Document — Business Rules*
