# 01 — Solution Structure

> **Document ID**: ARC-BE-STR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: .NET 9 Solution structure and project separation blueprint

---

## 1. Document Purpose

This document details the .NET 9 solution configuration and project layout for the backend of the Academic GPA Management System. It specifies the references and boundaries between projects.

---

## 2. Visual Solution Directory Tree

The backend is structured as a single .NET solution (`AcademicGPA.sln`) containing four distinct class libraries and one test suite directory:

```
AcademicGPASystem/ (Backend Root)
├── AcademicGPA.sln                       # Core solution registry file
├── Directory.Packages.props              # NuGet central package management
│
└── src/
    ├── AcademicGPA.Domain/               # Project 1: Core Domain Entities
    │   ├── AcademicGPA.Domain.csproj
    │   ├── Entities/
    │   ├── ValueObjects/
    │   ├── Enums/
    │   └── Exceptions/
    │
    ├── AcademicGPA.Application/          # Project 2: MediatR Use Case Handlers
    │   ├── AcademicGPA.Application.csproj
    │   ├── Common/
    │   ├── Features/
    │   └── DTOs/
    │
    ├── AcademicGPA.Infrastructure/       # Project 3: EF Core Context & Services
    │   ├── AcademicGPA.Infrastructure.csproj
    │   ├── Persistence/
    │   └── Services/
    │
    └── AcademicGPA.API/                  # Project 4: Web API Controllers (Host)
        ├── AcademicGPA.API.csproj
        ├── Controllers/
        ├── Middleware/
        └── Program.cs
```

---

## 3. Project Configuration & Package Rules

The project uses **Central Package Management (CPM)** via `Directory.Packages.props` at the solution root to prevent version mismatches.

### 3.1 Project Reference Constraints
*   **AcademicGPA.Domain**: Has zero project references and zero dependencies on external frameworks (e.g. EF Core, ASP.NET Core).
*   **AcademicGPA.Application**: References `AcademicGPA.Domain`. Uses libraries: `MediatR`, `FluentValidation`.
*   **AcademicGPA.Infrastructure**: References `AcademicGPA.Application`. Uses libraries: `Microsoft.EntityFrameworkCore.SqlServer`, `BCrypt.Net-Next`.
*   **AcademicGPA.API**: References `AcademicGPA.Infrastructure`. Uses libraries: `Microsoft.AspNetCore.Authentication.JwtBearer`, `Serilog.AspNetCore`.

---

*End of Document — Solution Structure*
