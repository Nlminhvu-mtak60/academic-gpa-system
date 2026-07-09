# 13 — Authentication Pages

> **Document ID**: UX-AUTH-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Login, sign-up, and password recovery wireframes

---

## 1. Document Purpose

This document provides page layouts and wireframes for authentication screens.

---

## 2. Authentication Wireframes

---

### 2.1 Sign-In Portal (Login Page)

```
+-------------------------------------------------------------+
|                                                      [VI/EN]|
|                      Academic GPA                           |
|                      Sign in to your account                |
|                                                             |
|  Email Address                                              |
|  [ student@gpa.app                                       ]  |
|                                                             |
|  Password                                                   |
|  [ **********                                            ]  |
|  [ ] Remember me                        [Forgot Password?]  |
|                                                             |
|  [ Sign In ]                                                |
|                                                             |
|  ====================== OR ======================           |
|                                                             |
|  [ G  Sign in with Google ]                                 |
|                                                             |
|  Don't have an account? [Create an account]                 |
|                                                             |
+-------------------------------------------------------------+
```

*   **Google Auth Interaction**: Clicking the Google button opens Google's OAuth popup.
*   **Error Display**: Invalid credentials trigger a toast notification: "Invalid email or password."

---

### 2.2 Registration Page
Contains a live password strength validation indicator.

```
+-------------------------------------------------------------+
|                      Create Your Account                    |
|                                                             |
|  Full Name                                                  |
|  [ Nguyen Van A                                          ]  |
|                                                             |
|  Email Address                                              |
|  [ student@gpa.app                                       ]  |
|                                                             |
|  Password                                                   |
|  [ **********                                            ]  |
|  Password Strength: [=====>------] Medium                   |
|  - [x] Min 8 characters   - [x] Contains numbers            |
|  - [ ] Special character  - [ ] Uppercase letter            |
|                                                             |
|  [ Sign Up ]                                                |
|                                                             |
|  Already have an account? [Sign In]                         |
|                                                             |
+-------------------------------------------------------------+
```

*   **Password Rules Validation**: Strength indicators update dynamically as the user types, enabling the "Sign Up" button only when all criteria are met.

---

### 2.3 Password Recovery (Forgot Password)

*   **View**: A clean input form requesting the account's registered email address.
*   **Action**: Clicking "Send Reset Link" updates the screen to show a success message: "If your email exists in our system, we have sent a password reset link to it." (prevents email enumeration).

---

### 2.4 Password Reset Form

*   **Access**: Accessed via reset links containing tokens (`/reset-password?token=XYZ`).
*   **Inputs**: Prompts for a "New Password" and "Confirm Password".
*   **Success Action**: Saving redirect users back to the Login Page, displaying a success toast.

---

*End of Document — Authentication Pages*
