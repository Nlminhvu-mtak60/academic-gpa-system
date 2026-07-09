# 14 — Notification API

> **Document ID**: API-NOTIF-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Notifications endpoint contracts and parameter rules

---

## 1. Notification Endpoints Overview

These endpoints handle delivery, status tracking, and read receipts for student system notifications and admin broadcasts.

---

## 2. Endpoint Specifications

---

### 2.1 GET /notifications
Returns a paginated list of notifications for the authenticated student.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Query Parameters**:
    *   `page` (int, optional): Page index (default: 1).
    *   `pageSize` (int, optional): Items per page (default: 20, max: 100).
    *   `unreadOnly` (bool, optional): If `true`, returns only unread items.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "id": "n12fb33f-8461-460d-85fa-7c961e67fa8b",
          "title": "AI System Upgrade Scheduled",
          "message": "We will perform scheduled updates on LLM...",
          "type": "System",
          "isRead": false,
          "createdAt": "2026-06-24T10:00:00Z"
        }
      ],
      "pagination": {
        "page": 1,
        "pageSize": 20,
        "totalItems": 15,
        "totalPages": 1
      }
    }
    ```

---

### 2.2 PUT /notifications/{id}/read
Marks a single notification as read.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target notification ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Notification marked as read."
    }
    ```
*   **Error Responses**:
    *   `404 Not Found`: Notification does not exist or belongs to another user.

---

### 2.3 PUT /notifications/read-all
Marks all unread notifications for the student as read.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "All notifications marked as read."
    }
    ```

---

### 2.4 GET /notifications/unread-count
Returns the count of unread notifications for the current student.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "unreadCount": 2
      }
    }
    ```

---

*End of Document — Notification API*
