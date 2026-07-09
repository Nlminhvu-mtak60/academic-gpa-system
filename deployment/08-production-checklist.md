# 08 - Production Pre-Flight Checklist (Phase 11)

This checklist provides the final sanity checks and validation steps for system engineers before officially releasing the system to users.

---

## 1. System Readiness Checklist

### Configuration Sanitization
- [ ] Production `.env` file generated on host server.
- [ ] Database SA password contains strong alphanumeric combination.
- [ ] AI Service Api key changed from default values.
- [ ] JWT Signing Key changed from default values.

### Persistence & Data Recovery
- [ ] Database docker volume mounts correctly mapped on host machine.
- [ ] Backup script ([backup.sh](file:///d:/aiiii/deployment/scripts/backup.sh)) execution tested.
- [ ] Restore script ([restore.sh](file:///d:/aiiii/deployment/scripts/restore.sh)) execution tested.
- [ ] Daily backup cron job active on host server.

### Diagnostics & Logging
- [ ] Core health checks mapped on `/health` (backend and AI container) return healthy.
- [ ] Backend Serilog folder permissions verified (can write files).
- [ ] Docker logging policies configured (e.g. limit log size to 10MB/file).

---

## 2. Go-Live Verification Routine

1. Run compile tests inside docker image:
   ```bash
   docker compose -f docker-compose.prod.yml build
   ```
2. Launch containers:
   ```bash
   docker compose -f docker-compose.prod.yml up -d
   ```
3. Inspect startup logs for backend container:
   ```bash
   docker logs gpa-backend-prod
   ```
4. Query endpoint statuses:
   - Backend status: `curl -I http://localhost:5000/health` -> should return `HTTP 200 OK`.
   - AI status: `curl -I http://localhost:8000/health` -> should return `HTTP 200 OK`.
