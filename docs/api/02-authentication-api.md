# 02 — Authentication API

> **Document ID**: API-AUTH-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Authentication endpoint contracts and payload formats

---

## 1. Authentication Endpoints Overview

This document specifies the authentication API contract, managing user registration, standard login, Google OAuth federation, token refreshing, and password recovery.

---

## 2. Endpoint Specifications

---

### 2.1 POST /auth/register
Registers a new user in the system with the Student role.

*   **HTTP Method**: `POST`
*   **Authentication**: None (Anonymous)
*   **Request Body**:
    ```json
    {
      "email": "student@gpa.app",
      "password": "Password@123",
      "firstName": "Nguyen Van",
      "lastName": "A"
    }
    ```
*   **Validation Rules**:
    *   `email`: Required, valid format, max 100 characters. Must be unique.
    *   `password`: Required, min 8 characters, must contain at least 1 uppercase, 1 lowercase, 1 digit, and 1 special character.
    *   `firstName` / `lastName`: Required, 1 to 50 characters, letters and spaces only.
*   **Success Response** (`201 Created`):
    ```json
    {
      "success": true,
      "message": "Registration successful. Please check your email to verify your account."
    }
    ```
*   **Error Responses**:
    *   `400 Bad Request`: Validation failure.
    *   `409 Conflict`: Email already registered.

---

### 2.2 POST /auth/login
Authenticates a user and issues a short-lived access token and a refresh token.

*   **HTTP Method**: `POST`
*   **Authentication**: None (Anonymous)
*   **Request Body**:
    ```json
    {
      "email": "student@gpa.app",
      "password": "Password@123"
    }
    ```
*   **Success Response** (`200 OK`):
    *   **Response Body**:
        ```json
        {
          "success": true,
          "message": "Login successful.",
          "data": {
            "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            "expiresInSeconds": 900,
            "role": "Student"
          }
        }
        ```
    *   **Response Headers**:
        *   `Set-Cookie`: `refreshToken=uuid-token; HttpOnly; Secure; SameSite=Strict; Path=/api/v1/auth; Max-Age=604800`
*   **Error Responses**:
    *   `400 Bad Request`: Input syntax error.
    *   `401 Unauthorized`: Invalid credentials or unverified email address.

---

### 2.3 POST /auth/google
Authenticates a user via Google OAuth 2.0.

*   **HTTP Method**: `POST`
*   **Request Body**:
    ```json
    {
      "authCode": "4/0AdQt8qg..."
    }
    ```
*   **Business Logic**: The backend exchanges `authCode` with Google's API to fetch user profile details. If the user does not exist, a new account is registered and verified automatically.
*   **Success Response** (`200 OK`): Same as `POST /auth/login` (returns a JWT access token and sets a secure HttpOnly refresh token cookie).

---

### 2.4 POST /auth/refresh-token
Rotates the user's refresh token and issues a new access token.

*   **HTTP Method**: `POST`
*   **Cookie Payload**: Reads the `refreshToken` cookie.
*   **Success Response** (`200 OK`):
    *   **Response Body**: Same as `POST /auth/login` (new access token).
    *   **Cookie**: Resets `refreshToken` with a rotated token.
*   **Error Responses**:
    *   `401 Unauthorized`: Token expired or invalid. If a token reuse attack is detected, the entire session family is revoked.

---

### 2.5 POST /auth/forgot-password
Initiates the password reset flow.

*   **HTTP Method**: `POST`
*   **Request Body**:
    ```json
    {
      "email": "student@gpa.app"
    }
    ```
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "If your email is registered in our system, we have sent a reset link to it."
    }
    ```

---

### 2.6 POST /auth/reset-password
Resets the password using a reset token.

*   **HTTP Method**: `POST`
*   **Request Body**:
    ```json
    {
      "token": "reset-uuid-token",
      "newPassword": "NewPassword@123"
    }
    ```
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "Password reset successfully. You can now login with your new password."
    }
    ```

---

*End of Document — Authentication API*
