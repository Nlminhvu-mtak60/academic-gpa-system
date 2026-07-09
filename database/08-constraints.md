# 08 — Constraints

> **Document ID**: DB-CONS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Data validation constraints and default specifications

---

## 1. Document Purpose

This document defines the database constraints that protect data integrity. These constraints prevent out-of-range values, invalid characters, or structural inconsistencies from being saved to the database.

---

## 2. Table Constraints Registry

---

### 2.1 Default Constraints
Timestamps and boolean flags use default values to simplify application inserts:

*   **Timestamps**:
    *   `Users.CreatedAt` / `UpdatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `AcademicYears.CreatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `Semesters.CreatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `Courses.CreatedAt` / `UpdatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `Scores.UpdatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `ScoreAuditLogs.ChangedAt`: Defaults to `SYSUTCDATETIME()`
    *   `GpaGoals.CreatedAt` / `UpdatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `SharedTranscripts.CreatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `AiConversations.CreatedAt` / `UpdatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `AiMessages.CreatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `Notifications.CreatedAt`: Defaults to `SYSUTCDATETIME()`
    *   `SystemSettings.UpdatedAt`: Defaults to `SYSUTCDATETIME()`
*   **Boolean Flags**:
    *   `Users.IsActive` $\rightarrow$ Default `1` (Active)
    *   `Users.IsEmailVerified` $\rightarrow$ Default `0` (Unverified)
    *   `AcademicYears.IsDeleted` $\rightarrow$ Default `0` (Active)
    *   `Semesters.IsDeleted` $\rightarrow$ Default `0` (Active)
    *   `Courses.IsRetake` $\rightarrow$ Default `0` (Standard attempt)
    *   `Courses.IsDeleted` $\rightarrow$ Default `0` (Active)
    *   `GpaGoals.IsAchieved` $\rightarrow$ Default `0` (Goal in progress)
    *   `SharedTranscripts.IsRevoked` $\rightarrow$ Default `0` (Active link)
    *   `Notifications.IsRead` $\rightarrow$ Default `0` (Unread)
    *   `Notifications.IsBroadcast` $\rightarrow$ Default `0` (Direct message)

---

### 2.2 Unique Constraints
Enforces logical uniqueness constraints on key identifier columns:

*   `UQ_Users_Email`: Ensures email addresses are unique across all accounts.
*   `UQ_Users_GoogleId`: Prevents linking a Google ID to multiple accounts (applies only when GoogleId is not null).
*   `UQ_StudentProfiles_StudentCode`: Enforces uniqueness on student registration numbers (MSSV).
*   `UQ_SharedTranscripts_ShareToken`: Enforces uniqueness on transcript share link tokens.
*   `UQ_SystemSettings_SettingKey`: Prevents duplicate entries for configuration keys.

---

### 2.3 Check Constraints (Domain Validation Range)
Check constraints prevent invalid data values from being inserted:

#### Table: Users
*   `CK_Users_Role`: `Role IN ('Student', 'Admin')`
*   `CK_Users_Language`: `PreferredLanguage IN ('vi', 'en')`
*   `CK_Users_Theme`: `PreferredTheme IN ('light', 'dark')`

#### Table: StudentProfiles
*   `CK_StudentProfiles_EnrollmentYear`: `EnrollmentYear >= 2000 AND EnrollmentYear <= 2100`
*   `CK_StudentProfiles_Credits`: `TotalRequiredCredits >= 30 AND TotalRequiredCredits <= 300`

#### Table: AcademicYears
*   `CK_AcademicYears_Years`: `StartYear <= EndYear AND EndYear <= StartYear + 1`

#### Table: Courses
*   `CK_Courses_Credits`: `Credits >= 1 AND Credits <= 6`

#### Table: Scores (Grade Calibration)
*   `CK_Scores_AttendanceRange`: `AttendanceScore IS NULL OR (AttendanceScore >= 0.00 AND AttendanceScore <= 10.00)`
*   `CK_Scores_ContinuousRange`: `ContinuousScore IS NULL OR (ContinuousScore >= 0.00 AND ContinuousScore <= 10.00)`
*   `CK_Scores_FinalRange`: `FinalExamScore IS NULL OR (FinalExamScore >= 0.00 AND FinalExamScore <= 10.00)`
*   `CK_Scores_CourseRange`: `CourseScore IS NULL OR (CourseScore >= 0.00 AND CourseScore <= 10.00)`
*   `CK_Scores_Gpa4Range`: `Gpa4Value IS NULL OR (Gpa4Value >= 0.00 AND Gpa4Value <= 4.00)`

#### Table: AiMessages
*   `CK_AiMessages_Role`: `Role IN ('user', 'assistant')`

---

*End of Document — Constraints*
