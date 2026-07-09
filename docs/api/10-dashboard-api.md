# 10 — Dashboard API

> **Document ID**: API-DASH-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Dashboard metadata endpoint contracts

---

## 1. Dashboard Endpoint Overview

To improve frontend performance, this endpoint returns all core metrics needed to render the student dashboard homepage in a single API call, reducing network round-trips.

---

## 2. Endpoint Specifications

---

### 2.1 GET /dashboard/summary
Retrieves consolidated data for the student dashboard.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "student": {
          "firstName": "Nguyen Van",
          "lastName": "A",
          "studentCode": "2024001"
        },
        "performanceSummary": {
          "cumulativeGpa10": 7.58,
          "cumulativeGpa4": 3.12,
          "classificationVn": "Khá",
          "totalCreditsCompleted": 45,
          "totalCreditsRequired": 120
        },
        "goalProgress": {
          "targetGpa10": 8.00,
          "targetGpa4": 3.60,
          "isAchieved": false,
          "requiredRemainingGpa": 8.35
        },
        "recentCourses": [
          {
            "id": "c62fb33f-8461-460d-85fa-7c961e67fa8b",
            "courseCode": "CS101",
            "courseName": "Intro to Computer Science",
            "credits": 3,
            "courseScore": 8.5,
            "letterGrade": "A"
          }
        ],
        "unreadNotificationsCount": 2
      }
    }
    ```

---

*End of Document — Dashboard API*
