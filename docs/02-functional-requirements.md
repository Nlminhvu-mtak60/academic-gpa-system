# 02 — Functional Requirements Specification

> **Document ID**: SRS-FR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Traceability**: Each requirement maps to User Stories in [05-user-stories.md](./05-user-stories.md) and Use Cases in [08-use-cases.md](./08-use-cases.md)

---

## 1. Document Purpose

This document specifies all functional requirements for the Academic GPA Management System. Each requirement is uniquely identified, prioritized, and categorized by module. These requirements serve as the contractual basis for development and testing.

---

## 2. Requirement Priority Definitions

| Priority | Label | Definition |
|----------|-------|------------|
| **P0** | Must Have | Core functionality without which the system has no value. Required for MVP. |
| **P1** | Should Have | Important features that significantly enhance value. Included in v1.0 but not MVP-blocking. |
| **P2** | Nice to Have | Enhancement features that improve UX. Can be deferred to v1.1+ without impacting core value. |
| **P3** | Future | Planned for future versions. Documented now for architectural consideration. |

---

## 3. Module FR-AUTH: Authentication & Account Management

### FR-AUTH-001: Student Registration

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-001 |
| **Priority** | P0 |
| **Description** | The system shall allow a new user to register a student account using an email address and password. |
| **Input** | Email, password, confirm password, first name, last name |
| **Process** | 1. Validate email format and uniqueness. 2. Validate password meets policy. 3. Hash password with bcrypt. 4. Create user record with role "Student". 5. Send verification email with token (24-hour expiry). 6. Account is inactive until email is verified. |
| **Output** | Success message instructing user to verify email |
| **Business Rules** | BR-AUTH-001, BR-AUTH-002, BR-AUTH-003 |
| **User Story** | US-AUTH-01 |

### FR-AUTH-002: Email Verification

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-002 |
| **Priority** | P0 |
| **Description** | The system shall verify the student's email address via a unique token sent to the registered email. |
| **Input** | Verification token (from email link) |
| **Process** | 1. Validate token exists and is not expired (24h). 2. Mark user's email as verified. 3. Activate user account. 4. Invalidate the used token. |
| **Output** | Success: Redirect to login page with confirmation. Failure: Error message with option to resend. |
| **Business Rules** | BR-AUTH-004 |
| **User Story** | US-AUTH-02 |

### FR-AUTH-003: Student Login

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-003 |
| **Priority** | P0 |
| **Description** | The system shall authenticate a registered student using email and password credentials. |
| **Input** | Email, password |
| **Process** | 1. Find user by email. 2. Check if account is active and email is verified. 3. Check if account is not locked. 4. Verify password hash. 5. Generate JWT access token (15-minute expiry). 6. Generate refresh token (7-day expiry). 7. Store refresh token in database. 8. Update last login timestamp. |
| **Output** | Success: JWT access token, refresh token, user profile data. Failure: Generic error "Invalid email or password". |
| **Business Rules** | BR-AUTH-005, BR-AUTH-006, BR-AUTH-007 |
| **User Story** | US-AUTH-03 |

### FR-AUTH-004: Google OAuth Login

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-004 |
| **Priority** | P1 |
| **Description** | The system shall allow users to register or login using their Google account via OAuth 2.0. |
| **Input** | Google OAuth authorization code |
| **Process** | 1. Exchange authorization code for Google access token. 2. Retrieve user profile from Google (email, name, avatar). 3. If email exists in system: link Google ID and login. 4. If email does not exist: create new account (email auto-verified) and login. 5. Generate JWT + refresh token as normal login. |
| **Output** | Same as FR-AUTH-003 |
| **Business Rules** | BR-AUTH-008 |
| **User Story** | US-AUTH-04 |

### FR-AUTH-005: Forgot Password

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-005 |
| **Priority** | P0 |
| **Description** | The system shall allow a student to request a password reset link sent to their registered email. |
| **Input** | Email address |
| **Process** | 1. Validate email exists in system. 2. Generate unique reset token (1-hour expiry). 3. Send email with reset link containing token. 4. Always show success message (prevent email enumeration). |
| **Output** | Success message: "If an account exists with this email, a reset link has been sent." |
| **Business Rules** | BR-AUTH-009 |
| **User Story** | US-AUTH-05 |

