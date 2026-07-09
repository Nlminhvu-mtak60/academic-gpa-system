# 08 — Security Architecture

> **Document ID**: ARC-SEC-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Security specifications and cryptographic protocols

---

## 1. Security Architecture Principles

The Academic GPA Management System enforces multi-layered security controls to protect student academic records, verify user identity, prevent data tampering, and defend against web-based attacks.

---

## 2. Authentication Security (JWT, RTR, & Google OAuth)

### 2.1 JSON Web Tokens (JWT)
*   **Signature Algorithm**: Tokens are signed using **HMAC SHA-256 (HS256)**.
*   **Secret Key Management**: Secret keys are stored as environment variables or injected from a secure vault (such as Azure Key Vault). They must never be hardcoded in application config files.
*   **Token Expiry**: Kept strictly short-lived at **15 minutes**.
*   **Token Validation**: The API validates signature key integrity, issuer, audience, and expiration on every request.

### 2.2 Refresh Token Rotation (RTR)
*   **Cookie Storage**: Refresh tokens are stored in the client's browser using `HttpOnly`, `Secure`, and `SameSite=Strict` flags. This prevents malicious scripts (XSS) from reading the tokens.
*   **Rotation Execution**:
    *   When a token is used to refresh a session, it is immediately revoked, and a new one is issued.
    *   **Breach Detection**: If a revoked refresh token is presented, the system assumes a token theft attempt has occurred, revokes all active refresh tokens in that token family, and prompts the user to re-authenticate.

### 2.3 Google OAuth 2.0 Integration
*   **OAuth Flow**: The frontend SPA initiates the authorization request and receives an authorization code.
*   **Verification**: The backend exchanges the code for Google user profile details via Google's token endpoint.
*   **Registration**: If the email matches an existing user, the account is linked; otherwise, a new verified student account is created automatically.

---

## 3. Authorization & Role-Based Access Control (RBAC)

The system uses role-based claims. Controllers check access permissions before executing actions:

*   **Student Claims**: Can perform CRUD operations only on their own academic records.
*   **Admin Claims**: Can search all student profiles in read-only mode, lock/unlock accounts, and send broadcasts. Admins cannot modify student scores or view decrypted passwords.
*   **Resource Ownership Verification**: Every request handling student data must verify that the target resource belongs to the current user claim:
    ```csharp
    // Business logic check inside Handlers:
    if (course.Semester.AcademicYear.StudentId != currentUser.Id)
    {
        throw new ForbiddenException("Access denied to another student's course record.");
    }
    ```

---

## 4. Web Application Defense

### 4.1 SQL Injection Prevention
*   **ORM Boundaries**: By using **Entity Framework Core 9**, all database queries are parameterized by default. 
*   **Raw SQL Avoidance**: Developers must avoid using raw SQL execution methods (`FromSqlRaw`, `ExecuteSqlRaw`) with string concatenation. If raw SQL is required, parameter placeholder objects must be used instead.

### 4.2 XSS (Cross-Site Scripting) Prevention
*   **Input Sanitization**: All text inputs (e.g. course names, profile universities) are sanitized before storage.
*   **Content Security Policy (CSP)**: The API Gateway injects headers to prevent unauthorized script execution:
    ```
    Content-Security-Policy: default-src 'self'; script-src 'self'; object-src 'none';
    ```
*   **X-Frame-Options**: Set to `DENY` to prevent clickjacking attacks.

### 4.3 CSRF (Cross-Site Request Forgery) Strategy
*   **Bearer Auth Protections**: Since JWT access tokens are transmitted via the `Authorization: Bearer <Token>` HTTP header (not via browser cookies), the primary API is naturally protected against CSRF.
*   **Refresh Token Protection**: The refresh token, which is stored in a cookie, is protected using the `SameSite=Strict` flag. This ensures the browser will not send the cookie during cross-site requests.

### 4.4 Rate Limiting
*   **Auth Endpoints**: Rejects requests after 5 failed login attempts per IP within a 15-minute window.
*   **Global API Limit**: Limits general requests to 100 per minute per IP.
*   **AI Chat Endpoints**: Enforces a strict rate limit of 20 queries per hour per student account.

---

## 5. Audit Logging & Score Integrity

*   **Audit Logging**: Every create, edit, or delete action on a course score is logged in the `ScoreAuditLog` table.
*   **Log Contents**: The log stores the Score ID, the name of the modified field, the old value, the new value, and a timestamp. 
*   **Tamper Prevention**: The audit log table is insert-only; updates and deletions are blocked by database constraints.

---

*End of Document — Security Architecture*
