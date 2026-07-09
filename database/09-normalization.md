# 09 — Normalization Analysis

> **Document ID**: DB-NORM-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Database normalization proof and design exceptions

---

## 1. Document Purpose

This document analyzes the database design of the Academic GPA Management System against relational normalization standards, verifying that the schema satisfies Third Normal Form (3NF) requirements and justifying any intentional design exceptions.

---

## 2. Normalization Analysis

The database is normalized to Third Normal Form (3NF) to ensure data integrity and prevent update anomalies.

### 2.1 First Normal Form (1NF) Validation
*   **Requirements**:
    *   No repeating groups or multi-valued columns.
    *   All column values are atomic.
    *   Each table has a unique identifier (Primary Key).
*   **Proof**:
    *   Every table maps to a single primary key GUID (`uniqueidentifier`).
    *   Data structures are atomic. For example, rather than storing scores in a single string array (e.g. `"8.0,7.5,9.0"`), they are separated into distinct columns: `AttendanceScore`, `ContinuousScore`, and `FinalExamScore`.

### 2.2 Second Normal Form (2NF) Validation
*   **Requirements**:
    *   Must satisfy 1NF.
    *   No partial dependencies (non-key columns must depend on the entire primary key, not a subset). This is naturally satisfied when tables use a single-column primary key.
*   **Proof**:
    *   Since all tables use a single surrogate key (`Id`) as their primary key, partial dependencies are mathematically impossible.
    *   Academic details are isolated from authentication details: the `StudentProfiles` table is separated from the `Users` table. If a student modifies their major or university, it does not affect their core authentication settings.

### 2.3 Third Normal Form (3NF) Validation
*   **Requirements**:
    *   Must satisfy 2NF.
    *   No transitive dependencies (non-key columns must depend only on the primary key, and not on other non-key columns).
*   **Proof**:
    *   Course details are separated from semesters. The `Courses` table stores only its own attributes (code, credits) and references the parent term via `SemesterId`. It does not store the semester's name or year, preventing transitive dependencies.
    *   The `StudentProfiles` table references the `Users` table directly. It does not store the user's name or email, ensuring user updates do not require changes to academic profile tables.

---

## 3. Justified Denormalization (Design Exceptions)

To optimize query speeds, the system includes two intentional exceptions to 3NF:

### 3.1 Derived Columns in the Scores Table (`CourseScore`, `LetterGrade`, `Gpa4Value`)
*   **Condition**: `LetterGrade` and `Gpa4Value` depend transitively on `CourseScore`, which is calculated from component scores (`AttendanceScore`, `ContinuousScore`, `FinalExamScore`). 
*   **Justification**:
    *   **Performance Optimization**: Pre-calculating and persisting these values avoids running complex calculations during high-volume reads (e.g., rendering transcripts, generating dashboard statistics, or compiling AI advisor recommendations).
    *   **Query Simplification**: Storing these values enables simple, high-performance SQL indexing and ordering (e.g. sorting students by GPA directly in SQL queries without recalculating values on the fly).
*   **Data Integrity Protection**: The application layer recalculates these fields automatically within transactional operations whenever component scores are added or updated. Any changes are tracked in the `ScoreAuditLogs` table for auditing.

---

*End of Document — Normalization Analysis*
