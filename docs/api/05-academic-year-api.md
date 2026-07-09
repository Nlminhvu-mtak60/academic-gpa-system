# 05 — Academic Year API

> **Document ID**: API-ACADYEAR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Academic years endpoint contracts and validation rules

---

## 1. Academic Years Endpoints Overview

These endpoints allow students to manage their academic years. All actions verify resource ownership, ensuring students can only access their own records.

---

## 2. Endpoint Specifications

---

### 2.1 GET /academic-years
Returns a list of all active academic years for the current student, including a summary of GPA and credits completed.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "id": "a62fb33f-8461-460d-85fa-7c961e67fa8b",
          "yearName": "2024-2025",
          "startYear": 2024,
          "endYear": 2025,
          "completedCredits": 30,
          "yearGpa10": 7.45,
          "yearGpa4": 3.08
        }
      ]
    }
    ```

---

### 2.2 POST /academic-years
Creates a new academic year.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Request Body**:
    ```json
    {
      "yearName": "2024-2025",
      "startYear": 2024,
      "endYear": 2025
    }
    ```
*   **Validation Rules**:
    *   `yearName`: Required, string, 1 to 20 characters. Must be unique per student.
    *   `startYear`: Required, between 2000 and 2100.
    *   `endYear`: Required, must equal `startYear` or `startYear + 1`.
*   **Success Response** (`201 Created`):
    ```json
    {
      "success": true,
      "message": "Academic year created successfully.",
      "data": {
        "id": "a62fb33f-8461-460d-85fa-7c961e67fa8b",
        "yearName": "2024-2025",
        "startYear": 2024,
        "endYear": 2025
      }
    }
    ```

---

### 2.3 PUT /academic-years/{id}
Updates academic year details.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target academic year ID.
*   **Request Body**: Same as `POST /academic-years`.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Academic year updated successfully."
    }
    ```
*   **Error Responses**:
    *   `403 Forbidden`: Attempting to edit another student's academic year.
    *   `404 Not Found`: Academic year does not exist.

---

### 2.4 DELETE /academic-years/{id}
Soft-deletes an academic year and all nested semesters and courses.

*   **HTTP Method**: `DELETE`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target academic year ID.
*   **Success Response** (`204 No Content`): No response body.
*   **Business Logic**: Marks the year and all child semesters and courses as deleted (`IsDeleted = 1`). Triggers a GPA recalculation for the student's remaining active semesters.

---

*End of Document — Academic Year API*