### FR-AUTH-006: Reset Password

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-006 |
| **Priority** | P0 |
| **Description** | The system shall allow a student to set a new password using a valid reset token. |
| **Input** | Reset token, new password, confirm new password |
| **Process** | 1. Validate token exists and is not expired. 2. Validate new password meets policy. 3. Hash new password. 4. Update user's password hash. 5. Invalidate reset token. 6. Revoke all existing refresh tokens (force re-login). |
| **Output** | Success: Redirect to login with confirmation. |
| **Business Rules** | BR-AUTH-002, BR-AUTH-010 |
| **User Story** | US-AUTH-06 |

### FR-AUTH-007: Token Refresh

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-007 |
| **Priority** | P0 |
| **Description** | The system shall issue a new JWT access token when a valid refresh token is presented. |
| **Input** | Current refresh token |
| **Process** | 1. Validate refresh token exists and is not expired/revoked. 2. Generate new JWT access token. 3. Rotate refresh token (issue new, revoke old). 4. Store new refresh token with reference to replaced token. 5. If a revoked token is reused, revoke entire token family (security breach detection). |
| **Output** | New access token + new refresh token |
| **Business Rules** | BR-AUTH-011 |
| **User Story** | US-AUTH-07 |

### FR-AUTH-008: Logout

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-008 |
| **Priority** | P0 |
| **Description** | The system shall invalidate the user's current session upon logout. |
| **Input** | Current refresh token |
| **Process** | 1. Revoke the provided refresh token. 2. Client discards access token. |
| **Output** | Success confirmation |
| **User Story** | US-AUTH-08 |

### FR-AUTH-009: Change Password

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AUTH-009 |
| **Priority** | P0 |
| **Description** | The system shall allow an authenticated student to change their password. |
| **Input** | Current password, new password, confirm new password |
| **Process** | 1. Verify current password. 2. Validate new password meets policy and differs from current. 3. Hash new password. 4. Update password hash. 5. Revoke all refresh tokens except current session. |
| **Output** | Success confirmation |
| **Business Rules** | BR-AUTH-002, BR-AUTH-012 |
| **User Story** | US-AUTH-09 |

---

## 4. Module FR-PROFILE: Profile Management

### FR-PROFILE-001: View Profile

| Attribute | Value |
|-----------|-------|
| **ID** | FR-PROFILE-001 |
| **Priority** | P0 |
| **Description** | The system shall display the authenticated student's profile information. |
| **Output** | First name, last name, email, student code, university name, major, enrollment year, avatar URL, preferred language, preferred theme, account creation date |
| **User Story** | US-PROFILE-01 |

### FR-PROFILE-002: Edit Profile

| Attribute | Value |
|-----------|-------|
| **ID** | FR-PROFILE-002 |
| **Priority** | P0 |
| **Description** | The system shall allow the authenticated student to update their profile information. |
| **Editable Fields** | First name, last name, student code, university name, major name, enrollment year, total required credits |
| **Non-Editable Fields** | Email (identity field), role |
| **Validation** | First/last name: 1–50 characters. Student code: 1–50 characters, unique per user. University name: 1–200 characters. Major: 1–200 characters. Enrollment year: 1900–current year. Total required credits: 1–500. |
| **User Story** | US-PROFILE-02 |

### FR-PROFILE-003: Upload Avatar

| Attribute | Value |
|-----------|-------|
| **ID** | FR-PROFILE-003 |
| **Priority** | P2 |
| **Description** | The system shall allow the student to upload a profile avatar image. |
| **Input** | Image file (JPEG, PNG, WebP) |
| **Constraints** | Max file size: 2 MB. Min dimensions: 100×100 px. Max dimensions: 2000×2000 px. |
| **Process** | 1. Validate file type and size. 2. Resize to 256×256 px. 3. Store in file storage. 4. Update user's avatar URL. 5. Delete previous avatar if exists. |
| **User Story** | US-PROFILE-03 |

### FR-PROFILE-004: Update Preferences

| Attribute | Value |
|-----------|-------|
| **ID** | FR-PROFILE-004 |
| **Priority** | P1 |
| **Description** | The system shall allow the student to update their display preferences. |
| **Editable Fields** | Preferred language (vi, en), Preferred theme (light, dark) |
| **Process** | 1. Validate values against allowed options. 2. Update user preferences. 3. Return updated preferences. 4. Client applies changes immediately without page reload. |
| **User Story** | US-PROFILE-04 |

---

## 5. Module FR-ACADEMIC: Academic Structure Management

### FR-ACADEMIC-001: Create Academic Year

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ACADEMIC-001 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to create an academic year to organize their semesters. |
| **Input** | Year name (e.g., "2024-2025"), start year, end year |
| **Validation** | Year name: 1–20 characters. Start year: 2000–2099. End year: start year or start year + 1. No duplicate year names for the same student. |
| **Process** | 1. Validate inputs. 2. Create academic year record. 3. Set sort order (auto-increment based on start year). |
| **Output** | Created academic year with ID |
| **Business Rules** | BR-ACAD-001 |
| **User Story** | US-ACAD-01 |

