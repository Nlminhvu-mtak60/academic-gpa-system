# 03 - CI/CD Integration Guide (Phase 11)

This document provides instructions on setting up, managing, and troubleshooting the automated GitHub Actions CI/CD workflows pipeline.

---

## 1. CI/CD Workflow Architecture

The automated pipeline defined in [.github/workflows/ci-cd.yml](file:///d:/aiiii/.github/workflows/ci-cd.yml) executes on every `push` or `pull_request` to the main/master branches. The pipeline is split into three concurrent job definitions:

```
                   ┌───────────────────────┐
                   │    GitHub Push Event  │
                   └───────────┬───────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         ▼                     ▼                     ▼
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│   backend-test   │  │    frontend-     │  │   docker-build-  │
│                  │  │    lint-build    │  │     validate     │
└────────┬─────────┘  └────────┬─────────┘  └────────┬─────────┘
         │                     │                     │
         │ .NET 10.0           │ Node.js 20          │ Docker Buildx
         │ Restore & Build     │ Install             │ Build validation
         │ Unit & Integrations │ Lint & Compile      │ Compose verify
         ▼                     ▼                     ▼
┌──────────────────────────────────────────────────────────────┐
│                  Successful Release Gate                     │
└──────────────────────────────────────────────────────────────┘
```

---

## 2. Secrets & Configurations Management

For CD environments, configure the following secrets inside your GitHub repository settings under **Settings -> Secrets and Variables -> Actions**:

| Secret Key | Description | Example / Recommended Value |
|---|---|---|
| `DB_PASSWORD` | Strong administrative SQL Server password. | `ProdSecretPassword321!` |
| `JWT_SECRET` | Secret key containing at least 256 bits. | `ASecretKeyThatIsAtLeast32BytesLongSecret!` |
| `AI_SERVICE_API_KEY` | Shared secret API Key. | `AiServiceCommunicationToken999!` |

---

## 3. Local Validation Pipeline Command
Developers should execute quality checks locally before pushing changes:
```bash
# 1. Run Backend Tests
dotnet test backend/AcademicGPA.slnx

# 2. Run Frontend Linting
cd frontend && npm run lint

# 3. Validate Docker builds
docker compose build
```
