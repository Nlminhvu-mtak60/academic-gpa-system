# 13 — Naming Conventions

> **Document ID**: DB-NAME-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Naming standard specifications

---

## 1. Document Purpose

This document defines the naming standards for all database objects in the Academic GPA Management System. Consistent naming ensures the database remains clean, readable, and easy to maintain across development and production environments.

---

## 2. Naming Standards

---

### 2.1 Table Names
*   **Plural Nouns**: Tables must use plural nouns (e.g. `Users`, `Courses`, `AcademicYears`).
*   **PascalCase**: Names must use PascalCase (no underscores or camelCase).
*   **Junction Tables**: Junction tables (if added in the future) must be named by combining parent tables in alphabetical order (e.g. `CoursesStudents`).

---

### 2.2 Column Names
*   **Singular Nouns**: Columns must use singular nouns (e.g. `Email`, `CourseCode`, `Credits`).
*   **PascalCase**: Names must use PascalCase.
*   **Primary Keys**: Must always be named exactly `Id`.
*   **Foreign Keys**: Must use the singular parent table name followed by `Id` (e.g. `StudentProfileId` pointing to `StudentProfiles(Id)`).

---

### 2.3 Key & Constraint Prefix Standards
To maintain clarity in database error logs and scripts, all constraints must follow prefix standards:

| Constraint Type | Prefix | Example |
|:---|:---:|:---|
| **Primary Key** | `PK_` | `PK_Users` |
| **Foreign Key** | `FK_` + SourceTable + `_` + TargetTable | `FK_Courses_Semesters` |
| **Unique Constraint** | `UQ_` + TableName + `_` + Columns | `UQ_Users_Email` |
| **Check Constraint** | `CK_` + TableName + `_` + Columns | `CK_Scores_FinalRange` |
| **Default Constraint**| `DF_` + TableName + `_` + Column | `DF_Users_IsActive` |

---

### 2.4 Index Prefix Standards
Indexes must follow descriptive naming conventions:

*   **Standard Indexes**: Prefix with `IX_` + TableName + `_` + Columns (e.g. `IX_Courses_SemesterId`).
*   **Unique Indexes**: Prefix with `UX_` + TableName + `_` + Columns (e.g. `UX_Users_Email`).
*   **Filtered Indexes**: Include a descriptor of the filter clause (e.g. `IX_Courses_SemesterId_Active`).

---

*End of Document — Naming Conventions*