### FR-ACADEMIC-002: List Academic Years

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ACADEMIC-002 |
| **Priority** | P0 |
| **Description** | The system shall display all academic years for the authenticated student, ordered by sort order, with summary statistics. |
| **Output** | For each year: ID, year name, start year, end year, number of semesters, total credits in year, year GPA (10-scale), year GPA (4-scale) |
| **Filter** | Exclude soft-deleted years |
| **User Story** | US-ACAD-02 |

### FR-ACADEMIC-003: Update Academic Year

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ACADEMIC-003 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to update an academic year's name, start year, and end year. |
| **Validation** | Same as FR-ACADEMIC-001. User can only edit their own academic years. |
| **User Story** | US-ACAD-03 |

### FR-ACADEMIC-004: Delete Academic Year

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ACADEMIC-004 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to delete an academic year. This is a soft delete. |
| **Process** | 1. Confirm deletion with user. 2. Soft delete the academic year (set IsDeleted = true). 3. Cascade soft delete to all semesters and courses within. 4. Recalculate cumulative GPA. |
| **Constraint** | User can only delete their own academic years. |
| **User Story** | US-ACAD-04 |

### FR-ACADEMIC-005: Create Semester

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ACADEMIC-005 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to create a semester within an academic year. |
| **Input** | Academic year ID, semester name (e.g., "Semester 1", "Semester 2", "Summer") |
| **Validation** | Semester name: 1–50 characters. Academic year must belong to the student. No duplicate semester names within the same academic year. Maximum 3 semesters per academic year. |
| **Business Rules** | BR-ACAD-002 |
| **User Story** | US-ACAD-05 |

### FR-ACADEMIC-006: List Semesters

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ACADEMIC-006 |
| **Priority** | P0 |
| **Description** | The system shall display all semesters within a specified academic year, with summary statistics. |
| **Output** | For each semester: ID, semester name, sort order, number of courses, total credits, semester GPA (10-scale), semester GPA (4-scale), academic classification |
| **User Story** | US-ACAD-06 |

### FR-ACADEMIC-007: Update Semester

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ACADEMIC-007 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to update a semester's name and sort order. |
| **User Story** | US-ACAD-07 |

### FR-ACADEMIC-008: Delete Semester

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ACADEMIC-008 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to soft-delete a semester and all courses within it. |
| **Process** | 1. Confirm deletion. 2. Soft delete semester. 3. Cascade soft delete to all courses. 4. Recalculate academic year GPA and cumulative GPA. |
| **User Story** | US-ACAD-08 |

---

## 6. Module FR-COURSE: Course Management

### FR-COURSE-001: Add Course

| Attribute | Value |
|-----------|-------|
| **ID** | FR-COURSE-001 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to add a course to a specific semester. |
| **Input** | Semester ID, course code, course name, credits, is retake (boolean), original course ID (if retake) |
| **Validation** | Course code: 1–20 characters. Course name: 1–200 characters. Credits: 1–6 (integer). Semester must belong to the student. If retake, original course must exist and belong to the student. |
| **Process** | 1. Validate inputs. 2. Create course record. 3. Create empty score record. 4. Recalculate semester statistics (credit count). |
| **Business Rules** | BR-COURSE-001, BR-COURSE-002 |
| **User Story** | US-COURSE-01 |

### FR-COURSE-002: List Courses in Semester

| Attribute | Value |
|-----------|-------|
| **ID** | FR-COURSE-002 |
| **Priority** | P0 |
| **Description** | The system shall display all courses in a specified semester with score and grade information. |
| **Output** | For each course: ID, course code, course name, credits, is retake, attendance score, continuous score, final exam score, calculated course score, letter grade, GPA-4 value |
| **Sort** | By creation date (default), by course name, by score |
| **User Story** | US-COURSE-02 |

### FR-COURSE-003: Update Course

| Attribute | Value |
|-----------|-------|
| **ID** | FR-COURSE-003 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to update course details (code, name, credits, retake status). |
| **Process** | 1. Validate inputs. 2. Update course. 3. If credits changed, recalculate semester and cumulative GPA. |
| **User Story** | US-COURSE-03 |

### FR-COURSE-004: Delete Course

| Attribute | Value |
|-----------|-------|
| **ID** | FR-COURSE-004 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to soft-delete a course. |
| **Process** | 1. Confirm deletion. 2. Soft delete course and associated score record. 3. Recalculate semester GPA and cumulative GPA. |
| **User Story** | US-COURSE-04 |

