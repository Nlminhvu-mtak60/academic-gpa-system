# 19 — Testing Strategy

> **Document ID**: ARC-BE-TEST-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Test structure guidelines and coverage targets

---

## 1. Document Purpose

This document defines the automated testing strategy, frameworks, mocking libraries, and test coverage targets for the backend codebase.

---

## 2. Testing Frameworks & Mocking Libraries

*   **Testing Engine**: **xUnit** (version 2.x).
*   **Assertion Library**: **FluentAssertions** (provides descriptive and readable test assertions).
*   **Mocking Library**: **NSubstitute** (used to mock interface dependencies during testing).
*   **Database Mocking**: EF Core In-Memory database or SQLite In-Memory providers are used for integration tests.

---

## 3. Testing Slices

The testing strategy is divided into three test layers:

```
Testing Pyramid:
Unit Tests (Domain / Application layers) -> 80% of test volume
   └── Integration Tests (Repositories / DB Context) -> 15% of test volume
          └── System & API Tests (Controllers / Middleware) -> 5% of test volume
```

### 3.1 Unit Tests
*   **Target**: The Domain and Application layers.
*   **Coverage Target**: Minimum **85% code coverage**. The GPA Calculation Engine must achieve **100% coverage**.
*   **Strategy**: Tests verify calculation logic, rounding rules, and validation exceptions. Dependencies are mocked using NSubstitute.

### 3.2 Integration Tests
*   **Target**: The Infrastructure and Persistence layers.
*   **Strategy**: Verifies database queries, soft delete filters, unique constraints, and transaction rollbacks against an in-memory SQL database.

### 3.3 API System Tests
*   **Target**: The Presentation layer.
*   **Strategy**: Uses `WebApplicationFactory` to spin up an in-memory test host. These tests verify API routing, authorization policies, middleware execution, and HTTP status code responses.

---

## 4. Test Class Naming Conventions

*   **Class Names**: TargetClassName + `Tests` (e.g. `GpaCalculatorTests.cs`).
*   **Test Methods**: MethodName + `_` + Scenario + `_` + ExpectedResult (e.g. `CalculateCourseScore_WithDecimalValues_ShouldRoundToNearestHalf`).

---

*End of Document — Testing Strategy*
