# 11 — Prediction API

> **Document ID**: API-PREDICT-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Prediction engine endpoint contracts and parameters

---

## 1. Prediction Endpoints Overview

These endpoints calculate the Final Exam score a student needs to achieve a target course grade, based on their attendance and continuous assessment scores.

---

## 2. Endpoint Specifications

---

### 2.1 POST /prediction/final-score
Predicts the required Final Exam score for a specific target grade.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Request Body**:
    ```json
    {
      "attendanceScore": 8.0,
      "continuousScore": 7.5,
      "targetGrade": "B+"
    }
    ```
*   **Validation Rules**:
    *   `attendanceScore` / `continuousScore`: Required, decimal between `0.00` and `10.00`.
    *   `targetGrade`: Required, must be a valid letter grade (e.g. `'A+'`, `'A'`, `'B+'`).
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "attendanceScore": 8.0,
        "continuousScore": 7.5,
        "targetGrade": "B+",
        "targetScoreThreshold": 8.0,
        "requiredFinalExamScore": 8.5,
        "feasibility": "Achievable",
        "advice": "You need to score at least 8.5 on the Final Exam to secure a B+."
      }
    }
    ```

---

### 2.2 POST /prediction/scenarios
Returns required Final Exam scores for all possible passing letter grades.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Request Body**:
    ```json
    {
      "attendanceScore": 8.0,
      "continuousScore": 7.5
    }
    ```
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "targetGrade": "A+",
          "requiredScore": 9.5,
          "feasibility": "Achievable"
        },
        {
          "targetGrade": "A",
          "requiredScore": 9.0,
          "feasibility": "Achievable"
        },
        {
          "targetGrade": "B+",
          "requiredScore": 8.5,
          "feasibility": "Achievable"
        },
        {
          "targetGrade": "B",
          "requiredScore": 7.0,
          "feasibility": "Achievable"
        },
        {
          "targetGrade": "C+",
          "requiredScore": 6.0,
          "feasibility": "Achievable"
        },
        {
          "targetGrade": "C",
          "requiredScore": 4.5,
          "feasibility": "Achievable"
        },
        {
          "targetGrade": "D+",
          "requiredScore": 3.5,
          "feasibility": "Achievable"
        },
        {
          "targetGrade": "D",
          "requiredScore": 2.0,
          "feasibility": "Guaranteed"
        }
      ]
    }
    ```

---

*End of Document — Prediction API*
