# Integration Test Verification Report (Phase 10)

This document contains the execution log, verified workflows, and coverage notes for the backend integration test suite of the Academic GPA Management System.

---

## 1. Integration Test Suite Structure

Integration tests verify component interactions, database state changes, and transaction handling via an Entity Framework Core in-memory provider.

### Core Test Classes

#### [IntegrationTestBase.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/IntegrationTestBase.cs)
- Shared fixture setting up in-memory `ApplicationDbContext`.
- Seeds a test student account and credentials context.
- Exposes mockable helper contexts.

#### [AcademicYearIntegrationTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/AcademicYearIntegrationTests.cs)
- Creates academic years and validates database insertion.
- Updates year name and verified changes persist.
- Validates deletion constraints: throwing exception if semesters exist, and soft-deleting successfully if year is empty.
- Checks unique constraints of academic year name per profile.

#### [CourseGradeIntegrationTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/CourseGradeIntegrationTests.cs)
- Simulates course grade submissions via `UpdateScoresCommandHandler`.
- Verifies automatic formula execution and rounding rules:
  - Attendance, Continuous, and Final scores round to nearest 0.5.
  - Generates correct Course Score, GPA-4 value, Vietnamese letter grade, and Pass/Fail status.
- Verifies score audit logs capture initial inserts and value diffs.
- Verifies clearing scores (setting null) resets all calculated values.

#### [GpaCalculationIntegrationTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/GpaCalculationIntegrationTests.cs)
- Configures full curriculum details.
- Validates semester-specific and cumulative GPA calculations.
- Verifies retake attempts override previous lower scores in cumulative GPA calculations but are retained for chronological semester-level GPA calculations.
- Verifies attempted, passed, and failed credit counts.

#### [ConversationIntegrationTests.cs](file:///d:/aiiii/backend/tests/AcademicGPA.UnitTests/ConversationIntegrationTests.cs)
- Enforces active chat thread limit (max 50 threads throws error).
- Verifies cross-student security: students cannot view or send messages on threads they do not own.
- Verifies hourly messaging rate limits (max 20 messages per hour throws error).
- Verifies conversation soft-deletion toggles the `IsDeleted` database property.

---

## 2. Test Run Results

- **Status**: **Passing**
- **Failures**: 0
- **Validation**: Database transitions, audits, constraints, and validation behaviors verified.
