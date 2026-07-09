# Security Audit & Verification Report (Phase 10)

This document contains the security evaluation, threat analysis, and automated/manual verification results for the Academic GPA Management System.

---

## 1. Security Architecture & Controls

The system implements a multi-layered security strategy following industry best practices:

### Authentication & JWT Security
- **Algorithm**: HMAC SHA-256 for token signature verification.
- **Payload Verification**: Access tokens are verified for Signature, Issuer, Audience, and Lifetime.
- **Expiration Policy**:
  - Access Token: 15 minutes (short-lived to minimize hijack window).
  - Refresh Token: 7 days, cryptographically random, stored securely in the database, with IP address tracking.
- **Verification**: Covered by `JwtServiceTests.cs` and `AuthTests.cs`.

### Cryptography & Password Hashing
- **Hashing Algorithm**: BCrypt (via `BCrypt.Net-Next`) is used to salt and hash all passwords.
- **Salt Randomness**: Generates distinct hashes for the same password input across multiple operations.
- **Verification**: Covered by `PasswordHasherTests.cs`.

### Input Sanitization & Parameterization
- **SQL Injection**: All data persistence is performed via Entity Framework Core using parameterized LINQ queries, which prevents SQL Injection by design.
- **XSS (Cross-Site Scripting)**:
  - Input: Alphanumeric characters only are enforced on structural fields such as `StudentCode` (via `UpdateStudentProfileCommandValidator`).
  - Output: React automatically escapes HTML content rendered in JSX, preventing arbitrary script injection.
- **Verification**: Verified in `InputValidationSecurityTests.cs`.

---

## 2. Threat Analysis Checklist

| Threat Vector | Mitigating Controls | Status |
|---|---|---|
| **SQL Injection (SQLi)** | Parameterized queries via EF Core LINQ. Raw SQL queries are avoided. | **Verified (Safe)** |
| **Cross-Site Scripting (XSS)** | React automatic sanitization + alphanumeric input validation constraints on critical fields. | **Verified (Safe)** |
| **Unauthorized Endpoint Access** | MediatR pipeline authenticates current user context via `ICurrentUserService`. | **Verified (Safe)** |
| **Broken Object-Level Authorization** | Thread/academic items verify that the requested resource `UserId` / `StudentProfileId` matches the authenticated user context. | **Verified (Safe)** |
| **Rate Limiting abuse** | Enforced 20 messages per hour on AI chatbot endpoint. | **Verified (Safe)** |
| **Mass Assignment** | DTO structures explicitly map incoming payload to domain models instead of mapping raw HTTP bodies. | **Verified (Safe)** |
| **JWT Brute-force / Weak Keys** | Configuration enforces minimum key size constraints. | **Verified (Safe)** |

---

## 3. Security Test Scenarios

### SQL Injection Attempt in Major/University Name
- **Payload**: `'; DROP TABLE Users; --`
- **Result**: Successfully saved as literal text in in-memory DB context. No command execution occurred.

### Cross-Site Scripting Attempt in Major/University Name
- **Payload**: `<script>alert('xss')</script>`
- **Result**: Saved as literal text. Evaluated by browser rendering (React JSX string conversion) safely as text instead of HTML markup.

### Alphanumeric Student Code Enforcement
- **Payload**: `STU_123`
- **Result**: Validation failed under `UpdateStudentProfileCommandValidator` with alphanumeric mismatch constraint.
