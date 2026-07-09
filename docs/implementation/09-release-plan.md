# 09 — Release Plan

> **Document ID**: PLN-REL-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Production compilation instructions, Docker packaging configurations, and CI/CD pipeline strategies

---

## 1. Document Purpose

This document outlines the build packaging, containerization, and deployment strategy for the Academic GPA Management System. It specifies the configuration parameters for compiling backend, frontend, and AI microservice components, defines the Docker environment configurations, and details the CI/CD pipeline steps for both staging and production releases.

---

## 2. Build & Compilation Strategy

### 2.1 Backend (.NET 9 Web API)
The backend .NET application is built for release targeting standard 64-bit Linux container hosts. The compilation command is configured as follows:
```bash
dotnet publish src/AcademicGPA.API/AcademicGPA.API.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained false \
  -o ./publish/backend
```
*Key parameters*: Central Package Management (CPM) is verified before compile. Trimming is disabled for the API layer to prevent dependency injection issues with MediatR handlers.

### 2.2 Frontend (React SPA client)
The React application is compiled using Vite to output optimized static HTML/JS/CSS assets:
```bash
npm run build
```
The build process compiles TypeScript, processes Tailwind directives, and places the output assets in the local `/dist` directory. Source maps are disabled for production builds to minimize bundle sizes.

### 2.3 AI Microservice (Python FastAPI)
The Python microservice dependency tree is locked using standard pip requirement files:
```bash
pip freeze > requirements.txt
```
The application uses the `uvicorn` server engine to run the FastAPI application.

---

## 3. Containerization (Docker)

To ensure consistent environments across staging and production, the system uses multi-stage Docker builds.

### 3.1 Backend Container Architecture (.NET API)
*   **Base SDK Image**: `mcr.microsoft.com/dotnet/sdk:9.0-alpine` (used during build and testing stages).
*   **Runtime Host Image**: `mcr.microsoft.com/dotnet/aspnet:9.0-alpine` (chosen to minimize security vulnerability exposure).
*   **Security Configuration**: The container processes run under an unprivileged user ID (`dotnetuser`), and port binding is limited to non-privileged ports ($> 1024$).

### 3.2 Frontend Container Architecture (Nginx Static Host)
*   **Build Environment**: Node.js alpine image.
*   **Runtime Web Server**: `nginx:alpine`.
*   **Routing Configuration**: Custom Nginx rules route all sub-paths back to `index.html` to support client-side React routing. Cache headers are configured to cache JS and CSS assets, while disabling caching for `index.html` to ensure users receive updates immediately.

### 3.3 AI Microservice Container Architecture (FastAPI Host)
*   **Runtime Host Image**: `python:3.11-slim-bookworm`.
*   **Dependencies**: System packages are kept to a minimum. Uvicorn binds to local ports and is configured to run behind the .NET API proxy.

---

## 4. CI/CD Deployment Pipeline

The application uses automated pipelines (e.g. GitHub Actions) to run tests and manage deployments.

```
                  +-----------------------------------+
                  |  Developer Pushes Code to GitHub  |
                  +-----------------------------------+
                                    |
                                    v
                  +-----------------------------------+
                  |  Pipeline Trigger: Lint & Test    |
                  |  (xUnit Tests, Jest Tests, Lint)  |
                  +-----------------------------------+
                                    |
            +-----------------------+-----------------------+
            | (If Tests Pass & Merge to Main)                | (If Git Tag Created)
            v                                               v
+---------------------------------------+       +---------------------------------------+
| STAGING DEPLOYMENT                    |       | PRODUCTION DEPLOYMENT                  |
| - Build Docker images with ':staging'  |       | - Build Docker images with ':version' |
| - Apply EF Core migrations to Staging |       | - Apply EF Core migrations to Prod    |
| - Deploy to Staging ECS/Kubernetes    |       | - Blue-Green swap on Prod cluster     |
+---------------------------------------+       +---------------------------------------+
```

### 4.1 Commit & Merge Validation Gates
Every pull request triggers validation workflows that must pass before merging:
1.  **Code Linting**: Checks formatting guidelines (ESLint for React, dotnet-format for C#).
2.  **Unit & Integration Tests**: Runs xUnit tests on backend projects and Jest tests on client components.
3.  **Security Scans**: Scans for hardcoded credentials and known vulnerabilities in dependencies.

### 4.2 Staging Deployment Pipeline
Triggered automatically on commits to the `main` branch:
1.  Builds backend, frontend, and FastAPI Docker images, tagging them with the `:staging` suffix.
2.  Deploys EF Core database updates to the staging database instance.
3.  Pushes and updates containers on the Staging environment.

### 4.3 Production Deployment Pipeline
Triggered only when a release is tagged (e.g., `v1.0.0`):
1.  Builds production Docker images, tagging them with the version number (e.g., `:1.0.0`) and `:latest`.
2.  Applies database migrations to the production SQL Server database.
3.  Deploys the containers to the production environment using a blue-green strategy, redirecting traffic only after health check verifications succeed.

---

## 5. Rollback Procedures

If a critical error is detected after deployment, the team will follow these rollback steps:

1.  **Container Rollback**: Immediately revert the container orchestrator configuration to pull the previous stable version tag (e.g., `:0.9.8`).
2.  **Database Migration Rollback**: If the release included database updates, run EF Core rollback commands targeting the previous migration ID:
    ```bash
    dotnet ef database update <PreviousStableMigrationName> --project src/AcademicGPA.Infrastructure
    ```
3.  **Client Cache Purge**: If frontend assets are updated, trigger a cache invalidation on the Content Delivery Network (CDN) to ensure browsers fetch the rolled-back static files.

---

*End of Document — Release Plan*
