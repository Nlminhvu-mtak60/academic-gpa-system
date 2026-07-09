# 03 — Non-Functional Requirements

> **Document ID**: SRS-NFR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review

---

## 1. Document Purpose

This document specifies all non-functional requirements (NFRs) for the Academic GPA Management System. NFRs define the quality attributes, constraints, and operational characteristics the system must satisfy beyond its core functionality.

---

## 2. NFR Categories Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    NON-FUNCTIONAL REQUIREMENTS                  │
├──────────────┬──────────────┬──────────────┬────────────────────┤
│ Performance  │  Security    │ Usability    │ Reliability        │
│ Scalability  │  Privacy     │ Accessibility│ Maintainability    │
│ Availability │  Compliance  │ Localization │ Portability        │
│ Monitoring   │  Auditability│ Compatibility│ Testability        │
└──────────────┴──────────────┴──────────────┴────────────────────┘
```

---

## 3. Performance Requirements

### NFR-PERF-001: API Response Time

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-PERF-001 |
| **Category** | Performance |
| **Description** | The system shall respond to API requests within defined latency targets under normal load. |
| **Criteria** | |

| Operation Type | P50 Latency | P95 Latency | P99 Latency |
|----------------|-------------|-------------|-------------|
| Simple CRUD (read) | ≤ 50ms | ≤ 100ms | ≤ 200ms |
| Simple CRUD (write) | ≤ 100ms | ≤ 200ms | ≤ 400ms |
| GPA Calculation | ≤ 100ms | ≤ 300ms | ≤ 500ms |
| Statistics/Analytics | ≤ 200ms | ≤ 500ms | ≤ 1000ms |
| AI Advisor (LLM) | ≤ 2000ms | ≤ 5000ms | ≤ 10000ms |
| File Upload (Avatar) | ≤ 500ms | ≤ 1500ms | ≤ 3000ms |
| Authentication | ≤ 200ms | ≤ 500ms | ≤ 800ms |

| **Measurement** | Measured at the API gateway level, excluding network transit to client. |
| **Test Method** | Load test with k6 or Apache JMeter under 100 concurrent users. |

### NFR-PERF-002: Frontend Performance

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-PERF-002 |
| **Category** | Performance |
| **Description** | The frontend shall meet web performance standards for user experience. |
| **Criteria** | |

| Metric | Target | Tool |
|--------|--------|------|
| First Contentful Paint (FCP) | ≤ 1.5s | Lighthouse |
| Largest Contentful Paint (LCP) | ≤ 2.5s | Lighthouse |
| Cumulative Layout Shift (CLS) | ≤ 0.1 | Lighthouse |
| First Input Delay (FID) | ≤ 100ms | Lighthouse |
| Time to Interactive (TTI) | ≤ 3.0s | Lighthouse |
| Lighthouse Performance Score | ≥ 85 | Lighthouse |
| Bundle Size (initial) | ≤ 300 KB gzipped | Webpack/Vite |

| **Conditions** | Measured on 4G connection simulation (1.6 Mbps, 150ms RTT) |

### NFR-PERF-003: Database Query Performance

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-PERF-003 |
| **Category** | Performance |
| **Description** | Database queries shall execute within defined time limits. |
| **Criteria** | Simple queries: ≤ 10ms. Join queries (2–3 tables): ≤ 50ms. Complex aggregation: ≤ 200ms. No query shall perform a full table scan on tables with > 10,000 rows. |
| **Enforcement** | All queries reviewed for execution plans. Critical indexes documented and maintained. |

### NFR-PERF-004: Concurrent Request Handling

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-PERF-004 |
| **Category** | Performance / Scalability |
| **Description** | The system shall handle concurrent requests without degradation. |
| **Criteria** | Support 200 concurrent users with no latency degradation. Support 500 concurrent users with ≤ 20% latency increase. Support 1000 concurrent users without service failure (graceful degradation). |

---

## 4. Security Requirements

### NFR-SEC-001: Authentication Security

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-001 |
| **Category** | Security |
| **Description** | The system shall implement secure authentication mechanisms. |
| **Criteria** | |

| Control | Specification |
|---------|---------------|
| Password Hashing | bcrypt with cost factor ≥ 12 |
| JWT Signing | HMAC-SHA256 with 256-bit secret (minimum) |
| Access Token Expiry | 15 minutes |
| Refresh Token Expiry | 7 days |
| Refresh Token Rotation | New token on each refresh; old token revoked |
| Token Reuse Detection | If revoked token is reused, entire token family is revoked |
| Login Throttling | Max 5 failed attempts per 15 minutes per IP/email |
| Account Lockout | Auto-lock after 10 failed attempts in 1 hour |

### NFR-SEC-002: Password Policy

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-002 |
| **Category** | Security |
| **Description** | The system shall enforce a strong password policy. |
| **Criteria** | Minimum 8 characters. At least 1 uppercase letter (A–Z). At least 1 lowercase letter (a–z). At least 1 digit (0–9). At least 1 special character (!@#$%^&*). Cannot be the same as previous 3 passwords. Cannot contain email or name. |

### NFR-SEC-003: Transport Security

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-003 |
| **Category** | Security |
| **Description** | All communications shall be encrypted in transit. |
| **Criteria** | HTTPS (TLS 1.2+) required for all endpoints. HTTP Strict Transport Security (HSTS) header enabled. No mixed content allowed. Internal service-to-service communication (API ↔ AI) over HTTPS or within Docker network. |

### NFR-SEC-004: Input Validation & Injection Prevention

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-004 |
| **Category** | Security |
| **Description** | The system shall validate all inputs and prevent injection attacks. |
| **Criteria** | All user inputs validated server-side (type, length, format, range). Parameterized queries via EF Core (SQL injection prevention). HTML output encoding (XSS prevention). Content Security Policy (CSP) headers. No dynamic SQL construction. File upload validation (type, size, magic bytes). |

### NFR-SEC-005: Authorization & Access Control

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-005 |
| **Category** | Security |
| **Description** | The system shall enforce role-based access control (RBAC) on all protected resources. |
| **Criteria** | Two roles: Student, Admin. Students can only access their own data. Admins can view (not edit) student academic data. Every API endpoint has explicit authorization requirements. Horizontal privilege escalation prevented (student A cannot access student B's data). |

### NFR-SEC-006: CORS Policy

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-006 |
| **Category** | Security |
| **Description** | The system shall implement a restrictive CORS policy. |
| **Criteria** | Only allow requests from the frontend domain. Explicit allowed methods (GET, POST, PUT, DELETE). Explicit allowed headers. Credentials included. No wildcard origins in production. |

### NFR-SEC-007: Rate Limiting

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-007 |
| **Category** | Security |
| **Description** | The system shall rate-limit API requests to prevent abuse. |
| **Criteria** | |

| Endpoint Category | Rate Limit |
|-------------------|------------|
| Authentication endpoints | 5 requests / minute / IP |
| General API (authenticated) | 100 requests / minute / user |
| AI Advisor messages | 20 messages / hour / user |
| File upload | 5 uploads / hour / user |
| Forgot password | 3 requests / hour / email |

### NFR-SEC-008: Data Privacy

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-008 |
| **Category** | Security / Privacy |
| **Description** | The system shall protect personal data. |
| **Criteria** | Passwords never stored in plaintext. Passwords never returned in API responses. Personal data (email, name) never logged in application logs. AI service receives only anonymized academic data (no email, name, student code). Shared transcript links use unguessable tokens (UUID v4). Soft-deleted data retained but not accessible via API. |

### NFR-SEC-009: Security Headers

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SEC-009 |
| **Category** | Security |
| **Description** | The system shall send security-related HTTP headers on all responses. |
| **Headers** | |

| Header | Value |
|--------|-------|
| `X-Content-Type-Options` | `nosniff` |
| `X-Frame-Options` | `DENY` |
| `X-XSS-Protection` | `1; mode=block` |
| `Strict-Transport-Security` | `max-age=31536000; includeSubDomains` |
| `Content-Security-Policy` | Restrictive CSP policy |
| `Referrer-Policy` | `strict-origin-when-cross-origin` |
| `Permissions-Policy` | Disable camera, microphone, geolocation |

---

## 5. Reliability & Availability Requirements

### NFR-REL-001: System Availability

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-REL-001 |
| **Category** | Availability |
| **Description** | The system shall maintain high availability. |
| **Criteria** | Target uptime: 99.5% (approximately 1.8 days of downtime per year). Planned maintenance windows: weekends, 2:00–6:00 AM (UTC+7). Maintenance announcements: 48 hours in advance. |

### NFR-REL-002: Error Handling

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-REL-002 |
| **Category** | Reliability |
| **Description** | The system shall handle errors gracefully without exposing internal details. |
| **Criteria** | All exceptions caught by global exception handler. Error responses follow RFC 7807 (ProblemDetails) format. Internal error details (stack traces, SQL errors) never exposed in production. User-friendly error messages in both Vietnamese and English. Frontend displays appropriate error state UI for all failure scenarios. |

### NFR-REL-003: Data Integrity

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-REL-003 |
| **Category** | Reliability |
| **Description** | The system shall maintain data integrity at all times. |
| **Criteria** | Database transactions for multi-step operations (e.g., cascade deletes, GPA recalculation). Foreign key constraints enforced at database level. Unique constraints on email, student code, share tokens. Optimistic concurrency on score updates. No orphaned records. |

### NFR-REL-004: Backup & Recovery

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-REL-004 |
| **Category** | Reliability |
| **Description** | The system shall implement data backup and recovery procedures. |
| **Criteria** | Daily automated database backups. Backup retention: 30 days. Recovery Point Objective (RPO): ≤ 24 hours. Recovery Time Objective (RTO): ≤ 4 hours. Backup restoration tested quarterly. |

### NFR-REL-005: Graceful Degradation

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-REL-005 |
| **Category** | Reliability |
| **Description** | The system shall degrade gracefully when dependent services fail. |
| **Criteria** | If AI service is unavailable: display "AI Advisor is temporarily unavailable" (do not crash). If email service is unavailable: queue emails for retry, do not block registration. If database is slow: implement circuit breaker pattern. Frontend displays appropriate fallback UI. |

---

## 6. Scalability Requirements

### NFR-SCALE-001: Horizontal Scalability

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SCALE-001 |
| **Category** | Scalability |
| **Description** | The system architecture shall support horizontal scaling. |
| **Criteria** | Stateless API servers (no in-memory session state). JWT-based authentication (no server-side session). Database connection pooling. AI service independently scalable. Frontend served via CDN (static assets). |

### NFR-SCALE-002: Data Growth

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-SCALE-002 |
| **Category** | Scalability |
| **Description** | The system shall handle projected data growth without performance degradation. |
| **Projections** | |

| Entity | Year 1 Estimate | Year 3 Estimate |
|--------|----------------|-----------------|
| Users | 5,000 | 50,000 |
| Courses | 50,000 | 500,000 |
| Scores | 50,000 | 500,000 |
| AI Messages | 100,000 | 2,000,000 |
| Notifications | 50,000 | 1,000,000 |

| **Mitigation** | Proper indexing, query optimization, pagination, archival strategy for old AI messages |

---

## 7. Usability Requirements

### NFR-USE-001: Learnability

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-USE-001 |
| **Category** | Usability |
| **Description** | New users shall be able to complete core tasks without training. |
| **Criteria** | A new user shall be able to register, create a semester, add a course, and input scores within 5 minutes without assistance. Navigation is self-explanatory with consistent labeling. Error messages are actionable (tell the user what to do). Tooltips provided for domain-specific terms (GPA-4, classification). |

### NFR-USE-002: Responsiveness

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-USE-002 |
| **Category** | Usability |
| **Description** | The application shall be usable across all common device sizes. |
| **Breakpoints** | |

| Device | Width | Layout |
|--------|-------|--------|
| Mobile | < 640px | Single column, bottom navigation |
| Tablet | 640–1024px | Collapsed sidebar, 2-column content |
| Desktop | > 1024px | Full sidebar + main content area |

| **Criteria** | All features accessible on all breakpoints. No horizontal scrolling. Touch-friendly targets (≥ 44×44 px). Forms optimized for mobile input. |

### NFR-USE-003: Feedback & Loading States

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-USE-003 |
| **Category** | Usability |
| **Description** | The system shall provide immediate visual feedback for all user actions. |
| **Criteria** | Loading spinners for async operations. Skeleton screens for initial page loads. Success/error toast notifications for CRUD operations. Disabled state for buttons during form submission. Progress indicator for file uploads. Confirmation dialogs for destructive actions (delete). Optimistic UI updates where appropriate. |

### NFR-USE-004: Form Usability

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-USE-004 |
| **Category** | Usability |
| **Description** | Forms shall follow UX best practices. |
| **Criteria** | Real-time validation feedback (on blur or change). Clear error messages positioned near the relevant field. Auto-focus on first field when form opens. Tab order follows logical sequence. Enter key submits forms. Required fields clearly marked. |

---

## 8. Accessibility Requirements

### NFR-ACC-001: WCAG Compliance

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-ACC-001 |
| **Category** | Accessibility |
| **Description** | The application shall conform to WCAG 2.1 Level AA guidelines. |
| **Key Criteria** | Color contrast ratio ≥ 4.5:1 for normal text, ≥ 3:1 for large text. All images have alt text. All form inputs have associated labels. Keyboard navigation for all interactive elements. Focus indicators visible on all focusable elements. Screen reader compatibility (semantic HTML, ARIA attributes). No content relies solely on color to convey information. |

### NFR-ACC-002: Keyboard Navigation

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-ACC-002 |
| **Category** | Accessibility |
| **Description** | All functionality shall be operable via keyboard. |
| **Criteria** | Tab order follows visual layout. Modal dialogs trap focus. Escape closes modals and dropdowns. Enter activates buttons and links. Arrow keys navigate within components (tabs, menus). Skip-to-content link for screen readers. |

---

## 9. Localization Requirements

### NFR-L10N-001: Multi-Language Support

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-L10N-001 |
| **Category** | Localization |
| **Description** | The system shall support Vietnamese and English languages. |
| **Criteria** | All UI text externalized in translation files (JSON). No hardcoded strings in components. Translation coverage: 100% of user-facing text. Language switch is instantaneous (no page reload). Right-to-left (RTL) not required (Vietnamese and English are both LTR). |

### NFR-L10N-002: Locale-Aware Formatting

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-L10N-002 |
| **Category** | Localization |
| **Description** | The system shall format data according to the selected locale. |
| **Criteria** | |

| Data Type | Vietnamese (vi-VN) | English (en-US) |
|-----------|--------------------|-----------------|
| Decimal separator | , (comma) | . (period) |
| Thousands separator | . (period) | , (comma) |
| Date format | dd/MM/yyyy | MM/dd/yyyy |
| Time format | HH:mm | h:mm a |
| GPA display | 8,50 | 8.50 |

---

## 10. Compatibility Requirements

### NFR-COMPAT-001: Browser Support

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-COMPAT-001 |
| **Category** | Compatibility |
| **Description** | The application shall function correctly on supported browsers. |
| **Supported Browsers** | |

| Browser | Minimum Version | Priority |
|---------|----------------|----------|
| Google Chrome | Last 2 major versions | P0 |
| Mozilla Firefox | Last 2 major versions | P0 |
| Microsoft Edge | Last 2 major versions | P0 |
| Apple Safari | Last 2 major versions | P1 |
| Samsung Internet | Last 2 major versions | P2 |

| **Not Supported** | Internet Explorer (any version) |

### NFR-COMPAT-002: Operating System Support

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-COMPAT-002 |
| **Category** | Compatibility |
| **Description** | The application shall function on major operating systems via supported browsers. |
| **Criteria** | Windows 10+, macOS 12+, iOS 15+, Android 10+, Linux (Chrome/Firefox) |

---

## 11. Maintainability Requirements

### NFR-MAINT-001: Code Quality Standards

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-MAINT-001 |
| **Category** | Maintainability |
| **Description** | The codebase shall adhere to defined quality standards. |
| **Criteria** | SOLID principles applied throughout. Clean Architecture separation enforced. Dependency Injection for all services. No circular dependencies between layers. Method complexity: Cyclomatic complexity ≤ 10 per method. File length: ≤ 400 lines per file (guideline). Naming conventions: C# — PascalCase for public members, camelCase for private. TypeScript — camelCase for variables, PascalCase for components/types. |

### NFR-MAINT-002: Test Coverage

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-MAINT-002 |
| **Category** | Maintainability / Testability |
| **Description** | The system shall maintain minimum test coverage thresholds. |
| **Criteria** | |

| Layer | Coverage Target | Test Type |
|-------|----------------|-----------|
| Domain (business logic) | ≥ 95% | Unit tests |
| Application (services) | ≥ 85% | Unit + integration tests |
| Infrastructure | ≥ 70% | Integration tests |
| API Controllers | ≥ 80% | Integration tests |
| Frontend Components | ≥ 70% | Component tests |
| GPA Calculation Engine | 100% | Unit tests (critical path) |

### NFR-MAINT-003: Documentation

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-MAINT-003 |
| **Category** | Maintainability |
| **Description** | The system shall be well-documented for developer onboarding. |
| **Criteria** | README with setup instructions. API documentation via Swagger/OpenAPI. Architecture Decision Records (ADRs) for key decisions. Inline code comments for complex business logic. Database schema documentation. Deployment guide. |

### NFR-MAINT-004: Logging & Monitoring

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-MAINT-004 |
| **Category** | Maintainability / Operability |
| **Description** | The system shall provide structured logging and health monitoring. |
| **Criteria** | Structured logging (JSON format) via Serilog. Log levels: Debug, Information, Warning, Error, Fatal. Correlation ID tracked across request pipeline. Health check endpoint: `/health`. Sensitive data never logged (passwords, tokens, PII). Log rotation and retention policy (30 days). |

---

## 12. Portability Requirements

### NFR-PORT-001: Containerization

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-PORT-001 |
| **Category** | Portability |
| **Description** | The system shall be containerized for deployment flexibility. |
| **Criteria** | All services packaged as Docker containers. docker-compose.yml for local development. Environment-specific configuration via environment variables. No hard-coded paths, URLs, or connection strings. Deployable on any Docker-compatible infrastructure (Azure, AWS, VPS, on-premises). |

### NFR-PORT-002: Configuration Management

| Attribute | Value |
|-----------|-------|
| **ID** | NFR-PORT-002 |
| **Category** | Portability |
| **Description** | All environment-specific configuration shall be externalized. |
| **Criteria** | Database connection strings via environment variables. JWT secrets via environment variables. API keys via environment variables. CORS origins configurable per environment. No secrets committed to source control. .env.example file with all required variables documented. |

---

## 13. NFR Verification Matrix

| NFR ID | Verification Method | Automated? | Frequency |
|--------|-------------------|------------|-----------|
| NFR-PERF-001 | Load testing (k6/JMeter) | Yes | Per release |
| NFR-PERF-002 | Lighthouse audit | Yes | Per release |
| NFR-PERF-003 | Query execution plan review | Manual | Per migration |
| NFR-SEC-001 | Penetration testing | Manual | Quarterly |
| NFR-SEC-002 | Unit tests for password validation | Yes | Per commit |
| NFR-SEC-004 | OWASP ZAP scan | Semi-auto | Monthly |
| NFR-SEC-007 | Rate limit integration tests | Yes | Per release |
| NFR-REL-001 | Uptime monitoring (UptimeRobot) | Yes | Continuous |
| NFR-USE-002 | Cross-device manual testing | Manual | Per release |
| NFR-ACC-001 | axe-core automated audit | Yes | Per release |
| NFR-L10N-001 | Translation completeness script | Yes | Per release |
| NFR-COMPAT-001 | BrowserStack cross-browser test | Semi-auto | Per release |
| NFR-MAINT-002 | Code coverage report (CI) | Yes | Per commit |

---

*End of Document — Non-Functional Requirements*
