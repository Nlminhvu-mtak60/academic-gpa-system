# 07 — Index Strategy

> **Document ID**: DB-IND-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Index specifications and performance optimization recommendations

---

## 1. Document Purpose

This document details the indexing strategy for the database to ensure fast queries and prevent table scan locks.

---

## 2. Clustered vs. Non-Clustered Indexes

*   **Clustered Indexes**: Every table in the system has a clustered index assigned to its primary key (`Id`).
*   **Sequential Order**: Clustered index sorting utilizes sequential GUIDs (`newsequentialid()`) to ensure new records are appended sequentially, minimizing page splits and keeping write speeds high.
*   **Non-Clustered Indexes**: Applied to columns frequently used in query filters (`WHERE` conditions), join keys (`JOIN`), or sorting (`ORDER BY`).

---

## 3. High-Performance Index Design Specs

The following indexes must be defined in the database configuration:

### 3.1 Unique Non-Clustered Indexes (Data Integrity & Search)
1.  **UX_Users_Email**:
    *   Table: `Users`
    *   Index Key: `Email`
    *   Purpose: Speeds up credentials checks during login.
2.  **UX_StudentProfiles_StudentCode**:
    *   Table: `StudentProfiles`
    *   Index Key: `StudentCode`
    *   Purpose: Enforces unique student registration codes.

---

### 3.2 Filtered Indexes (Soft-Delete Optimization)
Since soft delete indicators (`IsDeleted = 0`) are used extensively on academic years, semesters, and courses, filtered indexes are used to exclude deleted records:

1.  **IX_AcademicYears_StudentProfileId_Active**:
    *   Table: `AcademicYears`
    *   Index Key: `StudentProfileId`
    *   Filter Clause: `WHERE IsDeleted = 0`
2.  **IX_Semesters_AcademicYearId_Active**:
    *   Table: `Semesters`
    *   Index Key: `AcademicYearId`
    *   Filter Clause: `WHERE IsDeleted = 0`
3.  **IX_Courses_SemesterId_Active**:
    *   Table: `Courses`
    *   Index Key: `SemesterId`
    *   Filter Clause: `WHERE IsDeleted = 0`

---

### 3.3 Covering Indexes (Query Speedups)
Covering indexes include additional columns in the index leaf nodes using the `INCLUDE` clause. This allows SQL Server to resolve queries entirely from the index without performing expensive key lookups in the main table:

1.  **IX_Notifications_Recipient_Unread_Covering**:
    *   Table: `Notifications`
    *   Index Key: `RecipientId`, `IsRead`
    *   Include Columns: `Title`, `Message`, `Type`, `CreatedAt`
    *   Purpose: Speeds up the unread notification check triggered by user client polling.
2.  **IX_Scores_Course_Calculated_Covering**:
    *   Table: `Scores`
    *   Index Key: `CourseId`
    *   Include Columns: `AttendanceScore`, `ContinuousScore`, `FinalExamScore`, `CourseScore`, `LetterGrade`, `Gpa4Value`
    *   Purpose: Optimizes GPA calculations, allowing the engine to aggregate scores directly from the index.

---

*End of Document — Index Strategy*