### FR-COURSE-005: Input/Update Component Scores

| Attribute | Value |
|-----------|-------|
| **ID** | FR-COURSE-005 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to input or update the three component scores for a course. |
| **Input** | Course ID, attendance score (optional), continuous assessment score (optional), final exam score (optional) |
| **Validation** | Each score: 0.0–10.0, step 0.1. All three are optional (partial entry allowed). |
| **Process** | 1. Validate score ranges. 2. Apply rounding rules per BR-CALC-002 and BR-CALC-003. 3. Store rounded scores. 4. If all three components are present, calculate course score per BR-CALC-001. 5. Determine letter grade per BR-CALC-005. 6. Determine GPA-4 value per BR-CALC-005. 7. Create audit log entry for each changed score. 8. Recalculate semester GPA and cumulative GPA. |
| **Business Rules** | BR-CALC-001, BR-CALC-002, BR-CALC-003, BR-CALC-004, BR-CALC-005 |
| **User Story** | US-COURSE-05 |

### FR-COURSE-006: View Score Audit History

| Attribute | Value |
|-----------|-------|
| **ID** | FR-COURSE-006 |
| **Priority** | P2 |
| **Description** | The system shall display the change history for a course's scores. |
| **Output** | List of changes: field changed, old value, new value, timestamp |
| **Retention** | Last 30 days of changes |
| **User Story** | US-COURSE-06 |

---

## 7. Module FR-GPA: GPA Calculation

### FR-GPA-001: Calculate Semester GPA (10-Scale)

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GPA-001 |
| **Priority** | P0 |
| **Description** | The system shall calculate the weighted average GPA for a semester on the 10-point scale. |
| **Formula** | `Semester GPA₁₀ = Σ(CourseScore × Credits) / Σ(Credits)` |
| **Precision** | Rounded to 2 decimal places |
| **Conditions** | Only includes courses with all three component scores entered. Excludes soft-deleted courses. For retake courses, only the best attempt's score is used. |
| **Business Rules** | BR-CALC-006, BR-CALC-007, BR-CALC-008 |
| **User Story** | US-GPA-01 |

### FR-GPA-002: Calculate Semester GPA (4-Scale)

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GPA-002 |
| **Priority** | P0 |
| **Description** | The system shall calculate the weighted average GPA for a semester on the 4-point scale. |
| **Formula** | `Semester GPA₄ = Σ(GPA4Value × Credits) / Σ(Credits)` |
| **Precision** | Rounded to 2 decimal places |
| **Business Rules** | BR-CALC-006, BR-CALC-009 |
| **User Story** | US-GPA-02 |

### FR-GPA-003: Calculate Academic Year GPA

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GPA-003 |
| **Priority** | P0 |
| **Description** | The system shall calculate the weighted average GPA across all semesters in an academic year. |
| **Formula** | Weighted average of all courses across all semesters in the year |
| **User Story** | US-GPA-03 |

### FR-GPA-004: Calculate Cumulative GPA

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GPA-004 |
| **Priority** | P0 |
| **Description** | The system shall calculate the cumulative GPA across all semesters in all academic years. |
| **Formula** | Weighted average of all non-deleted courses across all non-deleted semesters |
| **Scope** | Both 10-scale and 4-scale |
| **Business Rules** | BR-CALC-008 |
| **User Story** | US-GPA-04 |

### FR-GPA-005: Determine Academic Classification

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GPA-005 |
| **Priority** | P1 |
| **Description** | The system shall determine and display the student's academic classification based on cumulative GPA. |
| **Business Rules** | BR-CALC-010 |
| **User Story** | US-GPA-05 |

---

## 8. Module FR-STATS: Statistics & Analytics

### FR-STATS-001: GPA Trend

| Attribute | Value |
|-----------|-------|
| **ID** | FR-STATS-001 |
| **Priority** | P0 |
| **Description** | The system shall provide GPA trend data across all semesters for chart visualization. |
| **Output** | Array of: {semester name, semester GPA (10), semester GPA (4), cumulative GPA (10) at that point, total credits at that point} |
| **Sort** | Chronological order |
| **User Story** | US-STATS-01 |

### FR-STATS-002: Grade Distribution

| Attribute | Value |
|-----------|-------|
| **ID** | FR-STATS-002 |
| **Priority** | P1 |
| **Description** | The system shall provide grade distribution data (count per letter grade) for chart visualization. |
| **Output** | Array of: {letter grade, count, percentage} |
| **Scope** | All graded courses or filtered by semester |
| **User Story** | US-STATS-02 |

