# 06 — Database Architecture

> **Document ID**: ARC-DB-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Database schema design guidelines and persistence strategies

---

## 1. Relational Database Design Principles

The database layer of the Academic GPA Management System is designed to ensure data integrity, performance under concurrent reads, and long-term maintainability. The system uses **Microsoft SQL Server 2022** (or Azure SQL Database) as its relational storage engine.

---

## 2. Naming Conventions

All database elements must adhere to standard database design patterns:

| Element | Convention | Example |
|---|---|---|
| **Table Names** | Plural, PascalCase | `AcademicYears`, `StudentProfiles` |
| **Column Names** | Singular, PascalCase | `StudentCode`, `AttendanceScore` |
| **Primary Keys** | `Id` (GUID type `uniqueidentifier`) | `Id` |
| **Foreign Keys** | SingularTargetTable + `Id` | `StudentProfileId` |
| **Indexes** | `IX_` + TableName + `_` + ColumnNames | `IX_Users_Email` |
| **Unique Constraints**| `UQ_` + TableName + `_` + ColumnNames | `UQ_StudentProfiles_StudentCode` |
| **Check Constraints** | `CK_` + TableName + `_` + ColumnName | `CK_Scores_AttendanceScore` |

---

## 3. Relationships & Cascade Actions

To maintain relational integrity, all foreign keys must configure explicit delete behaviors:
*   **Cascade Soft Delete**: Deleting a student, academic year, or semester marks the parent as deleted (`IsDeleted = 1`). EF Core global query filters automatically exclude these records from queries.
*   **Restricted Deletes (`OnDelete(DeleteBehavior.Restrict)`)**: Crucial entities (such as linking courses or user profiles) must prevent accidental deletion by throwing database constraint errors if dependent records exist.

---

## 4. Migration Strategy

The system uses **EF Core Code-First Migrations** to manage the database schema lifecycle.

### 4.1 Deployment Pipeline
1.  **Development**: Developers modify C# Domain entities and run `dotnet ef migrations add <MigrationName>`.
2.  **Code Review**: Migrations are reviewed alongside source code changes.
3.  **Idempotent Script Generation**: The CI/CD pipeline runs `dotnet ef migrations script --idempotent --output migration.sql`. This script verifies which migrations have run and applies pending updates within single transactions.
4.  **Production Deployment**: The release pipeline runs this idempotent script against the target SQL database, ensuring zero downtime and zero deployment conflicts.

### 4.2 Rollback Strategy
If a migration fails in production:
*   The deployment transaction is rolled back.
*   In emergency rollback situations, the database schema can be reverted using EF Core CLI commands: `update-database <LastGoodMigrationName>`.

---

## 5. Indexing Strategy

To support fast queries, indexes must be defined on all columns frequently used in `WHERE`, `JOIN`, or `ORDER BY` operations:

1.  **Clustered Indexes**: Defaulted to the primary key (`Id`) for all tables.
2.  **Unique Non-Clustered Indexes**: Enforced on `Users(Email)` and `StudentProfiles(StudentCode)` to guarantee uniqueness.
3.  **Filtered Indexes**: Indexes that include a `WHERE` condition to speed up active record queries and reduce index size (e.g. `IX_AcademicYears_StudentId WHERE IsDeleted = 0`).
4.  **Covering Indexes**: Selected queries (such as listing notifications) must use covering indexes that include status columns to prevent table scan lookups.

---

## 6. Schema Constraints

1.  **Numeric Constraints**: All grading and GPA components must use the C# `decimal` type mapped to SQL `decimal(5,2)` or `decimal(3,2)`. Under no circumstances are `float` or `real` columns allowed.
2.  **Range Constraints (Check Constraints)**:
    *   `AttendanceScore`, `ContinuousScore`, and `FinalExamScore` must be bounded between `0.00` and `10.00` (`CK_Scores_Range`).
    *   `CourseCredits` must reside between `1` and `6` (`CK_Courses_Credits`).
3.  **String Constraints**: String fields must configure explicit length limits (e.g., `nvarchar(256)` for passwords, `nvarchar(100)` for names) to avoid memory waste and SQL injection issues.

---

## 7. Scalability & Performance Archiving

*   **Read Replicas**: For scale, the application context configuration can route query operations to read-only replica connection strings, leaving the primary master database dedicated to score writes.
*   **Partitioning**: Tables with high row accumulation (such as `NOTIFICATIONS` or `SCORE_AUDIT_LOG`) can be partitioned vertically by creation date (e.g., partitioning by quarter).
*   **Soft Delete Purge Job**: Background system tasks can run monthly to identify and hard-delete soft-deleted courses and semesters older than 3 years, moving them to offline archive storage.

---

*End of Document — Database Architecture*
