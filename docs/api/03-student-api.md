# 03 — Student API

> **Document ID**: API-STUDENT-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Admin student directory endpoint contracts

---

## 1. Student Directory Endpoints Overview

These endpoints enable system administrators to search, monitor, lock, and manage student accounts. Under no circumstances can students access these endpoints (enforced via the `Admin` policy guard).

---

## 2. Endpoint Specifications

---

### 2.1 GET /admin/students
Returns a paginated list of students, supporting filtering by status and text searching.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Admin only)
*   **Query Parameters**:
    *   `page` (int, optional): Page index (default: 1).
    *   `pageSize` (int, optional): Items per page (default: 20, max: 100).
    *   `search` (string, optional): Searches against name, email, or StudentCode.
    *   `isActive` (bool, optional): Filter by active or locked status.
    *   `sortBy` (string, optional): Field name (e.g. `studentCode`, `lastName`).
    *   `sortOrder` (string, optional): Sort direction (`asc` or `desc`).
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "id": "33a25d2c-80a5-4089-9a2c-f60897f2c253",
          "studentCode": "2024001",
          "firstName": "Nguyen Van",
          "lastName": "A",
          "email": "a@domain.com",
          "universityName": "University of Technology",
          "majorName": "Computer Science",
          "isActive": true
        }
      ],
      "pagination": {
        "page": 1,
        "pageSize": 20,
        "totalItems": 145,
        "totalPages": 8
      }
    }
    ```

---

### 2.2 GET /admin/students/{id}
Retrieves detailed, read-only academic and profile information for a student.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Admin only)
*   **Path Parameters**:
    *   `id` (guid): The primary key user identifier.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "id": "33a25d2c-80a5-4089-9a2c-f60897f2c253",
        "email": "a@domain.com",
        "firstName": "Nguyen Van",
        "lastName": "A",
        "isActive": true,
        "studentProfile": {
          "studentCode": "2024001",
          "universityName": "University of Technology",
          "majorName": "Computer Science",
          "enrollmentYear": 2024,
          "totalRequiredCredits": 120
        }
      }
    }
    ```
*   **Error Responses**:
    *   `404 Not Found`: Student does not exist.

---

### 2.3 PUT /admin/students/{id}/lock
Locks a student account, blocking them from logging in.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Admin only)
*   **Path Parameters**:
    *   `id` (guid): Target student user ID.
*   **Request Body**:
    ```json
    {
      "lockReason": "Violation of system usage terms."
    }
    ```
*   **Validation Rules**:
    *   `lockReason`: Required, minimum 10 characters, maximum 200 characters.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Student account has been locked."
    }
    ```

---

### 2.4 PUT /admin/students/{id}/unlock
Unlocks a locked student account.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Admin only)
*   **Path Parameters**:
    *   `id` (guid): Target student user ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Student account has been unlocked."
    }
    ```

---

### 2.5 DELETE /admin/students/{id}
Soft-deletes a student profile.

*   **HTTP Method**: `DELETE`
*   **Authorization**: Bearer token (Admin only)
*   **Path Parameters**:
    *   `id` (guid): Target student user ID.
*   **Success Response** (`204 No Content`): No response body.

---

*End of Document — Student API*