### FR-STATS-003: Credit Progress

| Attribute | Value |
|-----------|-------|
| **ID** | FR-STATS-003 |
| **Priority** | P1 |
| **Description** | The system shall display the student's progress toward total required credits. |
| **Output** | Credits completed (courses with passing grade ≥ 4.0), total required credits (from profile), completion percentage |
| **Business Rules** | BR-CALC-011 |
| **User Story** | US-STATS-03 |

### FR-STATS-004: Semester Comparison

| Attribute | Value |
|-----------|-------|
| **ID** | FR-STATS-004 |
| **Priority** | P1 |
| **Description** | The system shall provide side-by-side comparison data for two or more semesters. |
| **Output** | Per semester: GPA (10), GPA (4), total credits, number of courses, highest grade, lowest grade, classification |
| **User Story** | US-STATS-04 |

### FR-STATS-005: Strongest/Weakest Subjects

| Attribute | Value |
|-----------|-------|
| **ID** | FR-STATS-005 |
| **Priority** | P2 |
| **Description** | The system shall identify the student's highest and lowest scoring courses. |
| **Output** | Top 5 highest-scoring courses, top 5 lowest-scoring courses |
| **User Story** | US-STATS-05 |

---

## 9. Module FR-GOAL: Goal Planner

### FR-GOAL-001: Set GPA Goal

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GOAL-001 |
| **Priority** | P0 |
| **Description** | The system shall allow the student to set a target cumulative GPA. |
| **Input** | Target cumulative GPA (10-scale), optional notes |
| **Validation** | Target GPA: 0.0–10.0. |
| **Process** | 1. Save or update goal. 2. Auto-calculate equivalent 4-scale target. 3. Calculate current progress percentage. |
| **User Story** | US-GOAL-01 |

### FR-GOAL-002: Calculate Required Semester GPA

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GOAL-002 |
| **Priority** | P0 |
| **Description** | The system shall calculate the minimum semester GPA needed in a future semester to reach the target cumulative GPA. |
| **Input** | Target cumulative GPA, estimated credits for next semester |
| **Formula** | `Required GPA = (Target × TotalCreditsAfter − CurrentSum) / NextSemesterCredits` |
| **Output** | Required semester GPA (10-scale and 4-scale). If required GPA > 10.0, display "Target is not achievable with current credits." |
| **Business Rules** | BR-GOAL-001 |
| **User Story** | US-GOAL-02 |

### FR-GOAL-003: What-If Simulator

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GOAL-003 |
| **Priority** | P1 |
| **Description** | The system shall allow the student to simulate hypothetical course scores and see the projected impact on their GPA. |
| **Input** | Array of hypothetical courses: {course name, credits, expected score} |
| **Process** | 1. Take current cumulative GPA data. 2. Add hypothetical courses. 3. Recalculate projected cumulative GPA. 4. Show difference from current GPA. |
| **Output** | Projected cumulative GPA (10 and 4 scale), change from current, projected classification |
| **Note** | Simulation data is NOT persisted. It is ephemeral and calculated client-side or via API. |
| **User Story** | US-GOAL-03 |

### FR-GOAL-004: Goal Progress Tracking

| Attribute | Value |
|-----------|-------|
| **ID** | FR-GOAL-004 |
| **Priority** | P1 |
| **Description** | The system shall display visual progress toward the student's GPA goal. |
| **Output** | Current cumulative GPA, target GPA, progress percentage, trend direction (improving/declining/stable), estimated semesters to reach goal |
| **User Story** | US-GOAL-04 |

---

## 10. Module FR-PREDICT: Final Exam Prediction

### FR-PREDICT-001: Calculate Required Final Exam Score

| Attribute | Value |
|-----------|-------|
| **ID** | FR-PREDICT-001 |
| **Priority** | P0 |
| **Description** | The system shall calculate the minimum final exam score needed to achieve a target letter grade, given the student's attendance and continuous assessment scores. |
| **Input** | Course ID (or attendance + continuous scores), target letter grade |
| **Formula** | `Target Score = (MinScoreForGrade − Attendance×0.1 − Continuous×0.3) / 0.6` |
| **Output** | Required final exam score (rounded to 0.5). If required score > 10.0: "Impossible — target grade cannot be achieved." If required score < 0: "Guaranteed — you will achieve this grade regardless of final exam." |
| **Business Rules** | BR-CALC-001, BR-PREDICT-001 |
| **User Story** | US-PREDICT-01 |

