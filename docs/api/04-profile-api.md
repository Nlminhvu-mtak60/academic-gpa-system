# 04 — Profile API

> **Document ID**: API-PROFILE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Student profile settings and preferences endpoint contracts

---

## 1. Profile Endpoints Overview

These endpoints allow students to view and update their profile details, customize layout preferences, and upload avatars.

---

## 2. Endpoint Specifications

---

### 2.1 GET /students/profile
Retrieves the profile details of the currently authenticated student.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "email": "student@gpa.app",
        "firstName": "Nguyen Van",
        "lastName": "A",
        "avatarUrl": "https://cdn.gpa.app/avatars/uuid-avatar.png",
        "preferredLanguage": "vi",
        "preferredTheme": "light",
        "profile": {
          "studentCode": "2024001",
          "universityName": "University of Technology",
          "majorName": "Computer Science",
          "enrollmentYear": 2024,
          "totalRequiredCredits": 120
        }
      }
    }
    ```

---

### 2.2 PUT /students/profile
Updates the student's profile details.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Student role)
*   **Request Body**:
    ```json
    {
      "studentCode": "2024001",
      "universityName": "University of Technology",
      "majorName": "Computer Science",
      "enrollmentYear": 2024,
      "totalRequiredCredits": 120
    }
    ```
*   **Validation Rules**:
    *   `studentCode`: Required, alphanumeric, 1 to 50 characters. Must be unique.
    *   `universityName` / `majorName`: Required, 1 to 200 characters.
    *   `enrollmentYear`: Required, between 2000 and 2100.
    *   `totalRequiredCredits`: Required, between 30 and 300.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Profile updated successfully.",
      "data": {
        "studentCode": "2024001",
        "universityName": "University of Technology",
        "majorName": "Computer Science",
        "enrollmentYear": 2024,
        "totalRequiredCredits": 120
      }
    }
    ```

---

### 2.3 POST /students/profile/avatar
Uploads a custom profile picture (avatar).

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Request Body**: `multipart/form-data`
    *   `file`: Binary file upload (image format: PNG, JPG, or WebP. Max size: 2MB).
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Avatar uploaded successfully.",
      "data": {
        "avatarUrl": "https://cdn.gpa.app/avatars/uuid-avatar.png"
      }
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: File format or size validation failure.

---

### 2.4 PUT /students/preferences
Updates the user's interface preferences (theme and language).

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Any authenticated user)
*   **Request Body**:
    ```json
    {
      "preferredLanguage": "vi",
      "preferredTheme": "dark"
    }
    ```
*   **Validation Rules**:
    *   `preferredLanguage`: Required, must be `'vi'` or `'en'`.
    *   `preferredTheme`: Required, must be `'light'` or `'dark'`.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Preferences updated successfully."
    }
    ```

---

*End of Document — Profile API*
