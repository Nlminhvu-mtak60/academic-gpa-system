# 04 — Table Design

> **Document ID**: DB-TD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Database table layouts and constraints

---

## 1. Document Purpose

This document details the physical design specifications for all 14 tables in the Academic GPA Management System.

---

## 2. Table Schemas

---

### 2.1 Table: Users
Holds authentication details, access settings, and user interface preferences.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **Email** | `nvarchar(100)` | NO | — | **UQ_Users_Email** |
| **PasswordHash** | `nvarchar(256)` | YES | — | Nullable for OAuth accounts |
| **FirstName** | `nvarchar(50)` | NO | — | — |
| **LastName** | `nvarchar(50)` | NO | — | — |
| **Role** | `nvarchar(20)` | NO | `'Student'` | **CK_Users_Role** (`'Student'`, `'Admin'`) |
| **IsActive** | `bit` | NO | `1` | — |
| **IsEmailVerified** | `bit` | NO | `0` | — |
| **AvatarUrl** | `nvarchar(500)` | YES | — | — |
| **PreferredLanguage**| `nvarchar(10)` | NO | `'vi'` | **CK_Users_Language** (`'vi'`, `'en'`) |
| **PreferredTheme** | `nvarchar(10)` | NO | `'light'` | **CK_Users_Theme** (`'light'`, `'dark'`) |
| **GoogleId** | `nvarchar(100)` | YES | — | **UQ_Users_GoogleId** (Filtered) |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |
| **UpdatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |
| **LastLoginAt** | `datetime2` | YES | — | — |
| **LockedAt** | `datetime2` | YES | — | — |
| **LockReason** | `nvarchar(200)` | YES | — | — |

*   **Check Constraints**:
    *   `CK_Users_Role`: `Role IN ('Student', 'Admin')`
    *   `CK_Users_Language`: `PreferredLanguage IN ('vi', 'en')`
    *   `CK_Users_Theme`: `PreferredTheme IN ('light', 'dark')`
*   **Index Recommendations**:
    *   `UX_Users_Email` (Unique, Non-Clustered) on `Email`
    *   `UX_Users_GoogleId` (Unique, Non-Clustered) on `GoogleId` where `GoogleId IS NOT NULL`

---

### 2.2 Table: RefreshTokens
Secures user sessions and implements refresh token rotation controls.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **UserId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `Users(Id)` (Cascade delete) |
| **Token** | `nvarchar(500)` | NO | — | **UQ_RefreshTokens_Token** |
| **ReplacedByToken** | `nvarchar(500)` | YES | — | — |
| **ExpiresAt** | `datetime2` | NO | — | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |
| **RevokedAt** | `datetime2` | YES | — | — |
| **CreatedByIp** | `nvarchar(100)` | NO | — | — |

*   **Index Recommendations**:
    *   `IX_RefreshTokens_Token` (Non-Clustered) on `Token`
    *   `IX_RefreshTokens_UserId` (Non-Clustered) on `UserId`

---

### 2.3 Table: StudentProfiles
Extends the generic User entity with student-specific academic information.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **UserId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `Users(Id)` (Unique, 1:1, Restrict) |
| **StudentCode** | `nvarchar(50)` | NO | — | **UQ_StudentProfiles_StudentCode** |
| **UniversityName** | `nvarchar(200)` | NO | — | — |
| **MajorName** | `nvarchar(200)` | NO | — | — |
| **EnrollmentYear** | `int` | NO | — | **CK_StudentProfiles_EnrollmentYear** |
| **TotalRequiredCredits**| `int` | NO | `120` | **CK_StudentProfiles_Credits** |

*   **Check Constraints**:
    *   `CK_StudentProfiles_EnrollmentYear`: `EnrollmentYear >= 2000 AND EnrollmentYear <= 2100`
    *   `CK_StudentProfiles_Credits`: `TotalRequiredCredits >= 30 AND TotalRequiredCredits <= 300`
*   **Index Recommendations**:
    *   `UX_StudentProfiles_UserId` (Unique, Non-Clustered) on `UserId`
    *   `UX_StudentProfiles_StudentCode` (Unique, Non-Clustered) on `StudentCode`

---

