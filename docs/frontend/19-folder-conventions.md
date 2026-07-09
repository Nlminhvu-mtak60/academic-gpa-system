# 19 — Folder Conventions

> **Document ID**: ARC-FE-FOLD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: File and directory naming standards

---

## 1. Document Purpose

This document specifies naming standards and organization guidelines for frontend files and folders.

---

## 2. File Naming Conventions

All frontend files must adhere to strict naming conventions based on their category:

### 2.1 UI Component Files
*   **Convention**: **PascalCase** with `.tsx` extension.
*   **Examples**: `Button.tsx`, `Sidebar.tsx`, `GpaSummaryCard.tsx`.

### 2.2 Page Wrapper Files
*   **Convention**: **PascalCase** ending with `Page.tsx`.
*   **Examples**: `LoginPage.tsx`, `DashboardPage.tsx`, `SemestersPage.tsx`.

### 2.3 Custom Hook Files
*   **Convention**: **camelCase** starting with the prefix `use` with `.ts` extension.
*   **Examples**: `useAuth.ts`, `useGpa.ts`, `useDebounce.ts`.

### 2.4 API Service Files
*   **Convention**: **camelCase** ending with the suffix `Api.ts`.
*   **Examples**: `authApi.ts`, `courseApi.ts`, `gpaApi.ts`.

### 2.5 Style & Config Files
*   **Convention**: **lowercase** with dashes (kebab-case).
*   **Examples**: `index.css`, `tailwind.config.ts`, `vite.config.ts`.

---

## 3. Directory Layout Rules

*   **Subfolders inside Components**: Each feature component folder must contain an `index.ts` file to clean up import statements:
    ```typescript
    // In components/common/index.ts:
    export { default as Button } from './Button';
    export { default as Input } from './Input';
    ```
*   **Clean Pages Hierarchy**: Sliced by module area (`/pages/auth`, `/pages/student`, `/pages/admin`) to keep page components organized.

---

*End of Document — Folder Conventions*
