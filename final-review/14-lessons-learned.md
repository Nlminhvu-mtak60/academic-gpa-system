# Lessons Learned — Retrospective Technical Takeaways

This document outlines key lessons learned during the development, testing, and DevOps stages of the Academic GPA Management System.

---

## 1. Architectural Cleanliness & Testing Benefits

- **Isolating Business Logic**:
  - Structuring the system using Clean Architecture simplified testing.
  - Keeping domain rules independent from Entity Framework allowed us to test grading calculations and target-prediction formulas without setting up database connections.
- **CQRS and MediatR decoupling**:
  - Splitting write actions (Commands) from read actions (Queries) kept code handlers small and maintainable.
  - It also simplified adding cross-cutting concerns (such as request validation and user logging) without changing the core query logic.

---

## 2. Dynamic Tech Integration (FastAPI and ASP.NET Core)

- **Microservice Separation**:
  - Running AI analytical recommendations on a FastAPI service while leaving data storage and user sessions to ASP.NET Core worked well.
  - Using Python for recommendations allowed us to leverage standard libraries, while ASP.NET Core managed type safety, database transactions, and user sessions.

---

## 3. DevOps & Routing Simplifications

- **Nginx Reverse Proxy Utility**:
  - Routing React client requests and proxying `/api` queries to the backend using Nginx simplified CORS setup.
  - It allowed the API and static frontend to run under the same domain host, reducing client-side routing issues.
- **Auto EF Migrations in Containers**:
  - Running migrations on startup (`ApplyMigrationsOnStartup=true`) simplified setting up container environments.
  - For high-availability multi-instance setups, executing database updates during deployment pipelines is preferred to avoid lock contentions.