### 2.4 Table: AcademicYears
Logical grouping of semesters.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **StudentProfileId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `StudentProfiles(Id)` (Cascade delete) |
| **YearName** | `nvarchar(20)` | NO | — | — |
| **StartYear** | `int` | NO | — | **CK_AcademicYears_Years** (Start $\le$ End) |
| **EndYear** | `int` | NO | — | — |
| **SortOrder** | `int` | NO | `0` | — |
| **IsDeleted** | `bit` | NO | `0` | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Check Constraints**:
    *   `CK_AcademicYears_Years`: `StartYear <= EndYear AND EndYear <= StartYear + 1`
*   **Index Recommendations**:
    *   `IX_AcademicYears_StudentProfileId` (Non-Clustered) on `StudentProfileId` where `IsDeleted = 0`

---

### 2.5 Table: Semesters
Logical term boundaries.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **AcademicYearId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `AcademicYears(Id)` (Cascade delete) |
| **SemesterName** | `nvarchar(50)` | NO | — | — |
| **SortOrder** | `int` | NO | `0` | — |
| **IsDeleted** | `bit` | NO | `0` | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Index Recommendations**:
    *   `IX_Semesters_AcademicYearId` (Non-Clustered) on `AcademicYearId` where `IsDeleted = 0`

---

### 2.6 Table: Courses
Holds course definitions, credits, and retake pointers.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **SemesterId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `Semesters(Id)` (Cascade delete) |
| **CourseCode** | `nvarchar(20)` | NO | — | — |
| **CourseName** | `nvarchar(200)` | NO | — | — |
| **Credits** | `int` | NO | `3` | **CK_Courses_Credits** (1 to 6) |
| **IsRetake** | `bit` | NO | `0` | — |
| **OriginalCourseId**| `uniqueidentifier` | YES | — | **FK** $\rightarrow$ `Courses(Id)` (Self-ref, Restrict) |
| **IsDeleted** | `bit` | NO | `0` | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |
| **UpdatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Check Constraints**:
    *   `CK_Courses_Credits`: `Credits >= 1 AND Credits <= 6`
*   **Index Recommendations**:
    *   `IX_Courses_SemesterId` (Non-Clustered) on `SemesterId` where `IsDeleted = 0`
    *   `IX_Courses_OriginalCourseId` (Non-Clustered) on `OriginalCourseId` where `OriginalCourseId IS NOT NULL`

---

### 2.7 Table: Scores
Contains component scores and calculation aggregates.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **CourseId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `Courses(Id)` (Unique, 1:1, Cascade delete) |
| **AttendanceScore** | `decimal(5,2)` | YES | — | **CK_Scores_AttendanceRange** (0 to 10) |
| **ContinuousScore** | `decimal(5,2)` | YES | — | **CK_Scores_ContinuousRange** (0 to 10) |
| **FinalExamScore** | `decimal(5,2)` | YES | — | **CK_Scores_FinalRange** (0 to 10) |
| **CourseScore** | `decimal(5,2)` | YES | — | **CK_Scores_CourseRange** (0 to 10) |
| **LetterGrade** | `nvarchar(5)` | YES | — | — |
| **Gpa4Value** | `decimal(3,2)` | YES | — | **CK_Scores_Gpa4Range** (0 to 4) |
| **CalculatedAt** | `datetime2` | YES | — | — |
| **UpdatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Check Constraints**:
    *   `CK_Scores_AttendanceRange`: `AttendanceScore IS NULL OR (AttendanceScore >= 0.00 AND AttendanceScore <= 10.00)`
    *   `CK_Scores_ContinuousRange`: `ContinuousScore IS NULL OR (ContinuousScore >= 0.00 AND ContinuousScore <= 10.00)`
    *   `CK_Scores_FinalRange`: `FinalExamScore IS NULL OR (FinalExamScore >= 0.00 AND FinalExamScore <= 10.00)`
    *   `CK_Scores_CourseRange`: `CourseScore IS NULL OR (CourseScore >= 0.00 AND CourseScore <= 10.00)`
    *   `CK_Scores_Gpa4Range`: `Gpa4Value IS NULL OR (Gpa4Value >= 0.00 AND Gpa4Value <= 4.00)`
*   **Index Recommendations**:
    *   `UX_Scores_CourseId` (Unique, Non-Clustered) on `CourseId`

---

### 2.8 Table: ScoreAuditLogs
Traces all score updates.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **ScoreId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `Scores(Id)` (Cascade delete) |
| **FieldChanged** | `nvarchar(50)` | NO | — | — |
| **OldValue** | `nvarchar(20)` | YES | — | — |
| **NewValue** | `nvarchar(20)` | NO | — | — |
| **ChangedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Index Recommendations**:
    *   `IX_ScoreAuditLogs_ScoreId` (Non-Clustered) on `ScoreId`

