# 06 — Primary & Foreign Keys

> **Document ID**: DB-KEYS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Database key definitions and index mappings

---

## 1. Primary Keys: Guid Strategy

All database tables use **Globally Unique Identifiers (GUIDs)**—represented as the `uniqueidentifier` type in SQL Server—as their primary keys.

### 1.1 GUID Selection Rationale
1.  **Decoupled Key Generation**: Keys can be generated on the client or API layer without waiting for database round-trips.
2.  **Distributed-Safe Merging**: Enables safe database imports, backups, and staging merges without key collisions.
3.  **Security**: Prevents users from guessing resource URLs (e.g. `/api/v1/courses/15` vs `/api/v1/courses/c62fb33f-8461-460d-85fa-7c961e67fa8b`).

### 1.2 Performance Optimization
Standard random GUIDs (`NEWID()`) cause index page fragmentation in SQL Server clustered indexes because they are inserted out of order.
*   **Mitigation**: Database default definitions configuration must use `newsequentialid()` for clustered primary key defaults. This creates sequential GUIDs, ensuring SQL Server inserts new rows at the end of index pages, reducing page fragmentation and keeping write speeds high.

---

## 2. Foreign Keys Reference Catalog

The table below lists all foreign key configurations in the database.

| Source Table | FK Column | Target Table | Target Column | Delete Rule | Index recommended |
|:---|:---|:---|:---|:---|:---|
| `RefreshTokens` | `UserId` | `Users` | `Id` | `CASCADE` | Yes (`IX_RefreshTokens_UserId`) |
| `StudentProfiles` | `UserId` | `Users` | `Id` | `RESTRICT` | Yes (`UX_StudentProfiles_UserId`) |
| `AcademicYears` | `StudentProfileId`| `StudentProfiles` | `Id` | `CASCADE` | Yes (`IX_AcademicYears_ProfileId`) |
| `Semesters` | `AcademicYearId` | `AcademicYears` | `Id` | `CASCADE` | Yes (`IX_Semesters_YearId`) |
| `Courses` | `SemesterId` | `Semesters` | `Id` | `CASCADE` | Yes (`IX_Courses_SemesterId`) |
| `Courses` | `OriginalCourseId`| `Courses` | `Id` | `RESTRICT` | Yes (`IX_Courses_OriginalCourse`) |
| `Scores` | `CourseId` | `Courses` | `Id` | `CASCADE` | Yes (`UX_Scores_CourseId`) |
| `ScoreAuditLogs` | `ScoreId` | `Scores` | `Id` | `CASCADE` | Yes (`IX_ScoreAuditLogs_ScoreId`) |
| `GpaGoals` | `StudentProfileId`| `StudentProfiles` | `Id` | `CASCADE` | Yes (`IX_GpaGoals_ProfileId`) |
| `SharedTranscripts`| `StudentProfileId`| `StudentProfiles` | `Id` | `CASCADE` | Yes (`IX_SharedTranscripts_Profile`) |
| `AiConversations` | `StudentProfileId`| `StudentProfiles` | `Id` | `CASCADE` | Yes (`IX_AiConversations_ProfileId`) |
| `AiMessages` | `ConversationId` | `AiConversations` | `Id` | `CASCADE` | Yes (`IX_AiMessages_ConvId`) |
| `Notifications` | `RecipientId` | `Users` | `Id` | `CASCADE` | Yes (`IX_Notifications_Recipient`) |
| `Notifications` | `SenderId` | `Users` | `Id` | `RESTRICT` | Yes (`IX_Notifications_Sender`) |

---

## 3. Referential Integrity Guarantees

1.  **Orphan Prevention**: Foreign keys configure non-nullable fields (e.g. `Courses.SemesterId` must contain a valid GUID), ensuring no orphan records can exist in the database.
2.  **Cascade Triggers**: When a parent record is deleted, Cascade delete rules ensure dependent records are cleaned up automatically (e.g., deleting a semester deletes all associated courses, scores, and audit logs).

---

*End of Document — Primary & Foreign Keys*
