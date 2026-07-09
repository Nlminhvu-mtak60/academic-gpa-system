# 03 — Data Dictionary

> **Document ID**: DB-DD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Column attributes dictionary and metadata references

---

## 1. Document Purpose

This document provides a descriptive metadata dictionary for all columns across the tables of the Academic GPA Management System database.

---

## 2. Table Column Descriptions

### Table: Users
*   **Id** (`uniqueidentifier`): Primary Key. Cryptographically random GUID (UUID v4).
*   **Email** (`nvarchar(100)`): User's primary email. Must be unique. Used for authentication.
*   **PasswordHash** (`nvarchar(256)`): Hashed login password (BCrypt hash value). Nullable for Google-only sign-ins.
*   **FirstName** (`nvarchar(50)`): User's first name. Supports Unicode characters.
*   **LastName** (`nvarchar(50)`): User's last name. Supports Unicode characters.
*   **Role** (`nvarchar(20)`): Access role. Restricts actions. Allowed values: `Student`, `Admin`.
*   **IsActive** (`bit`): Account lock status flag. `1` = Active, `0` = Locked.
*   **IsEmailVerified** (`bit`): Verification indicator. Blocked from login if `0`.
*   **AvatarUrl** (`nvarchar(500)`): URL path pointing to user's uploaded image. Nullable.
*   **PreferredLanguage** (`nvarchar(10)`): Client UI display language code. Defaults to `vi`.
*   **PreferredTheme** (`nvarchar(10)`): Client UI display styling mode. Defaults to `light`.
*   **GoogleId** (`nvarchar(100)`): External federated provider unique identifier. Nullable.
*   **CreatedAt** (`datetime2`): Creation timestamp. Set automatically.
*   **UpdatedAt** (`datetime2`): Modification timestamp. Updated on save.
*   **LastLoginAt** (`datetime2`): Timestamp of the last successful authentication.
*   **LockedAt** (`datetime2`): Timestamp when the account was locked by an Admin.
*   **LockReason** (`nvarchar(200)`): Admin-entered comment explaining lock status.

### Table: RefreshTokens
*   **Id** (`uniqueidentifier`): Primary Key.
*   **UserId** (`uniqueidentifier`): Foreign Key referencing the parent `Users(Id)`.
*   **Token** (`nvarchar(500)`): Encrypted refresh token string.
*   **ReplacedByToken** (`nvarchar(500)`): Tracks token rotation family tree. Nullable.
*   **ExpiresAt** (`datetime2`): Expiry timestamp (exactly 7 days from creation).
*   **CreatedAt** (`datetime2`): Token generation timestamp.
*   **RevokedAt** (`datetime2`): Timestamp of manual logouts or token rotations.
*   **CreatedByIp** (`nvarchar(100)`): Trace IP that requested token issuance.

### Table: StudentProfiles
*   **Id** (`uniqueidentifier`): Primary Key.
*   **UserId** (`uniqueidentifier`): Foreign Key (Unique) referencing the parent `Users(Id)`.
*   **StudentCode** (`nvarchar(50)`): Student Identification Number (MSSV). Must be unique.
*   **UniversityName** (`nvarchar(200)`): Name of the university.
*   **MajorName** (`nvarchar(200)`): Name of the student's major study field.
*   **EnrollmentYear** (`int`): Year the student enrolled (e.g. 2024).
*   **TotalRequiredCredits** (`int`): Total credits needed to graduate. Defaults to 120.

### Table: AcademicYears
*   **Id** (`uniqueidentifier`): Primary Key.
*   **StudentProfileId** (`uniqueidentifier`): Foreign Key referencing the parent `StudentProfiles(Id)`.
*   **YearName** (`nvarchar(20)`): Label for the year (e.g. "2024-2025").
*   **StartYear** (`int`): Calibration start year (e.g. 2024).
*   **EndYear** (`int`): Calibration end year (e.g. 2025).
*   **SortOrder** (`int`): Dictates sequential sorting in display lists.
*   **IsDeleted** (`bit`): Soft-delete indicator flag. Defaults to `0`.
*   **CreatedAt** (`datetime2`): Record creation timestamp.

### Table: Semesters
*   **Id** (`uniqueidentifier`): Primary Key.
*   **AcademicYearId** (`uniqueidentifier`): Foreign Key referencing the parent `AcademicYears(Id)`.
*   **SemesterName** (`nvarchar(50)`): Label (e.g., "Semester 1", "Summer").
*   **SortOrder** (`int`): Display sorting sequence index.
*   **IsDeleted** (`bit`): Soft-delete indicator. Defaults to `0`.
*   **CreatedAt** (`datetime2`): Creation timestamp.

### Table: Courses
*   **Id** (`uniqueidentifier`): Primary Key.
*   **SemesterId** (`uniqueidentifier`): Foreign Key referencing the parent `Semesters(Id)`.
*   **CourseCode** (`nvarchar(20)`): Course code identifier (e.g. "CS101").
*   **CourseName** (`nvarchar(200)`): Display name of the course.
*   **Credits** (`int`): Weight volume of the course (1 to 6).
*   **IsRetake** (`bit`): Retake flag indicator. `1` = Retake attempt.
*   **OriginalCourseId** (`uniqueidentifier`): Self-referencing Foreign Key pointing to the original `Courses(Id)`. Nullable.
*   **IsDeleted** (`bit`): Soft-delete flag.
*   **CreatedAt** (`datetime2`): Creation timestamp.
*   **UpdatedAt** (`datetime2`): Last update timestamp.

