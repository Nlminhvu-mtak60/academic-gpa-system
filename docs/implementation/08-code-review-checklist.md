# 08 — Code Review Checklist

> **Document ID**: PLN-REV-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Code review checklists and code quality guidelines for backend, frontend, and business rule reviews

---

## 1. Document Purpose

This document provides a set of code review checklists and guidelines for the Academic GPA Management System. It ensures that incoming pull requests match the system architecture, security requirements, and core business calculation rules before merge.

---

## 2. Backend Code Review Checklist (.NET 9)

### 2.1 Domain Layer Constraints
- [ ] **No External Dependencies**: Ensure `AcademicGPA.Domain` references zero external NuGet libraries (excluding core system types) and does not reference other projects in the solution.
- [ ] **Rich Domain Logic**: Business entities should enforce encapsulation. Property setters should be private or init-only where appropriate, and state modifications should go through domain methods.
- [ ] **Entities vs. Value Objects**: Ensure entities have unique IDs, while immutable types (e.g. GPA score results) are implemented as Value Objects.

### 2.2 Application Layer (CQRS & Validation)
- [ ] **Decoupled Handlers**: Ensure MediatR `IRequestHandler` classes are isolated and perform a single functional responsibility.
- [ ] **Strict Input Validation**: Verify that every command and query has a corresponding `AbstractValidator<T>` validation class using FluentValidation.
- [ ] **No Leakage of DB details**: Check that handlers map database entities to Data Transfer Objects (DTOs) before returning them to controllers.
- [ ] **Exception Handling**: Validate that handlers do not catch business errors to return custom error objects. Instead, they should throw specific exceptions (e.g., `NotFoundException`) to be handled by global middleware.

### 2.3 Infrastructure Layer (EF Core & Access)
- [ ] **Efficient Database Queries**: Check that read-only queries use `.AsNoTracking()` to improve database performance.
- [ ] **N+1 Query Verification**: Ensure related entities (e.g., loading Semesters with Courses) are loaded efficiently using `.Include()` or projections, avoiding repeated query loops.
- [ ] **Indexed Query Filters**: Confirm that queries utilize database columns configured with indexes (such as StudentCode, Email, or CourseCode).
- [ ] **Security Standards**: Ensure passwords are encrypted using BCrypt with salt rounds $\ge 12$ before saving to the database.

### 2.4 API Layer (Controllers & Endpoints)
- [ ] **Route Versioning**: Confirm that all routes are versioned using the path prefix (e.g., `api/v1/[controller]`).
- [ ] **Consistent JSON Envelope**: Verify that all endpoints return the structured JSON response wrapper `{ success, data, errors, timestamp }`.
- [ ] **Status Code Mapping**: Ensure controllers return correct HTTP statuses: `200 OK` for successful queries, `201 Created` for successful creations, `400 Bad Request` for validations, `401/403` for auth errors, and `404 Not Found` for missing resources.

---

## 3. Frontend Code Review Checklist (React & TS)

### 3.1 Routing & Security Guards
- [ ] **Lazy Loading**: Verify that page views use React lazy-loading (`React.lazy()`) and `Suspense` placeholders to reduce initial bundle sizes.
- [ ] **Route Protection**: Ensure that private student and admin routes are wrapped in route guards that check JWT claims.
- [ ] **Token Refresh**: Confirm that Axios request interceptors automatically refresh JWT access tokens when they expire.

### 3.2 State Management & Components
- [ ] **Separation of Concerns**: Keep React UI components presentation-focused. Complex state logic, calculations, and API interactions must reside in custom Hooks or service classes.
- [ ] **Prop Validation**: Ensure all props are typed with TypeScript interfaces; do not use `any` types.
- [ ] **Key Attributes**: Verify that array rendering (`.map()`) uses unique, stable key values (e.g., entity IDs), never index numbers.

### 3.3 Layout & Styling (Tailwind)
- [ ] **Responsive Classes**: Ensure components use responsive prefixes (`sm:`, `md:`, `lg:`) to verify layouts fit mobile and desktop dimensions.
- [ ] **Color Tokens**: Check that styling elements use Tailwind tokens matching the Color System design document. Do not use custom hex values in code.
- [ ] **Dark Mode Support**: Ensure that every layout class includes dark mode variants using the `dark:` prefix.

---

## 4. GPA Calculation Engine Review Guidelines ⭐ CRITICAL

Because the GPA calculation rules are critical to the application's integrity, pull requests touching `GpaCalculator.cs` or related classes require verification against the BR-CALC requirements:

- [ ] **Decimal Accuracy**: Ensure calculations use the `decimal` data type rather than `float` or `double` to prevent precision errors.
- [ ] **Rounding Rules**:
    *   Verify component scores round to the nearest 0.5.
    *   Verify course summary scores round to one decimal place.
- [ ] **Letter Grade Mapping**: Check that weighted averages map to the correct letter grades (A+ to F) and 4-scale points.
- [ ] **Retake Exclusion**: Confirm that GPA aggregations exclude previous attempts of retaken courses, counting only the highest score.
- [ ] **Zero Credit Edge Case**: Ensure calculations handle zero-credit courses without causing division-by-zero errors.

---

## 5. Security & PII Review Checklist

- [ ] **Sensitive Data Scrubbing**: Check that passwords, JWT keys, and third-party API tokens are not recorded in log files.
- [ ] **AI Advisor PII Filter**: Ensure that the anonymization service replaces student names, emails, and identifiers with generic values (e.g., `STUDENT_X`) before sending data to the FastAPI Python service.
- [ ] **Input Sanitization**: Check that all text inputs are sanitized to protect the application against SQL injection and Cross-Site Scripting (XSS).

---

*End of Document — Code Review Checklist*
