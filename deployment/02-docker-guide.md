# 02 - Docker Administration Guide (Phase 11)

This guide documents the container setup, build stages, volumes configuration, networks topology, and administrative instructions for managing the system's Docker components.

---

## 1. Container Configuration Details

### Frontend Container (`gpa-frontend-prod`)
- **Docker Image**: Built using a two-stage `Dockerfile`:
  - **Stage 1 (Build)**: Node 20-alpine compiler building assets using `npm run build`.
  - **Stage 2 (Runtime)**: Minimal Nginx alpine container copying assets to `/usr/share/nginx/html` and exposing port `80`.
- **Nginx Config**: Serves static HTML and forwards `/api/` traffic to the backend API container.

### Backend Container (`gpa-backend-prod`)
- **Docker Image**: Built using two-stage `.NET 10.0` environment:
  - **Stage 1 (Build)**: Dotnet 10.0 SDK container executing restore, compilation, and publish.
  - **Stage 2 (Runtime)**: Dotnet 10.0 ASP.NET runtime image executing `AcademicGPA.API.dll` on port `8080`.

### AI Service Container (`gpa-ai-service-prod`)
- **Docker Image**: Built using python:3.11-slim container.
- **Service**: Runs `uvicorn main:app --host 0.0.0.0 --port 8000`.

### Database Container (`gpa-db-prod`)
- **Docker Image**: `mcr.microsoft.com/mssql/server:2022-latest`.
- **Persistence**: Data is persisted in docker volume `mssql_prod_data`.

---

## 2. Docker Command Reference

### Build & Run Stack
```bash
docker compose -f docker-compose.prod.yml up -d --build
```

### Stop Stack
```bash
docker compose -f docker-compose.prod.yml down
```

### Check Container Status
```bash
docker compose -f docker-compose.prod.yml ps
```

### View Live Resource Usage (Memory/CPU)
```bash
docker stats
```

---

## 3. Container Networking & Port Isolation
Only the `frontend` container exposes public ports (`80:80`) to the host network interface. The backend API, AI service, and database run in isolated private networks (`gpa-network`) accessible only between containers.
