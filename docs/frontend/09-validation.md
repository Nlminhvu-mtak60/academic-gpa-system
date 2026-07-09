# 09 — Validation Strategy

> **Document ID**: ARC-FE-VAL-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Client-side Zod validation schemas and constraints

---

## 1. Client-Side Validation with Zod

The frontend uses **Zod** to define schemas and enforce type-safe validations in the browser before data is sent to the API.

---

## 2. Validation Schemas

---

### 2.1 Login Schema (`loginSchema`)
*   `email`: Required, valid email format. Displays error: "Email không hợp lệ / Invalid email".
*   `password`: Required, minimum 8 characters.

---

### 2.2 Registration Schema (`registerSchema`)
*   `email`: Required, valid email format.
*   `password`: Required, minimum 8 characters. Must contain at least one uppercase letter, one lowercase letter, one number, and one special character.
*   `firstName` / `lastName`: Required, 1 to 50 characters, letters and spaces only.

---

### 2.3 Course Schema (`courseSchema`)
*   `courseCode`: Required, alphanumeric, 1 to 20 characters.
*   `courseName`: Required, string, 1 to 200 characters.
*   `credits`: Required, integer between 1 and 6.

---

### 2.4 Score Schema (`scoreSchema`)
*   `attendanceScore` / `continuousScore` / `finalExamScore`: Optional (allows partial grade entry). If provided, values must be decimals between `0.00` and `10.00` inclusive.

---

### 2.5 Goal Schema (`goalSchema`)
*   `targetCumulativeGpa10`: Required, decimal between `0.00` and `10.00`.
*   `notes`: Optional, maximum 500 characters.

---

## 3. Real-Time UI Validation Feedback

*   **Syntax Checking**: Validations run on the `onChange` event to provide real-time feedback as the user types.
*   **Submit Blocking**: The form submit action is disabled until all Zod schema constraints are satisfied.

---

*End of Document — Validation Strategy*
