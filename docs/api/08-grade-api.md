# 08 — Grade API

> **Document ID**: API-GRADE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Grades and scores input endpoint contracts

---

## 1. Grade Endpoints Overview

These endpoints allow students to input and update course component scores (Attendance, Continuous Assessment, and Final Exam) and view calculated results.

---

## 2. Endpoint Specifications

---

### 2.1 PUT /courses/{courseId}/scores
Inputs or updates component scores for a course.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `courseId` (guid): Target course ID.
*   **Request Body**:
    ```json
    {
      "attendanceScore": 8.0,
      "continuousScore": 7.5,
      "finalExamScore": 6.5
    }
    ```
*   **Validation Rules**:
    *   All scores (`attendanceScore`, `continuousScore`, `finalExamScore`) are optional to allow partial entries.
    *   If provided, values must be decimals between `0.00` and `10.00` inclusive, with up to one decimal place.
*   **Business Logic**:
    *   **Rounding**: Applies nearest $0.5$ rounding to components before calculation.
    *   **Calculation**: Calculates the final grade using the formula: `Attendance * 0.1 + Continuous * 0.3 + Final * 0.6`. Rounds the final course score to $1$ decimal place.
    *   **Recalculation**: Automatically updates semester and cumulative GPAs.
    *   **Audit Logging**: Inserts audit records into the `ScoreAuditLogs` table for any modified fields.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Scores updated and grades recalculated successfully.",
      "data": {
        "courseId": "c62fb33f-8461-460d-85fa-7c961e67fa8b",
        "attendanceScore": 8.0,
        "continuousScore": 7.5,
        "finalExamScore": 6.5,
        "courseScore": 6.8,
        "letterGrade": "C+",
        "gpa4Value": 2.5
      }
    }
    ```

---

### 2.2 GET /courses/{courseId}/scores
Retrieves the scores and calculated results for a course.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `courseId` (guid): Target course ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "attendanceScore": 8.0,
        "continuousScore": 7.5,
        "finalExamScore": 6.5,
        "courseScore": 6.8,
        "letterGrade": "C+",
        "gpa4Value": 2.5,
        "calculatedAt": "2026-06-24T13:30:00Z"
      }
    }
    ```

---

### 2.3 GET /courses/{courseId}/scores/audit
Returns the audit log of all changes to a course's scores.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `courseId` (guid): Target course ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "fieldChanged": "FinalExamScore",
          "oldValue": "5.0",
          "newValue": "6.5",
          "changedAt": "2026-06-24T13:30:00Z"
        }
      ]
    }
    ```

---

*End of Document — Grade API*
