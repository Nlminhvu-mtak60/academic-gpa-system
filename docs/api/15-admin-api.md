# 15 — Admin API

> **Document ID**: API-ADMIN-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Administrative operational endpoint contracts

---

## 1. Admin Endpoints Overview

These endpoints allow system administrators to view system-wide usage statistics, monitor performance metrics, and send targeted or broadcast notifications.

---

## 2. Endpoint Specifications

---

### 2.1 GET /admin/statistics
Returns aggregated system-wide performance and engagement metrics.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Admin only)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "userStats": {
          "totalStudents": 1420,
          "activeStudents": 1395,
          "lockedAccounts": 25
        },
        "academicOverview": {
          "systemAverageGpa10": 7.15,
          "systemAverageGpa4": 2.85,
          "totalCreditsEarned": 42500
        },
        "gpaDistribution": {
          "excellent": 112,
          "veryGood": 345,
          "good": 620,
          "average": 280,
          "belowAverage": 50,
          "fail": 13
        }
      }
    }
    ```

---

### 2.2 POST /admin/notifications
Sends a direct notification to a specific student.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Admin only)
*   **Request Body**:
    ```json
    {
      "recipientId": "33a25d2c-80a5-4089-9a2c-f60897f2c253",
      "title": "Academic Probation Review",
      "message": "Please schedule an advising review session.",
      "type": "AdvisingReview"
    }
    ```
*   **Validation Rules**:
    *   `recipientId`: Required, must be a valid GUID referencing an active student.
    *   `title`: Required, 1 to 200 characters.
    *   `message`: Required, 1 to 2000 characters.
*   **Success Response** (`201 Created`):
    ```json
    {
      "success": true,
      "message": "Direct notification sent successfully."
    }
    ```

---

### 2.3 POST /admin/notifications/broadcast
Broadcasts an announcement notification to all active students.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Admin only)
*   **Request Body**:
    ```json
    {
      "title": "System-wide Maintenance",
      "message": "The system will be offline for maintenance on Saturday from 02:00 to 04:00 AM."
    }
    ```
*   **Success Response** (`201 Created`):
    ```json
    {
      "success": true,
      "message": "Broadcast notification sent successfully."
    }
    ```

---

### 2.4 GET /admin/notifications/history
Returns a list of all broadcast and direct notifications sent by administrators.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Admin only)
*   **Query Parameters**: Standard pagination (`page`, `pageSize`).
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "id": "n12fb33f-8461-460d-85fa-7c961e67fa8c",
          "title": "System-wide Maintenance",
          "isBroadcast": true,
          "recipientName": null,
          "createdAt": "2026-06-24T10:00:00Z"
        }
      ],
      "pagination": {
        "page": 1,
        "pageSize": 20,
        "totalItems": 12,
        "totalPages": 1
      }
    }
    ```

---

*End of Document — Admin API*
