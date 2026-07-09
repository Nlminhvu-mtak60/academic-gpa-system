# 12 — Definition of Done

> **Document ID**: ARC-DOD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Quality checklist and release acceptance criteria

---

## 1. Document Purpose

This document defines the quality standards that every user story or feature must meet before it can be marked as complete. Enforcing a strict Definition of Done (DoD) prevents code regressions, security flaws, and technical debt.

---

## 2. Core Definition of Done (DoD) Checklist

A feature is considered complete only when it satisfies all of the following criteria:

---

### ✓ 1. Architecture Alignment
- The code must align with **Clean Architecture** boundaries. Domain entities must remain free of external framework dependencies, and application logic must communicate through interface abstractions.
- All class instantiations must use the dependency injection container; direct couplings (using the `new` keyword on stateful services) are prohibited.

---

### ✓ 2. Business Rules Enforcement
- Calculations must match the rules specified in the [Business Rules](./../04-business-rules.md) document. 
- Math operations must be verified against the core rounding and letter-grade conversion matrices.

---

### ✓ 3. Input Validation & Security
- User inputs must pass through Zod validations on the frontend and FluentValidation on the backend.
- Inputs must be checked for range boundaries (e.g. scores between 0.0 and 10.0, credits between 1 and 6).
- All endpoints must include authorization checks (`[Authorize]`) unless explicitly marked as public.

---

### ✓ 4. Automated Test Coverage
- **Unit Tests**: Business logic in the domain and application layers must achieve a minimum of **85% code coverage**.
- The Core GPA calculation engine must achieve **100% test coverage**, passing all verification scenarios.
- All automated tests must run and pass in the local build and CI/CD pipelines before code is merged.

---

### ✓ 5. API Documentation (Swagger)
- XML documentation comments must be added to all controller actions, describing parameters and potential HTTP status code responses (e.g. `ProducesResponseType`).
- Swagger/OpenAPI specs must update automatically and render correctly without console errors.

---

### ✓ 6. Code Quality & Review
- **Code Duplication**: Duplicate code must be refactored into reusable services or helper utilities.
- **Code Review**: The pull request must be reviewed and approved by at least one Senior Software Architect.
- Static analysis checks (Roslyn Analyzers and ESLint) must report zero errors.

---

*End of Document — Definition of Done*