### FR-PREDICT-002: Multi-Scenario Prediction

| Attribute | Value |
|-----------|-------|
| **ID** | FR-PREDICT-002 |
| **Priority** | P1 |
| **Description** | The system shall display the required final exam score for ALL possible letter grades simultaneously. |
| **Output** | Table of: {letter grade, minimum course score needed, required final exam score, feasibility (possible/impossible)} |
| **Example Output** | A+ (9.0): Final ≥ 9.8 ✅, A (8.5): Final ≥ 8.9 ✅, B+ (8.0): Final ≥ 8.1 ✅, ..., F (<4.0): Any score ✅ |
| **User Story** | US-PREDICT-02 |

---

## 11. Module FR-AI: AI Academic Advisor

### FR-AI-001: Start AI Conversation

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AI-001 |
| **Priority** | P1 |
| **Description** | The system shall allow the student to start a new AI advisor conversation. |
| **Process** | 1. Create conversation record. 2. Fetch student's academic context (GPA, scores, trends, goals). 3. Build system prompt with context. 4. Return conversation ID. |
| **User Story** | US-AI-01 |

### FR-AI-002: Send Message to AI

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AI-002 |
| **Priority** | P1 |
| **Description** | The system shall accept a student message, forward it with academic context to the AI service, and return the AI's response. |
| **Input** | Conversation ID, message text |
| **Validation** | Message: 1–2000 characters. Rate limit: 20 messages per hour per student. |
| **Process** | 1. Store student message. 2. Build conversation history + system prompt. 3. Send to FastAPI AI service. 4. Receive and store AI response. 5. Return AI response to client. |
| **Business Rules** | BR-AI-001, BR-AI-002 |
| **User Story** | US-AI-02 |

### FR-AI-003: View Conversation History

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AI-003 |
| **Priority** | P2 |
| **Description** | The system shall display the student's past AI conversations. |
| **Output** | List of conversations: {ID, title (auto-generated from first message), created date, message count} |
| **User Story** | US-AI-03 |

### FR-AI-004: Retrieve Conversation Messages

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AI-004 |
| **Priority** | P2 |
| **Description** | The system shall display all messages in a selected conversation. |
| **Output** | Ordered list of: {role (student/assistant), content, timestamp} |
| **User Story** | US-AI-04 |

### FR-AI-005: Delete Conversation

| Attribute | Value |
|-----------|-------|
| **ID** | FR-AI-005 |
| **Priority** | P2 |
| **Description** | The system shall allow the student to delete an AI conversation and all its messages. |
| **Note** | Hard delete (messages are not retained). |
| **User Story** | US-AI-05 |

---

## 12. Module FR-NOTIF: Notifications

### FR-NOTIF-001: Receive Notifications

| Attribute | Value |
|-----------|-------|
| **ID** | FR-NOTIF-001 |
| **Priority** | P1 |
| **Description** | The system shall deliver in-app notifications to students from admins or system events. |
| **Notification Types** | Admin message, broadcast message, system alert (future: goal milestone, grade warning) |
| **User Story** | US-NOTIF-01 |

### FR-NOTIF-002: List Notifications

| Attribute | Value |
|-----------|-------|
| **ID** | FR-NOTIF-002 |
| **Priority** | P1 |
| **Description** | The system shall display the student's notifications in reverse chronological order with pagination. |
| **Output** | Per notification: ID, title, message preview (100 chars), type, is read, sender name, created date |
| **Pagination** | Default: 20 per page |
| **User Story** | US-NOTIF-02 |

### FR-NOTIF-003: Mark as Read

| Attribute | Value |
|-----------|-------|
| **ID** | FR-NOTIF-003 |
| **Priority** | P1 |
| **Description** | The system shall allow the student to mark individual or all notifications as read. |
| **User Story** | US-NOTIF-03 |

### FR-NOTIF-004: Unread Count Badge

| Attribute | Value |
|-----------|-------|
| **ID** | FR-NOTIF-004 |
| **Priority** | P1 |
| **Description** | The system shall provide the count of unread notifications for badge display. |
| **Output** | Integer count of unread notifications |
| **Polling** | Client polls every 30 seconds |
| **User Story** | US-NOTIF-04 |

---

## 13. Module FR-TRANSCRIPT: Transcript Sharing

### FR-TRANSCRIPT-001: View Transcript

