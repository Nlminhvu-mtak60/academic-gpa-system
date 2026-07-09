# Database Review — Schema, Relationships, & Indexing

The database is built on MS SQL Server and mapped using Entity Framework Core 10.0 code-first migrations. 

---

## 1. Relational Schema & Tables

The schema is configured via declarative entity configurations in the [Configurations/](file:///d:/aiiii/backend/src/AcademicGPA.Infrastructure/Persistence/Configurations/) directory:

- **Users & Auth**:
  - `Users`: System credentials, passwords, role attributes, and lock status.
  - `RefreshTokens`: Token string, expiration date, and revocation tracking.
- **Academic Hierarchy**:
  - `AcademicYears`: Defines academic cycles (e.g., "2025-2026").
  - `Semesters`: Belongs to `AcademicYears`. Stores name (e.g., "Semester 1") and credit targets.
  - `Courses`: Belongs to `Semesters`. Stores course name, code, and credits count.
  - `Scores`: Core attendance, continuous, final, calculated course scores, letter grades, and 4.0 GPA. Associated one-to-one with a `Course`.
- **System Interactions**:
  - `UserSettings`: Application theme and language selections.
  - `Notifications`: System, academic, and milestone-related messages.
  - `Conversations` & `ConversationMessages`: Storage for AI advisor chat sessions, message content, and contextual roles.
  - `ScoreAuditLogs` & `UserActivityLogs`: Operational trail for security audits and grade recalculations.

---

## 2. Integrity & Database Constraints

- **Foreign Keys & Cascade Deletes**:
  - Strict foreign keys map semesters to years, courses to semesters, and scores to courses.
  - Delete behaviors are configured to prevent accidental cascading data loss.
- **Field Constraints**:
  - String inputs are size-restricted (e.g., `CourseCode` max length of 15 chars, `PasswordHash` max length of 256 chars).
  - Score components are restricted via FluentValidation and database checks ($0.0 \le \text{grade} \le 10.0$).

---

## 3. Query Performance & Indexing

To optimize GPA aggregation queries and dashboard render speeds, dedicated database indexes have been configured:
- **Unique Indexes**:
  - `Email` unique index in `Users` table for credential lookups.
  - `CourseCode` + `SemesterId` composite unique constraint to prevent duplicate subjects in the same term.
- **Lookup Indexes**:
  - Indexes on foreign keys (`UserId` in `StudentProfile`, `SemesterId` in `Courses`) to avoid table scans during joins.

---

## 4. Soft Delete Implementations

Entities implementing `ISoftDelete` (e.g., `AcademicYear`, `Semester`, `Course`, `Conversation`, `AcademicGoal`) utilize EF Core Global Query Filters:
- When deleted, the system sets `IsDeleted = true` and records `DeletedAt`.
- Queries automatically omit deleted items unless explicitly configured with `.IgnoreQueryFilters()`.