---

### 2.9 Table: GpaGoals
Stores student target goals.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **StudentProfileId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `StudentProfiles(Id)` (Cascade delete) |
| **TargetCumulativeGpa10**| `decimal(5,2)` | NO | — | **CK_GpaGoals_TargetGpa10** (0 to 10) |
| **TargetCumulativeGpa4** | `decimal(3,2)` | NO | — | **CK_GpaGoals_TargetGpa4** (0 to 4) |
| **Notes** | `nvarchar(500)` | YES | — | — |
| **IsAchieved** | `bit` | NO | `0` | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |
| **UpdatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Check Constraints**:
    *   `CK_GpaGoals_TargetGpa10`: `TargetCumulativeGpa10 >= 0.00 AND TargetCumulativeGpa10 <= 10.00`
    *   `CK_GpaGoals_TargetGpa4`: `TargetCumulativeGpa4 >= 0.00 AND TargetCumulativeGpa4 <= 4.00`
*   **Index Recommendations**:
    *   `IX_GpaGoals_StudentProfileId` (Non-Clustered) on `StudentProfileId`

---

### 2.10 Table: SharedTranscripts
Stores transcript sharing tokens.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **StudentProfileId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `StudentProfiles(Id)` (Cascade delete) |
| **ShareToken** | `uniqueidentifier` | NO | `NEWID()` | **UQ_SharedTranscripts_Token** |
| **ExpiresAt** | `datetime2` | YES | — | — |
| **IsRevoked** | `bit` | NO | `0` | — |
| **ViewCount** | `int` | NO | `0` | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Index Recommendations**:
    *   `UX_SharedTranscripts_ShareToken` (Unique, Non-Clustered) on `ShareToken`

---

### 2.11 Table: AiConversations
Tracks AI chatbot sessions.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **StudentProfileId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `StudentProfiles(Id)` (Cascade delete) |
| **Title** | `nvarchar(200)` | NO | — | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |
| **UpdatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Index Recommendations**:
    *   `IX_AiConversations_StudentProfileId` (Non-Clustered) on `StudentProfileId`

---

### 2.12 Table: AiMessages
Stores chat bubbles inside threads.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **ConversationId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `AiConversations(Id)` (Cascade delete) |
| **Role** | `nvarchar(10)` | NO | — | **CK_AiMessages_Role** (`'user'`, `'assistant'`) |
| **Content** | `nvarchar(max)` | NO | — | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Check Constraints**:
    *   `CK_AiMessages_Role`: `Role IN ('user', 'assistant')`
*   **Index Recommendations**:
    *   `IX_AiMessages_ConversationId` (Non-Clustered) on `ConversationId`

---

### 2.13 Table: Notifications
Handles system alerts and admin broadcasts.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **RecipientId** | `uniqueidentifier` | NO | — | **FK** $\rightarrow$ `Users(Id)` (Cascade delete) |
| **SenderId** | `uniqueidentifier` | YES | — | **FK** $\rightarrow$ `Users(Id)` (Restrict) |
| **Title** | `nvarchar(200)` | NO | — | — |
| **Message** | `nvarchar(2000)` | NO | — | — |
| **Type** | `nvarchar(20)` | NO | `'System'` | — |
| **IsRead** | `bit` | NO | `0` | — |
| **ReadAt** | `datetime2` | YES | — | — |
| **IsBroadcast** | `bit` | NO | `0` | — |
| **CreatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Index Recommendations**:
    *   `IX_Notifications_RecipientId_IsRead` (Non-Clustered) on `RecipientId, IsRead`

---

### 2.14 Table: SystemSettings
Global configuration parameters.

| Column | Data Type | Nullable | Default | Keys / Constraints |
|:---|:---|:---:|:---|:---|
| **Id** | `uniqueidentifier` | NO | `NEWID()` | **PK** |
| **SettingKey** | `nvarchar(100)` | NO | — | **UQ_SystemSettings_Key** |
| **SettingValue** | `nvarchar(max)` | NO | — | — |
| **Description** | `nvarchar(500)` | YES | — | — |
| **UpdatedAt** | `datetime2` | NO | `SYSUTCDATETIME()`| — |

*   **Index Recommendations**:
    *   `UX_SystemSettings_SettingKey` (Unique, Non-Clustered) on `SettingKey`

---

*End of Document — Table Design*
