# 13 — Statistics API

> **Document ID**: API-STATS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Statistics and analytics endpoint contracts

---

## 1. Statistics Endpoints Overview

These endpoints retrieve aggregated academic performance analytics, trend data, grade distributions, and subject strength assessments.

---

## 2. Endpoint Specifications

---

### 2.1 GET /statistics/gpa-trend
Returns GPA metrics organized chronologically by semester.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "semesterId": "e32fb33f-8461-460d-85fa-7c961e67fa8b",
          "semesterName": "Semester 1",
          "yearName": "2024-2025",
          "gpa10": 7.10,
          "gpa4": 2.95,
          "cumulativeGpa10": 7.10,
          "cumulativeGpa4": 2.95
        },
        {
          "semesterId": "e32fb33f-8461-460d-85fa-7c961e67fa8c",
          "semesterName": "Semester 2",
          "yearName": "2024-2025",
          "gpa10": 7.60,
          "gpa4": 3.20,
          "cumulativeGpa10": 7.35,
          "cumulativeGpa4": 3.08
        }
      ]
    }
    ```

---

### 2.2 GET /statistics/grade-distribution
Returns the total count of earned letter grades.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "Aplus": 1,
        "A": 3,
        "Bplus": 4,
        "B": 5,
        "Cplus": 2,
        "C": 1,
        "Dplus": 0,
        "D": 0,
        "F": 0
      }
    }
    ```

---

### 2.3 GET /statistics/credit-progress
Retrieves credit completion progress data.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "completedCredits": 45,
        "failedCredits": 3,
        "inProgressCredits": 12,
        "totalRequiredCredits": 120,
        "remainingCredits": 75
      }
    }
    ```

---

### 2.4 GET /statistics/strengths-weaknesses
Analyzes historical performance to identify the student's strongest and weakest courses.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "strongestCourses": [
          {
            "courseCode": "CS101",
            "courseName": "Intro to Computer Science",
            "score": 8.5,
            "letterGrade": "A"
          }
        ],
        "weakestCourses": [
          {
            "courseCode": "MATH101",
            "courseName": "Calculus 1",
            "score": 5.5,
            "letterGrade": "C"
          }
        ]
      }
    }
    ```

---

*End of Document — Statistics API*