### Table: Scores
*   **Id** (`uniqueidentifier`): Primary Key.
*   **CourseId** (`uniqueidentifier`): Foreign Key (Unique) referencing the parent `Courses(Id)`.
*   **AttendanceScore** (`decimal(5,2)`): Component attendance score (0.00 to 10.00). Nullable.
*   **ContinuousScore** (`decimal(5,2)`): Component midterm/homework score (0.00 to 10.00). Nullable.
*   **FinalExamScore** (`decimal(5,2)`): Component exam score (0.00 to 10.00). Nullable.
*   **CourseScore** (`decimal(5,2)`): Auto-calculated final course grade (10-scale). Nullable.
*   **LetterGrade** (`nvarchar(5)`): Converted letter grade (A+ to F). Nullable.
*   **Gpa4Value** (`decimal(3,2)`): Converted GPA value (0.00 to 4.00). Nullable.
*   **CalculatedAt** (`datetime2`): Timestamp of the last grade recalculation.
*   **UpdatedAt** (`datetime2`): Timestamp of the last score modification.

### Table: ScoreAuditLogs
*   **Id** (`uniqueidentifier`): Primary Key.
*   **ScoreId** (`uniqueidentifier`): Foreign Key referencing the parent `Scores(Id)`.
*   **FieldChanged** (`nvarchar(50)`): The field that was modified (e.g. "FinalExamScore").
*   **OldValue** (`nvarchar(20)`): The field value before modification. Nullable.
*   **NewValue** (`nvarchar(20)`): The new field value after modification.
*   **ChangedAt** (`datetime2`): Timestamp when the change occurred.

### Table: GpaGoals
*   **Id** (`uniqueidentifier`): Primary Key.
*   **StudentProfileId** (`uniqueidentifier`): Foreign Key referencing the parent `StudentProfiles(Id)`.
*   **TargetCumulativeGpa10** (`decimal(5,2)`): Target cumulative GPA on the 10-scale.
*   **TargetCumulativeGpa4** (`decimal(3,2)`): Target cumulative GPA on the 4-scale.
*   **Notes** (`nvarchar(500)`): Notes or thoughts entered by the student.
*   **IsAchieved** (`bit`): Target completion indicator flag. Defaults to `0`.
*   **CreatedAt** (`datetime2`): Goal setting timestamp.
*   **UpdatedAt** (`datetime2`): Modification timestamp.

### Table: SharedTranscripts
*   **Id** (`uniqueidentifier`): Primary Key.
*   **StudentProfileId** (`uniqueidentifier`): Foreign Key referencing the parent `StudentProfiles(Id)`.
*   **ShareToken** (`uniqueidentifier`): Cryptographically secure public key used in shared URLs.
*   **ExpiresAt** (`datetime2`): Token expiration timestamp. Nullable (allows perpetual links).
*   **IsRevoked** (`bit`): Revocation indicator flag. Defaults to `0`.
*   **ViewCount** (`int`): Increments each time the shared link is accessed. Defaults to `0`.
*   **CreatedAt** (`datetime2`): Creation timestamp.

### Table: AiConversations
*   **Id** (`uniqueidentifier`): Primary Key.
*   **StudentProfileId** (`uniqueidentifier`): Foreign Key referencing the parent `StudentProfiles(Id)`.
*   **Title** (`nvarchar(200)`): Subject line or title of the conversation.
*   **CreatedAt** (`datetime2`): Creation timestamp.
*   **UpdatedAt** (`datetime2`): Last message timestamp.

### Table: AiMessages
*   **Id** (`uniqueidentifier`): Primary Key.
*   **ConversationId** (`uniqueidentifier`): Foreign Key referencing the parent `AiConversations(Id)`.
*   **Role** (`nvarchar(10)`): Message author. Must be `user` or `assistant`.
*   **Content** (`nvarchar(max)`): The message body.
*   **CreatedAt** (`datetime2`): Message timestamp.

### Table: Notifications
*   **Id** (`uniqueidentifier`): Primary Key.
*   **RecipientId** (`uniqueidentifier`): Foreign Key referencing the destination `Users(Id)`.
*   **SenderId** (`uniqueidentifier`): Foreign Key referencing the sender `Users(Id)`. Nullable (for system-generated alerts).
*   **Title** (`nvarchar(200)`): Header of the notification.
*   **Message** (`nvarchar(2000)`): Notification body.
*   **Type** (`nvarchar(20)`): Classification (e.g. `System`, `GoalAchieved`, `AdminBroadcast`).
*   **IsRead** (`bit`): Notification read status. Defaults to `0`.
*   **ReadAt** (`datetime2`): Timestamp when marked as read. Nullable.
*   **IsBroadcast** (`bit`): Broadcast status indicator flag. Defaults to `0`.
*   **CreatedAt** (`datetime2`): Notification creation timestamp.

### Table: SystemSettings
*   **Id** (`uniqueidentifier`): Primary Key.
*   **SettingKey** (`nvarchar(100)`): Configuration property key. Must be unique.
*   **SettingValue** (`nvarchar(max)`): Config property value.
*   **Description** (`nvarchar(500)`): Detailed description of the setting's purpose.
*   **UpdatedAt** (`datetime2`): Timestamp of the last configuration change.

---

*End of Document — Data Dictionary*