| Attribute | Value |
|-----------|-------|
| **ID** | FR-TRANSCRIPT-001 |
| **Priority** | P1 |
| **Description** | The system shall display a formatted academic transcript for the authenticated student. |
| **Output** | Student name, student code, university, major. Per academic year → per semester → course list with: code, name, credits, course score, letter grade, GPA-4. Semester GPA (10 & 4). Cumulative GPA (10 & 4). Academic classification. Total credits completed. |
| **User Story** | US-TRANSCRIPT-01 |

### FR-TRANSCRIPT-002: Generate Share Link

| Attribute | Value |
|-----------|-------|
| **ID** | FR-TRANSCRIPT-002 |
| **Priority** | P1 |
| **Description** | The system shall generate a unique, unguessable URL for sharing the student's transcript publicly. |
| **Process** | 1. Generate cryptographically random token (UUID v4). 2. Store share record with optional expiry date. 3. Return shareable URL. |
| **Options** | Expiry: none, 7 days, 30 days, custom date |
| **User Story** | US-TRANSCRIPT-02 |

### FR-TRANSCRIPT-003: View Shared Transcript (Public)

| Attribute | Value |
|-----------|-------|
| **ID** | FR-TRANSCRIPT-003 |
| **Priority** | P1 |
| **Description** | The system shall display a read-only transcript when accessed via a valid share link. No authentication required. |
| **Validation** | Token must exist, not be revoked, and not be expired. |
| **Output** | Same content as FR-TRANSCRIPT-001 in read-only format |
| **Additional** | Increment view count. Display "Shared on [date]" and "Valid until [date]". |
| **User Story** | US-TRANSCRIPT-03 |

### FR-TRANSCRIPT-004: Revoke Share Link

| Attribute | Value |
|-----------|-------|
| **ID** | FR-TRANSCRIPT-004 |
| **Priority** | P1 |
| **Description** | The system shall allow the student to revoke a previously generated share link. |
| **Process** | Set IsRevoked = true. Subsequent access to the link returns "This transcript is no longer available." |
| **User Story** | US-TRANSCRIPT-04 |

### FR-TRANSCRIPT-005: List Active Share Links

| Attribute | Value |
|-----------|-------|
| **ID** | FR-TRANSCRIPT-005 |
| **Priority** | P1 |
| **Description** | The system shall display all active (non-revoked, non-expired) share links for the student. |
| **Output** | Per link: token URL, created date, expiry date, view count, revoke action |
| **User Story** | US-TRANSCRIPT-05 |

---

## 14. Module FR-PREF: UI Preferences

### FR-PREF-001: Dark/Light Mode Toggle

| Attribute | Value |
|-----------|-------|
| **ID** | FR-PREF-001 |
| **Priority** | P1 |
| **Description** | The system shall allow the user to switch between dark and light themes. |
| **Behavior** | 1. Toggle is available in the header/sidebar. 2. Theme changes immediately without page reload. 3. Preference is saved to user profile (persisted server-side). 4. On first visit, default to system preference (prefers-color-scheme). |
| **User Story** | US-PREF-01 |

### FR-PREF-002: Language Switcher

| Attribute | Value |
|-----------|-------|
| **ID** | FR-PREF-002 |
| **Priority** | P1 |
| **Description** | The system shall allow the user to switch the UI language between Vietnamese and English. |
| **Behavior** | 1. Language switcher available in header. 2. All UI text updates immediately. 3. Preference saved to user profile. 4. Default: Vietnamese. 5. Date/number formatting adjusts accordingly. |
| **Scope** | All UI labels, form labels, error messages, validation messages, notifications, button text, menu items |
| **User Story** | US-PREF-02 |

---

## 15. Module FR-ADMIN: Administration

### FR-ADMIN-001: Admin Login

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-001 |
| **Priority** | P0 |
| **Description** | The system shall authenticate admin users using the same login mechanism as students, with role-based access control. |
| **Process** | Same as FR-AUTH-003, but upon login, the system checks the user's role and redirects to admin dashboard if role is "Admin". |
| **User Story** | US-ADMIN-01 |

### FR-ADMIN-002: View Student List

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-002 |
| **Priority** | P0 |
| **Description** | The system shall display a paginated list of all registered students with search, filter, and sort capabilities. |
| **Search Fields** | Email, first name, last name, student code |
| **Filter** | Account status (active/locked), email verified (yes/no) |
| **Sort** | Name, email, registration date, last login, cumulative GPA |
| **Output** | Per student: name, email, student code, university, registration date, last login, account status, cumulative GPA |
| **Pagination** | Default: 20 per page |
| **User Story** | US-ADMIN-02 |

