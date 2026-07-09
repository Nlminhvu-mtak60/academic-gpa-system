# 09 — GPA API

> **Document ID**: API-GPA-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: GPA aggregates calculation endpoint contracts

---

## 1. GPA Endpoints Overview

These endpoints retrieve calculated GPA aggregates (Semester GPA, Academic Year GPA, Cumulative GPA) and academic classifications.

---

## 2. Endpoint Specifications

---

### 2.1 GET /gpa/semester/{semesterId}
Returns the 10-scale and 4-scale GPA for a specific semester.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `semesterId` (guid): Target semester ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "semesterId": "e32fb33f-8461-460d-85fa-7c961e67fa8b",
        "gpa10": 8.03,
        "gpa4": 3.46,
        "totalCredits": 9
      }
    }
    ```
*   **Error Responses**:
    *   `404 Not Found`: Semester does not exist.

---

### 2.2 GET /gpa/academic-year/{yearId}
Returns the aggregated GPA for an academic year.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `yearId` (guid): Target academic year ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "academicYearId": "a62fb33f-8461-460d-85fa-7c961e67fa8b",
        "gpa10": 7.45,
        "gpa4": 3.08,
        "totalCredits": 30
      }
    }
    ```

---

### 2.3 GET /gpa/cumulative
Returns the overall cumulative GPA and total completed credits.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "cumulativeGpa10": 7.58,
        "cumulativeGpa4": 3.12,
        "totalCreditsCompleted": 45,
        "totalCreditsRequired": 120,
        "completionPercentage": 37.5
      }
    }
    ```

---

### 2.4 GET /gpa/classification
Returns the academic classification based on the student's cumulative GPA.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "cumulativeGpa10": 7.58,
        "classificationEn": "Good",
        "classificationVn": "Khá",
        "minimumThresholdGpa10": 7.00
      }
    }
    ```

---

*End of Document — GPA API*
