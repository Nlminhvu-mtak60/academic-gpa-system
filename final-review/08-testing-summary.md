# Testing Summary — Automated Suites & Coverage

Quality assurance was validated using automated testing suites in the backend and manual validation plans in the frontend.

---

## 1. Test Suite Architecture & Command

- **Test Runner Command**:
  ```bash
  dotnet test backend/
  ```
- **Test Metrics**:
  - Total Tests Executed: **200**
  - Passed: **200**
  - Failed: **0**
  - Skipped: **0**
  - Execution Time: **~7 seconds**

---

## 2. Backend Automated Test Coverage

The backend tests located in the [AcademicGPA.UnitTests/](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/) project are split into two categories:

### Unit Tests
- **Business Computation Verification**:
  - Rounded attendance, continuous, and final score calculations.
  - Weighted final course grade calculations.
  - Vietnamese letter grade scale conversions.
  - GPA calculation algorithm bounds (e.g. 0-credit semesters).
  - Target goal feasibility checks.
- **Security & Pipeline Verification**:
  - Middleware response formatting on uncaught exceptions.
  - Request validation pipeline blocks.
  - JWT creation and signature assertions.
  - Password hashing and salt uniqueness.
  - Input field sanitizations and max length validations.

### Integration Tests (EF Core InMemory)
- **Database CRUD & Lifecycle Logs**:
  - Academic Year creations, cascading checks, and soft delete locks.
  - Course Grade inserts, audit logging, recalculations, and retake overrides.
  - Active conversation thread counts, rate limits, and contextual message storage.

---

## 3. Continuous Integration Gate
The testing suite is configured as a blocking check in the CI/CD workflow:
- Any failing test blocks code merging.
- Standard code compliance is maintained at all times.
