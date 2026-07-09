# 03 — Folder Structure

> **Document ID**: ARC-FS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Unified repository folder layout structure tree

---

## 1. Directory Blueprint

The repository is structured as a monorepo to simplify developer onboarding, database migrations, CI/CD integrations, and deployment configurations.

```
AcademicGpaSystem/ (Workspace Root)
├── .github/                       # CI/CD pipelines
│   └── workflows/
│       ├── build-backend.yml      # CI for .NET API
│       ├── build-frontend.yml     # CI for React SPA
│       └── build-ai-service.yml   # CI for Python FastAPI
│
├── deploy/                        # Production & staging deployment configurations
│   ├── docker-compose.prod.yml    # Orchestrates production services
│   ├── docker-compose.staging.yml # Orchestrates staging testing
│   ├── nginx/
│   │   ├── nginx.conf             # Main reverse proxy configuration
│   │   └── mime.types
│   └── certs/                     # SSL certificates configuration (stub/vault)
│
├── database/                      # SQL scripts and seeding configs
│   ├── seeding/
│   │   ├── admin_seeding.sql      # Core admin account seeding
│   │   └── static_grading_data.sql# Calibration grading system constraints
│   └── migrations/                # Handled via EF Core, backed up as raw SQL
│
├── docs/                          # Project specifications and software architecture
│   ├── architecture/              # Phase 2 Design Documents (01-14)
│   │   ├── 01-overall-architecture.md
│   │   ├── 02-solution-architecture.md
│   │   └── ...
│   ├── 01-software-vision.md      # Phase 1 Requirements Documents
│   ├── 02-functional-requirements.md
│   └── ...
│
├── src/                           # Primary source code directory
│   ├── AcademicGPA.Domain/        # Backend Layer 1: Core Domain Models
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   └── Exceptions/
│   │
│   ├── AcademicGPA.Application/   # Backend Layer 2: Business Logic & MediatR Handlers
│   │   ├── Common/
│   │   │   ├── Behaviors/         # MediatR validation & logging hooks
│   │   │   ├── Interfaces/
│   │   │   └── Mappings/
│   │   ├── Features/              # Sliced by feature area
│   │   │   ├── Auth/
│   │   │   ├── Semesters/
│   │   │   └── ...
│   │   └── DTOs/
│   │
│   ├── AcademicGPA.Infrastructure/# Backend Layer 3: Persistence & Adapters
│   │   ├── Persistence/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── Configurations/    # Entity SQL mapping (Fluent API)
│   │   │   └── Repositories/
│   │   ├── Services/              # Auth, Email, GpaCalculator implementations
│   │   └── DependencyInjection.cs # Registers infrastructure services
│   │
│   ├── AcademicGPA.API/           # Backend Layer 4: Presentation API Entrypoint
│   │   ├── Controllers/
│   │   ├── Middleware/            # Exception handler, rate limit hooks
│   │   ├── appsettings.json
│   │   └── Program.cs
│   │
│   ├── AcademicGPA.AiService/     # AI Microservice (Python FastAPI)
│   │   ├── app/
│   │   │   ├── routers/           # Endpoint controllers (advisor, predictor)
│   │   │   ├── services/          # LLM integrations & prompt builders
│   │   │   ├── models/            # Pydantic schemas (Request / Response validation)
│   │   │   ├── core/              # Config settings, logging wrappers
│   │   │   └── main.py            # FastAPI app initialization
│   │   ├── requirements.txt       # Dependency definitions
│   │   └── Dockerfile
│   │
│   └── academic-gpa-client/       # Frontend Application (React SPA)
│       ├── public/                # Static assets, locales
│       │   └── locales/
│       │       ├── en/
│       │       └── vi/
│       ├── src/
│       │   ├── api/               # Axios services
│       │   ├── components/        # Reusable UI elements (atomic breakdown)
│       │   │   ├── common/        # Buttons, Inputs, Cards, Loaders
│       │   │   └── layout/        # Sidebar, Header, Footer templates
│       │   ├── contexts/          # Theme, Auth, Language context providers
│       │   ├── hooks/             # Custom state hooks (useAuth, useGpa)
│       │   ├── pages/             # Route-level screens (Dashboard, Semesters)
│       │   ├── router/            # React Router setup & guards
│       │   ├── styles/            # Tailwind core configuration & index.css
│       │   ├── types/             # TypeScript definitions
│       │   ├── utils/             # Converters, formatters
│       │   ├── App.tsx
│       │   └── main.tsx
│       ├── tailwind.config.ts
│       ├── tsconfig.json
│       ├── vite.config.ts
│       └── package.json
│
└── tests/                         # Automated testing suites
    ├── AcademicGPA.Domain.UnitTests/
    ├── AcademicGPA.Application.UnitTests/
    ├── AcademicGPA.Infrastructure.IntegrationTests/
    ├── AcademicGPA.API.IntegrationTests/
    ├── AcademicGPA.AiService.Tests/
    └── academic-gpa-client.test/
```

---

## 2. Directory Separation Rationale

1.  **Strict Layer Isolation (.NET)**: Slicing the C# backend into separate physical projects (`AcademicGPA.Domain`, `AcademicGPA.Application`, etc.) prevents illegal reference directions at compile time (e.g. preventing developers from referencing database models directly in the domain core).
2.  **Feature Slicing in Application Layer**: Within the `AcademicGPA.Application` layer, folders are grouped by domain features (e.g. `Features/Semesters/Commands`, `Features/Semesters/Queries`), keeping commands, queries, and DTOs closely grouped for better maintainability.
3.  **Client-Side Modularity**: The `academic-gpa-client` React directory separates route-level modules (`/pages`) from structural scaffolding components (`/components/layout`) and atomic controls (`/components/common`).

---

*End of Document — Folder Structure*
