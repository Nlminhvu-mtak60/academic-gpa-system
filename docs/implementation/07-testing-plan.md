# 07 — Testing Plan

> **Document ID**: PLN-TST-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Quality Assurance strategy detailing Unit, Integration, and Validation checklists for all 18 modules

---

## 1. Document Purpose

This document outlines the Quality Assurance (QA) strategy and testing protocols for the Academic GPA Management System. It specifies the testing framework configuration and defines unit tests, integration tests, automated validation checks, and manual verification steps for each of the 18 system modules.

---

## 2. Testing Frameworks & Setup

*   **Backend Testing**: Built using **xUnit** and **FluentAssertions**. Uses **Moq** for mocking infrastructure services and dependencies. Uses Entity Framework Core's **InMemory provider** or a dedicated local SQL Server testing instance for integration testing.
*   **Frontend Testing**: Component and hooks testing using **Jest** and **React Testing Library**.
*   **End-to-End (E2E) Testing**: Visual validation and multi-role workflows are verified using **Playwright**.
*   **AI Microservice Testing**: FastAPI testing using the built-in **TestClient** and HTTP mockup libraries.

---

## 3. Module Testing Specifications

### Module 1: Foundation
*   **Unit Tests**: Verify the exception handler middleware maps exceptions (e.g., `NotFoundException`, `ValidationException`) to correct HTTP response status codes.
*   **Integration Tests**: Verify database migrations run against empty database schemas.
*   **Validation Checklist**:
    *   API response bodies conform to the unified JSON envelope wrapper `{ success, data, errors, timestamp }`.
*   **Manual Testing Checklist**:
    *   Confirm that server-side errors are caught and do not leak stack traces to the API response.

### Module 2: Authentication
*   **Unit Tests**: Test BCrypt hashing helper (passwords hash correctly and match valid inputs). Test JWT token generation payload structures.
*   **Integration Tests**: Test user register, login, and token refresh endpoints. Verify token family rotation logic blocks replayed refresh tokens.
*   **Validation Checklist**:
    *   Email address formats must conform to RFC 5322 rules.
    *   Passwords must contain at least 8 characters, one uppercase letter, one lowercase letter, one digit, and one special character.
*   **Manual Testing Checklist**:
    *   Verify registration confirmation links are sent to target emails.
    *   Test registration and login flow using mock email details.

### Module 3: Authorization
*   **Unit Tests**: Verify custom policies assign roles (`Student`, `Admin`) correctly. Test OAuth token decoder structures.
*   **Integration Tests**: Ensure endpoints with `[Authorize(Roles = "Admin")]` attribute reject Student tokens with a `403 Forbidden` status.
*   **Validation Checklist**:
    *   Google OAuth tokens are validated against official Google certificate issuers.
*   **Manual Testing Checklist**:
    *   Verify students cannot access admin pages.
    *   Test Google Login button on different browsers.

### Module 4: Student Management (Profile Module)
*   **Unit Tests**: Verify profile update requests mapping.
*   **Integration Tests**: Test profile fetch and edit endpoints. Check that updating university details does not overwrite email identifiers.
*   **Validation Checklist**:
    *   Student Code must be unique.
    *   Required graduation credit limits must fall between 50 and 200.
*   **Manual Testing Checklist**:
    *   Update student details and verify changes persist across dashboard updates.
    *   Test name and major fields for maximum character lengths.

### Module 5: Academic Year
*   **Unit Tests**: Verify academic year sorting logic.
*   **Integration Tests**: Test CRUD endpoints for academic years. Ensure deleting an academic year deletes child semesters cascade-style.
*   **Validation Checklist**:
    *   End year must be exactly one year greater than start year (e.g. 2024 to 2025).
*   **Manual Testing Checklist**:
    *   Create academic years out of order and verify they are listed chronologically in UI dropdowns.

### Module 6: Semester
*   **Unit Tests**: Verify semester type evaluations.
*   **Integration Tests**: Test CRUD endpoints for semesters under academic years. Ensure that attempting to create a fourth semester in a single year fails.
*   **Validation Checklist**:
    *   Semester codes must belong to a predefined enum (`Fall`, `Spring`, `Summer`).
*   **Manual Testing Checklist**:
    *   Verify semester lists render correctly on the dashboard.

### Module 7: Course
*   **Unit Tests**: Verify course credits calculation and retake status assignment.
*   **Integration Tests**: Test course registration endpoints. Verify retaken courses correctly link back to their original course records.
*   **Validation Checklist**:
    *   Course credits must be between 1 and 6.
*   **Manual Testing Checklist**:
    *   Select course retake checkboxes and verify the UI shows updated retake links.

