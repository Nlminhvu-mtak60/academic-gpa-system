# 11 — Coding Standards

> **Document ID**: ARC-STD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Code style requirements and standardization policies

---

## 1. Document Purpose

This document establishes the coding conventions, naming structures, formatting rules, and commit message formats for the Academic GPA Management System. Consistent coding standards ensure the codebase remains clean, readable, and easy to maintain.

---

## 2. Naming Conventions

All projects in the monorepo must follow these naming patterns:

### 2.1 Backend (.NET / C#)
*   **Namespaces & Classes**: Must use **PascalCase** (e.g. `namespace AcademicGpa.Application.Services`, `class GpaCalculator`).
*   **Interfaces**: Must prefix with a capital `I` followed by **PascalCase** (e.g. `interface IGpaCalculator`).
*   **Methods**: Must use **PascalCase** (e.g. `CalculateCumulativeGpa()`).
*   **Local Variables & Parameters**: Must use **camelCase** (e.g. `semesterId`, `courseScore`).
*   **DTOs (Data Transfer Objects)**: Must use **PascalCase** and end with `Dto` (e.g. `CourseDto`, `LoginRequestDto`).
*   **Controllers**: Must use **PascalCase** and end with `Controller` (e.g. `SemestersController`).
*   **Repositories**: Must use **PascalCase** and end with `Repository` (e.g. `CourseRepository`).

### 2.2 Frontend (React / TypeScript / HTML)
*   **Directories**: Must use **camelCase** (e.g. `/components`, `/hooks`, `/pages`).
*   **Component Files**: Must use **PascalCase** (e.g. `GpaSummaryCard.tsx`).
*   **Hooks**: Must prefix with `use` followed by **PascalCase** (e.g. `useAuth.ts`).
*   **Style Sheets**: Must use **lowercase** and hyphens (e.g. `globals.css`).

### 2.3 AI Service (Python / FastAPI)
*   **File Names & Modules**: Must use **snake_case** (e.g. `advisor_router.py`, `llm_service.py`).
*   **Classes**: Must use **PascalCase** (e.g. `class SystemPromptBuilder`).
*   **Functions & Variables**: Must use **snake_case** (e.g. `def calculate_final_prediction()`).

---

## 3. Code Style & Formatting Rules

To enforce consistency across different IDEs, the project uses automated linting and formatting tools:

1.  **Backend (.NET C#)**:
    *   An `.editorconfig` file is placed in the backend root directory to configure formatting rules (e.g. enforcing 4 spaces for indentation, requiring curly braces for control flow statements, and ordering namespaces alphabetically).
    *   **Linter**: Code analysis rules (Roslyn Analyzers) run during compilation, treating style warnings as errors.
2.  **Frontend (React SPA)**:
    *   **Prettier**: Enforces consistent code formatting (2 spaces for indentation, single quotes, trailing commas, and semicolons).
    *   **ESLint**: Enforces TypeScript code quality and React best practices. Runs automatically before commits.

---

## 4. Conventional Commits

Commit messages must follow the **Conventional Commits** standard (`type(scope): message`):

### Commit Types
*   `feat`: A new feature (e.g. `feat(auth): add google sign-in provider`).
*   `fix`: A bug fix (e.g. `fix(calc): resolve banker rounding edge cases`).
*   `docs`: Documentation updates only (e.g. `docs(architecture): update folder layout schema`).
*   `style`: Code formatting changes that do not affect code logic (e.g. fixing indents or removing whitespace).
*   `refactor`: Code changes that neither fix a bug nor add a feature, but improve code structure.
*   `test`: Adding missing tests or correcting existing ones.
*   `chore`: Updating build scripts, dependencies, or project configurations.

---

*End of Document — Coding Standards*
