# 07 — Course API

> **Document ID**: API-COURSE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Courses endpoint contracts and validation rules

---

## 1. Course Endpoints Overview

These endpoints allow students to manage courses within a semester. All actions verify resource ownership to ensure students can only modify their own academic records.

---

## 2. Endpoint Specifications

---

### 2.1 GET /semesters/{semesterId}/courses
Lists all active courses inside a specific semester.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `semesterId` (guid): Target semester ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "id": "c62fb33f-8461-460d-85fa-7c961e67fa8b",
          "courseCode": "CS101",
          "courseName": "Intro to Computer Science",
          "credits": 3,
          "isRetake": false,
          "originalCourseId": null
        }
      ]
    }
    ```

---

### 2.2 POST /semesters/{semesterId}/courses
Adds a new course to a semester.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `semesterId` (guid): Target semester ID.
*   **Request Body**:
    ```json
    {
      "courseCode": "CS101",
      "courseName": "Intro to Computer Science",
      "credits": 3,
      "isRetake": false,
      "originalCourseId": null
    }
    ```
*   **Validation Rules**:
    *   `courseCode`: Required, alphanumeric, 1 to 20 characters.
    *   `courseName`: Required, string, 1 to 200 characters.
    *   `credits`: Required, integer between 1 and 6.
    *   `isRetake`: Required, boolean value.
    *   `originalCourseId`: Required if `isRetake` is `true`. Must be a valid GUID referencing an existing course.
*   **Success Response** (`201 Created`):
    ```json
    {
      "success": true,
      "message": "Course added successfully.",
      "data": {
        "id": "c62fb33f-8461-460d-85fa-7c961e67fa8b",
        "courseCode": "CS101",
        "courseName": "Intro to Computer Science",
        "credits": 3
      }
    }
    ```

---

### 2.3 PUT /courses/{id}
Updates course details.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target course ID.
*   **Request Body**: Same as `POST /semesters/{semesterId}/courses`.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Course updated successfully."
    }
    ```

---

### 2.4 DELETE /courses/{id}
Soft-deletes a course.

*   **HTTP Method**: `DELETE`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target course ID.
*   **Success Response** (`204 No Content`): No response body.
*   **Business Logic**: Marks the course and its linked score record as deleted (`IsDeleted = 1`). Triggers a GPA recalculation.

---

*End of Document — Course API*
