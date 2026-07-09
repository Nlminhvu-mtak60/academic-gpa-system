# 06 — Semester API

> **Document ID**: API-SEMESTER-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Semesters endpoint contracts and business validation rules

---

## 1. Semester Endpoints Overview

These endpoints allow students to manage semesters within an academic year. 

---

## 2. Endpoint Specifications

---

### 2.1 GET /academic-years/{yearId}/semesters
Lists all active semesters inside a specific academic year.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `yearId` (guid): Target academic year ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "id": "e32fb33f-8461-460d-85fa-7c961e67fa8b",
          "semesterName": "Semester 1",
          "sortOrder": 1,
          "completedCredits": 12,
          "semesterGpa10": 8.02,
          "semesterGpa4": 3.45
        }
      ]
    }
    ```

---

### 2.2 POST /academic-years/{yearId}/semesters
Creates a new semester inside an academic year.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `yearId` (guid): Target academic year ID.
*   **Request Body**:
    ```json
    {
      "semesterName": "Semester 1"
    }
    ```
*   **Validation Rules**:
    *   `semesterName`: Required, string, 1 to 50 characters. Must be unique within the academic year.
*   **Business Rules**:
    *   **Maximum Semesters**: A student can have a maximum of 3 semesters per academic year. If this limit is exceeded, the server returns a `422 Unprocessable Entity` response with the message: "Maximum of 3 semesters per academic year."
*   **Success Response** (`201 Created`):
    ```json
    {
      "success": true,
      "message": "Semester created successfully.",
      "data": {
        "id": "e32fb33f-8461-460d-85fa-7c961e67fa8b",
        "semesterName": "Semester 1",
        "sortOrder": 1
      }
    }
    ```

---

### 2.3 PUT /semesters/{id}
Updates the name of a semester.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target semester ID.
*   **Request Body**: Same as `POST /academic-years/{yearId}/semesters`.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Semester updated successfully."
    }
    ```

---

### 2.4 DELETE /semesters/{id}
Soft-deletes a semester and all nested courses.

*   **HTTP Method**: `DELETE`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target semester ID.
*   **Success Response** (`204 No Content`): No response body.
*   **Business Logic**: Marks the semester and all child courses as deleted (`IsDeleted = 1`). Triggers a GPA recalculation for the student's remaining active semesters.

---

*End of Document — Semester API*
