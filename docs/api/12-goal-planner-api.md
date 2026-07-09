# 12 — Goal Planner API

> **Document ID**: API-GOAL-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Goal Planner endpoint contracts and parameters

---

## 1. Goal Planner Endpoints Overview

These endpoints manage student target cumulative GPAs and calculate the average grades required in remaining semesters to achieve them.

---

## 2. Endpoint Specifications

---

### 2.1 GET /goals
Retrieves the student's active and historical academic goals.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "id": "g12fb33f-8461-460d-85fa-7c961e67fa8b",
          "targetCumulativeGpa10": 8.00,
          "targetCumulativeGpa4": 3.60,
          "notes": "Target for scholarship application.",
          "isAchieved": false,
          "createdAt": "2026-06-24T13:30:00Z"
        }
      ]
    }
    ```

---

### 2.2 POST /goals
Sets a new target cumulative GPA. This replaces the previous active goal.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Request Body**:
    ```json
    {
      "targetCumulativeGpa10": 8.00,
      "notes": "Target for graduation honor."
    }
    ```
*   **Validation Rules**:
    *   `targetCumulativeGpa10`: Required, decimal between `0.00` and `10.00`.
    *   `notes`: Optional, maximum 500 characters.
*   **Success Response** (`201 Created`):
    ```json
    {
      "success": true,
      "message": "Goal set successfully.",
      "data": {
        "id": "g12fb33f-8461-460d-85fa-7c961e67fa8b",
        "targetCumulativeGpa10": 8.00,
        "targetCumulativeGpa4": 3.60
      }
    }
    ```

---

### 2.3 GET /goals/required-gpa
Calculates the average GPA required in remaining credits to achieve the active goal.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "targetCumulativeGpa10": 8.00,
        "currentCumulativeGpa10": 7.42,
        "creditsCompleted": 45,
        "creditsRemaining": 75,
        "requiredRemainingGpa10": 8.35,
        "feasibility": "Achievable",
        "message": "You need an average GPA of 8.35 in your remaining 75 credits to achieve your goal."
      }
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: No active goal defined.
    *   `422 Unprocessable Entity`: Goal is statistically impossible (requires a GPA > 10.00).

---

### 2.4 POST /goals/simulate
Simulates a "what-if" grade scenario for the current semester without saving changes.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Request Body**:
    ```json
    {
      "simulatedCourses": [
        {
          "courseId": "c62fb33f-8461-460d-85fa-7c961e67fa8b",
          "attendanceScore": 9.0,
          "continuousScore": 8.5,
          "finalExamScore": 8.0
        }
      ]
    }
    ```
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "simulatedSemesterGpa10": 8.15,
        "simulatedCumulativeGpa10": 7.62,
        "targetVariance": -0.38
      }
    }
    ```

---

*End of Document — Goal Planner API*
