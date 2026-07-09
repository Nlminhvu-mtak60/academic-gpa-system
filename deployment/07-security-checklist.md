# 07 - Security Architecture Checklist (Phase 11)

This checklist outlines the required security policies, network configurations, and database permissions for hosting the Academic GPA Management System in a production environment.

---

## 1. Network & Protocol Configurations

- [ ] **Enforce TLS/HTTPS**:
  - Configure Nginx reverse proxy or cloud balancer with valid SSL certificates.
  - Disable HTTP or set up automatic HTTP redirection to HTTPS (port `80` -> port `443`).
- [ ] **Port Level Access Policies**:
  - Block external access to ports `1433` (SQL Server) and `8000` (FastAPI).
  - Open only port `80` / `443` for public web traffic.

---

## 2. Authentication & JWT Security

- [ ] **Symmetric Keys length constraints**:
  - Verify `Jwt:Secret` contains at least 256 bits (32+ alphanumeric characters).
- [ ] **JWT Lifetime Verification**:
  - Verify access token lifespan is set to 15 minutes.
  - Verify refresh token lifespan is set to 7 days.
- [ ] **Secure Refresh Token Cookies**:
  - Verify refresh tokens are transmitted via secure, HttpOnly, SameSite cookies.

---

## 3. Application Security Controls

- [ ] **CORS Configuration**:
  - Disable wildcard mappings (`*`) in production.
  - Update allowed origins in `appsettings.Production.json` to include only the official domain URL.
- [ ] **Rate Limiting Enforcement**:
  - Verify the 20 messages per hour chat rate limit is active.
- [ ] **Secrets Exclusions**:
  - Verify no production passwords, API keys, or JWT tokens are committed to repository git logs.