### Module 8: Grade (Component Scores)
*   **Unit Tests**: Verify weighted score equations calculate final course percentages correctly.
*   **Integration Tests**: Test component score editing endpoints. Check that updates generate audit log entries.
*   **Validation Checklist**:
    *   Component scores must fall within the range $0.0 \le \text{Score} \le 10.0$.
    *   Component weights must sum to exactly 100%.
*   **Manual Testing Checklist**:
    *   Input scores using different decimal values (e.g., 8.25, 9.5) and verify they are recorded accurately.

### Module 9: GPA Engine ⭐ CRITICAL
*   **Unit Tests**: Verify calculations for GPA rounding rules (BR-CALC), letter grade mapping (A+ to F), GPA-4 mapping, semester GPA aggregations, and cumulative GPAs (ensuring retaken courses are handled correctly).
*   **Integration Tests**: Test recalculations on course database changes.
*   **Validation Checklist**:
    *   Verification suites must pass all calculation scenarios.
*   **Manual Testing Checklist**:
    *   Manually calculate sample records and verify they match the GPA values shown on the UI dashboard.

### Module 10: Dashboard
*   **Unit Tests**: Verify dashboard summary DTO mappings.
*   **Integration Tests**: Verify the unified dashboard API responds in $< 200\text{ms}$.
*   **Validation Checklist**:
    *   Data payloads must not contain null fields.
*   **Manual Testing Checklist**:
    *   Verify loading skeletons display during API fetches.
    *   Test screen layout responsiveness on different screen widths (mobile, tablet, desktop).

### Module 11: Statistics
*   **Unit Tests**: Verify coordinates generation logic for charts.
*   **Integration Tests**: Test statistics API payloads.
*   **Validation Checklist**:
    *   Ensure data points match the GPA values returned by the GPA Engine.
*   **Manual Testing Checklist**:
    *   Hover over chart points to verify tooltips show the correct GPA values.
    *   Verify progress bars fill up relative to completed credits.

### Module 12: Goal Planner
*   **Unit Tests**: Test equations calculating required GPAs. Verify division-by-zero checks.
*   **Integration Tests**: Test GPA goal creation, fetch, and update APIs.
*   **Validation Checklist**:
    *   Target GPAs must be in the range $0.0 \le \text{Target} \le 4.0$ (or 10.0 equivalent).
*   **Manual Testing Checklist**:
    *   Create a target GPA and verify that changing course scores dynamically updates the remaining GPA goal progress bar.

### Module 13: Prediction Engine
*   **Unit Tests**: Verify formulas predicting required final exam scores.
*   **Integration Tests**: Test final exam prediction APIs.
*   **Validation Checklist**:
    *   Ensure predicted target scores do not exceed 10.0.
*   **Manual Testing Checklist**:
    *   Change target letter grades and check that the predicted exam scores update instantly.

### Module 14: Notification
*   **Unit Tests**: Verify alert message generator mappings.
*   **Integration Tests**: Verify notification status updates (`/read`).
*   **Validation Checklist**:
    *   Unread counts must update correctly.
*   **Manual Testing Checklist**:
    *   Check that unread count badges update when new notifications are received.

### Module 15: Settings
*   **Unit Tests**: Verify locale dictionary file loading.
*   **Integration Tests**: Confirm theme preferences persist to local storage.
*   **Validation Checklist**:
    *   Supported languages are limited to English (`en`) and Vietnamese (`vi`).
*   **Manual Testing Checklist**:
    *   Switch languages and check that all dashboard headers, buttons, and error messages translate correctly.

### Module 16: Share (Transcript Sharing)
*   **Unit Tests**: Verify UUID v4 format and expiration date validations.
*   **Integration Tests**: Test public shared transcript view endpoints. Ensure links return 404 after expiration.
*   **Validation Checklist**:
    *   Ensure public viewer response excludes sensitive student profile details (names, emails, student codes).
*   **Manual Testing Checklist**:
    *   Generate a share link, open it in an incognito browser window, and verify that the layout and styling are correct.

### Module 17: Admin
*   **Unit Tests**: Verify telemetry aggregation rules.
*   **Integration Tests**: Test student search, lock/unlock, and system broadcast APIs.
*   **Validation Checklist**:
    *   Verify locking mechanisms immediately block a student's active token.
*   **Manual Testing Checklist**:
    *   Lock a student profile in the Admin dashboard and verify the student is redirected to the login screen on their next action.

### Module 18: AI Advisor
*   **Unit Tests**: Verify PII filters replace names and emails with placeholder strings. Test API rate limits.
*   **Integration Tests**: Test communication between the .NET backend API and the FastAPI microservice.
*   **Validation Checklist**:
    *   Verify that external LLM API calls are rate limited to 20 per hour per user.
*   **Manual Testing Checklist**:
    *   Send messages to the AI Advisor, check that responses are displayed correctly, and verify the UI handles timeouts or API issues gracefully.

---

*End of Document — Testing Plan*
