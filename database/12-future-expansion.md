# 12 — Future Expansion

> **Document ID**: DB-EXP-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Database scalability patterns and multi-tenant schema overlays

---

## 1. Document Purpose

This document details the database schema modifications and architectural changes required to scale the database to support multiple universities, dynamic grading scales, and high-volume data partitioning.

---

## 2. Multi-Tenant Expansion (Multiple Universities)

To support multiple universities within a single database instance, we will introduce a **Logical Multi-Tenancy** model using shared tables.

### 2.1 Schema Updates
1.  **Add `Tenants` Table**:
    *   Columns: `Id` (PK, uniqueidentifier), `Name` (nvarchar(200)), `Domain` (nvarchar(100)), `ThemeConfig` (nvarchar(max)), `CreatedAt` (datetime2).
2.  **Add `TenantId` Column**:
    *   Add a non-nullable `TenantId` foreign key referencing `Tenants(Id)` to the following tables: `StudentProfiles`, `AcademicYears`, `Semesters`, and `Courses`.
3.  **Unique Constraint Updates**:
    *   Update unique constraints to include `TenantId` (e.g. `UQ_StudentProfiles_StudentCode` becomes a composite unique constraint on `TenantId, StudentCode`).

---

## 3. Dynamic Grading Systems

To support universities with different grading systems, static grading conversions will be replaced with database-driven configuration tables:

```
Dynamic Grading Schema Design:
[GradingScales] 
   └── Id (PK)
   └── Name (e.g. "Standard VN", "US GPA 4.0")
   └── FormulaExpression (e.g. "A*0.1 + C*0.3 + F*0.6")
   └── [GradeThresholds] (1-to-many relationship)
          └── Id (PK)
          └── GradingScaleId (FK)
          └── LetterGrade (e.g. "A+")
          └── MinScore (e.g. 9.00)
          └── Gpa4Value (e.g. 4.00)
          └── ClassificationVN (e.g. "Xuất sắc")
```

### 3.1 Schema Execution
*   Link `StudentProfiles.GradingScaleId` to the `GradingScales` table.
*   The `GpaCalculatorService` reads the student's active `GradingScale` and `GradeThresholds` at runtime to apply the correct grading rules and formulas.

---

## 4. High-Volume Partitioning (Audit Logs & Notifications)

As user activity grows, the `ScoreAuditLogs` and `Notifications` tables will accumulate millions of records.
*   **Partitioning Key**: Apply vertical partitioning on the `ChangedAt` (for audit logs) and `CreatedAt` (for notifications) timestamp columns.
*   **Archiving**: Partition data by year. Older partitions (e.g. audit logs older than 3 years) can be moved to read-only archival tables or cold storage, keeping active tables small and fast.

---

*End of Document — Future Expansion*