### FR-ADMIN-003: View Student Detail

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-003 |
| **Priority** | P0 |
| **Description** | The system shall display a student's full profile and academic data in read-only mode. |
| **Output** | Profile info + all academic years + all semesters + all courses with scores + cumulative GPA |
| **Access** | Read-only. Admin cannot edit student academic data. |
| **User Story** | US-ADMIN-03 |

### FR-ADMIN-004: Lock Student Account

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-004 |
| **Priority** | P0 |
| **Description** | The system shall allow an admin to lock a student's account, preventing them from logging in. |
| **Input** | Student ID, lock reason |
| **Process** | 1. Set user's IsActive = false. 2. Record LockedAt timestamp and LockReason. 3. Revoke all refresh tokens. 4. Student's active sessions are invalidated. |
| **User Story** | US-ADMIN-04 |

### FR-ADMIN-005: Unlock Student Account

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-005 |
| **Priority** | P0 |
| **Description** | The system shall allow an admin to unlock a previously locked student account. |
| **Process** | 1. Set user's IsActive = true. 2. Clear LockedAt and LockReason. 3. Student can login again. |
| **User Story** | US-ADMIN-05 |

### FR-ADMIN-006: Reset Student Password

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-006 |
| **Priority** | P0 |
| **Description** | The system shall allow an admin to reset a student's password to a system-generated temporary password. |
| **Process** | 1. Generate random temporary password (12 characters, mixed case + digits + special). 2. Hash and store as new password. 3. Revoke all refresh tokens. 4. Send email to student with temporary password. 5. Force password change on next login. |
| **User Story** | US-ADMIN-06 |

### FR-ADMIN-007: Delete Student Account

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-007 |
| **Priority** | P1 |
| **Description** | The system shall allow an admin to soft-delete a student account. |
| **Process** | 1. Set user's IsActive = false and mark as deleted. 2. Revoke all tokens. 3. Student data is retained but account is inaccessible. |
| **Note** | Hard delete is NOT supported. Data retention for audit. |
| **User Story** | US-ADMIN-07 |

### FR-ADMIN-008: Admin Dashboard Statistics

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-008 |
| **Priority** | P1 |
| **Description** | The system shall display aggregate statistics on the admin dashboard. |
| **Output** | Total students, active students, locked students, new registrations (this month, this week), average cumulative GPA across all students, GPA distribution histogram data, most common universities, recent activity log |
| **User Story** | US-ADMIN-08 |

### FR-ADMIN-009: Send Notification to Student

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-009 |
| **Priority** | P1 |
| **Description** | The system shall allow an admin to send an in-app notification to a specific student. |
| **Input** | Recipient student ID, title, message body |
| **Validation** | Title: 1–200 characters. Message: 1–2000 characters. |
| **User Story** | US-ADMIN-09 |

### FR-ADMIN-010: Broadcast Notification

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-010 |
| **Priority** | P1 |
| **Description** | The system shall allow an admin to send a notification to ALL active students. |
| **Process** | 1. Create notification with IsBroadcast = true. 2. Create individual notification records for each active student. |
| **User Story** | US-ADMIN-10 |

### FR-ADMIN-011: View Notification History

| Attribute | Value |
|-----------|-------|
| **ID** | FR-ADMIN-011 |
| **Priority** | P1 |
| **Description** | The system shall display a history of notifications sent by the admin. |
| **Output** | Per notification: title, message preview, recipient (name or "All Students"), sent date, read count / total recipients |
| **User Story** | US-ADMIN-11 |

---

## 16. Requirements Traceability Matrix

| Requirement ID | User Story | Use Case | Business Rule | Priority |
|----------------|-----------|----------|---------------|----------|
| FR-AUTH-001 | US-AUTH-01 | UC-01 | BR-AUTH-001,002,003 | P0 |
| FR-AUTH-003 | US-AUTH-03 | UC-02 | BR-AUTH-005,006,007 | P0 |
| FR-AUTH-004 | US-AUTH-04 | UC-03 | BR-AUTH-008 | P1 |
| FR-COURSE-005 | US-COURSE-05 | UC-08 | BR-CALC-001–005 | P0 |
| FR-GPA-001 | US-GPA-01 | UC-09 | BR-CALC-006,007,008 | P0 |
| FR-GPA-004 | US-GPA-04 | UC-09 | BR-CALC-008 | P0 |
| FR-PREDICT-001 | US-PREDICT-01 | UC-11 | BR-CALC-001,BR-PREDICT-001 | P0 |
| FR-AI-002 | US-AI-02 | UC-12 | BR-AI-001,002 | P1 |
| FR-ADMIN-004 | US-ADMIN-04 | UC-15 | — | P0 |

---

*End of Document — Functional Requirements Specification*
