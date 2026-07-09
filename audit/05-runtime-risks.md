# Audit Report — Runtime Risks & Operational Vulnerabilities

This report outlines potential runtime risks, error handling resilience, and mitigation strategies for production environments.

---

## 1. Identified Runtime Risks

### Risk 1: Credentials & Secret Leakage
- **Details**:
  - The [appsettings.Production.json](file:///d:/aiiii/backend/src/AcademicGPA.API/appsettings.Production.json) file uses placeholder values for connection strings, API keys, and JWT keys.
  - Deploying without replacing these placeholders poses a significant security risk.
- **Verdict**: **VERIFIED** (Risk exists; mitigated by deployment pipeline variable injection).

### Risk 2: Database Startup Latency
- **Details**:
  - ASP.NET API attempts database connection upon initialization. If the SQL Server container takes longer than expected to initialize tables and seed data, the backend API could fail.
- **Mitigation**:
  - Managed by Docker Compose startup checks.
  - The API service uses the `depends_on` parameter, waiting for the database health check to pass before starting.
- **Verdict**: **VERIFIED**

### Risk 3: AI Service Outages & Network Delays
- **Details**:
  - If the FastAPI AI advisor container crashes or experiences networking lag, the backend API could hang while waiting for recommendation summaries.
- **Mitigation**:
  - The backend calls the service asynchronously using timeout thresholds, catching connection errors to display fallback messages instead of causing API crashes.
- **Verdict**: **VERIFIED**

### Risk 4: Refresh Token De-synchronization
- **Details**:
  - If a user sends multiple concurrent API calls with an expired access token, the system might trigger multiple simultaneous refresh token requests, causing token reuse violations and forcing unexpected user logouts.
- **Mitigation**:
  - Handled by frontend Axios interceptors, locking parallel requests until the token update completes.
- **Verdict**: **VERIFIED**
