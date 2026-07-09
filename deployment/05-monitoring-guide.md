# 05 - System Monitoring Guide (Phase 11)

This document contains guidelines for managing application logs, health check endpoints, diagnostic tools, and production alerts for the Academic GPA Management System.

---

## 1. Health Check Endpoint Matrix

The system exposes unified status endpoints for infrastructure monitoring tools:

| Service | Endpoint Path | Method | Expected Output | Mitigations on Failure |
|---|---|---|---|---|
| **Backend API** | `/health` | `GET` | `Healthy` (HTTP 200) | Check SQL Server database container connectivity. |
| **AI Advisor** | `/health` | `GET` | `{"status": "healthy", ...}` | Check FastAPI container status and requirements.txt load. |

### Automated Health Verification Command
Execute HTTP queries directly from host machine console:
```bash
# Verify API
curl -f http://localhost:5000/health

# Verify AI Service
curl -f http://localhost:8000/health
```

---

## 2. Production Logging Architecture

### Application & Database logs (Serilog)
The backend API writes system diagnostics and errors to two main targets:
1. **Stdout/Console**: Captured by container runtime logs (`docker logs gpa-backend`).
2. **Rolling log files**: Stored inside the container at `/app/logs/gpa-api-YYYYMMDD.log` and mounted to host volume `backend_logs` for backup.

### Querying Container Logs
Retrieve log details directly from container console output:
```bash
# Retrieve last 100 backend lines
docker logs --tail 100 gpa-backend-prod

# Check database connection failure logs
docker logs gpa-db-prod
```
---

## 3. Performance & Resource Tracking
Review active container system performance metrics:
- **Disk I/O**: Verify volume disk allocations for SQL Server backups.
- **Memory footprint**: Ensure the host server contains at least 2 GB unallocated RAM to accommodate database indexing operations.
