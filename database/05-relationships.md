# 05 — Relationships

> **Document ID**: DB-REL-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Entity relationship mappings and referential constraints

---

## 1. Document Purpose

This document analyzes and specifies all entity relationships in the Academic GPA Management System database, outlining the referential keys and cascade delete rules.

---

## 2. Mapping Categories

The system resolves relationships into **One-to-One (1:1)** and **One-to-Many (1:N)** patterns. There are no direct many-to-many tables; many-to-many concepts (such as student-to-student notifications) are resolved using intermediate junction paths.

---

## 3. One-to-One (1:1) Relationships

### 3.1 Users $\leftrightarrow$ StudentProfiles
*   **Logical Mapping**: Every Student Profile belongs to exactly one authenticated User. An authenticated User can have zero Student Profiles (e.g., an Admin account) or one Student Profile.
*   **Database Key**: `StudentProfiles.UserId` acts as a unique Foreign Key pointing to `Users.Id`.
*   **Delete Behavior**: `ON DELETE RESTRICT` (or `DeleteBehavior.Restrict` in EF Core). Deleting a User account is prevented if an active Student Profile exists, protecting student academic history from accidental deletion.

### 3.2 Courses $\leftrightarrow$ Scores
*   **Logical Mapping**: Every Course has exactly one related Score record.
*   **Database Key**: `Scores.CourseId` acts as a unique Foreign Key pointing to `Courses.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`. Soft-deleting or hard-deleting a course automatically deletes the associated scores and component values.

---

## 4. One-to-Many (1:N) Relationships

### 4.1 Users $\rightarrow$ RefreshTokens
*   **Logical Mapping**: A User can have multiple active Refresh Tokens across their devices.
*   **Database Key**: `RefreshTokens.UserId` points to `Users.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.2 StudentProfiles $\rightarrow$ AcademicYears
*   **Logical Mapping**: A student's profile contains multiple academic years.
*   **Database Key**: `AcademicYears.StudentProfileId` points to `StudentProfiles.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.3 AcademicYears $\rightarrow$ Semesters
*   **Logical Mapping**: An academic year contains multiple semesters (max 3 semesters).
*   **Database Key**: `Semesters.AcademicYearId` points to `AcademicYears.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.4 Semesters $\rightarrow$ Courses
*   **Logical Mapping**: A semester contains multiple courses.
*   **Database Key**: `Courses.SemesterId` points to `Semesters.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.5 Scores $\rightarrow$ ScoreAuditLogs
*   **Logical Mapping**: A score record accumulates multiple modification logs.
*   **Database Key**: `ScoreAuditLogs.ScoreId` points to `Scores.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.6 StudentProfiles $\rightarrow$ GpaGoals
*   **Logical Mapping**: A student sets multiple target GPA milestones over time.
*   **Database Key**: `GpaGoals.StudentProfileId` points to `StudentProfiles.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.7 StudentProfiles $\rightarrow$ SharedTranscripts
*   **Logical Mapping**: A student can generate multiple public transcript sharing tokens.
*   **Database Key**: `SharedTranscripts.StudentProfileId` points to `StudentProfiles.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.8 StudentProfiles $\rightarrow$ AiConversations
*   **Logical Mapping**: A student can initiate multiple chat threads with the AI Advisor.
*   **Database Key**: `AiConversations.StudentProfileId` points to `StudentProfiles.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.9 AiConversations $\rightarrow$ AiMessages
*   **Logical Mapping**: An AI advisor conversation contains multiple individual messages.
*   **Database Key**: `AiMessages.ConversationId` points to `AiConversations.Id`.
*   **Delete Behavior**: `ON DELETE CASCADE`.

### 4.10 Users $\rightarrow$ Notifications
*   **Logical Mapping**: A User can receive multiple notifications.
*   **Database Keys**:
    *   `Notifications.RecipientId` (FK $\rightarrow$ `Users.Id`, `ON DELETE CASCADE`): Recipient reference.
    *   `Notifications.SenderId` (FK $\rightarrow$ `Users.Id`, `ON DELETE RESTRICT`): Sender reference. If an admin account is deleted, their sent system messages are preserved for record-keeping.

---

## 5. Self-Referential Relationships

### 5.1 Courses $\rightarrow$ Courses (Retake Mapping)
*   **Logical Mapping**: A course can point to its original attempt to indicate a retake.
*   **Database Key**: `Courses.OriginalCourseId` (FK $\rightarrow$ `Courses.Id`).
*   **Delete Behavior**: `ON DELETE RESTRICT`. Deleting an original course record is blocked if retake attempts reference it, ensuring academic records remain consistent.

---

*End of Document — Relationships*
