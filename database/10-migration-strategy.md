# 10 — Migration Strategy

> **Document ID**: DB-MIGR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Database deployment pipeline and rollback strategy

---

## 1. Document Purpose

This document details the database migration and deployment strategy for the Academic GPA Management System. It establishes the pipeline for introducing schema changes, managing development environments, deploying updates to production, and executing rollbacks.

---

## 2. Core Migration Framework

The system uses **Entity Framework Core 9 Code-First Migrations** to manage database schema updates.

```
Database Schema Lifecycle Pipeline:
Code Change in C# Domain Entity
   └── Developer runs CLI command to generate Migration metadata
          └── Pull Request reviewed by Team Lead
                 └── CI/CD pipeline runs tests & generates idempotent SQL Script
                        └── Release pipeline applies script to database in single transaction
```

---

## 3. Deployment Pipeline

### 3.1 Step 1: Development Environment (Local)
*   Developers introduce schema modifications by editing C# Domain Entity classes.
*   The developer runs the EF Core migration command:
    ```bash
    dotnet ef migrations add <MigrationName> --project src/AcademicGPA.Infrastructure --startup-project src/AcademicGPA.API
    ```
*   This generates metadata classes and a schema snapshot file in the `AcademicGPA.Infrastructure/Migrations` folder.
*   The developer tests the changes locally by applying the migration:
    ```bash
    dotnet ef database update
    ```

### 3.2 Step 2: Continuous Integration (CI)
*   When a Pull Request is raised, the CI pipeline verifies that the migrations compile and run successfully.
*   The pipeline generates an idempotent SQL script containing all pending database updates:
    ```bash
    dotnet ef migrations script --idempotent --output artifacts/migration.sql --project src/AcademicGPA.Infrastructure
    ```
*   The generated SQL script is archived as a build artifact for production deployment.

### 3.3 Step 3: Production Release (CD)
*   The release pipeline executes the idempotent SQL script (`migration.sql`) against the production SQL Server.
*   **Transactional Execution**: The script wraps schema changes in a single SQL transaction. If a statement fails, the transaction is aborted, rolling back all schema changes to prevent database corruption.

---

## 4. Rollback Strategy

If a migration fails in production:
*   The deployment transaction is rolled back automatically.
*   If a bug is discovered after deployment, the database schema can be reverted to a previous stable state:
    ```bash
    dotnet ef database update <LastGoodMigrationName> --project src/AcademicGPA.Infrastructure --startup-project src/AcademicGPA.API
    ```
*   This executes the `Down` method of the target migration classes, reverting changes in reverse chronological order.

---

## 5. Minimizing Deployment Downtime

To ensure high availability during schema updates, migrations must follow these guidelines:

1.  **Backwards Compatibility**: Schema changes must be backwards-compatible to allow the old API and the new API to run concurrently (e.g. during rolling or blue-green deployments).
2.  **Adding Columns**: New columns must be created as nullable or define default values. This allows the old API to write records without supplying values for the new columns.
3.  **Renaming Columns**: Columns must not be renamed directly. Instead:
    *   Add the new column.
    *   Deploy a code update that writes to both columns.
    *   Run a background data migration script to copy old values to the new column.
    *   Deploy a second code update that reads only from the new column.
    *   Drop the old column in a final migration.

---

*End of Document — Migration Strategy*
