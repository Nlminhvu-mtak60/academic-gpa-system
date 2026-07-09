# 01 — Project Structure

> **Document ID**: ARC-FE-STR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: React Single Page Application (SPA) folder structure blueprint

---

## 1. Document Purpose

This document details the project folder structure and file organization for the frontend application of the Academic GPA Management System.

---

## 2. Directory Tree Mapping

The frontend code resides in the `academic-gpa-client/` directory and is structured to keep concerns separated:

```
academic-gpa-client/
├── public/                       # Static files
│   └── locales/                  # Multi-language translation dictionaries
│       ├── en/
│       │   └── translation.json
│       └── vi/
│           └── translation.json
│
├── src/                          # Primary codebase
│   ├── api/                      # Axios client & api service layers
│   │   ├── axiosInstance.ts      # Axios client configuration & interceptors
│   │   └── authApi.ts
│   │
│   ├── components/               # UI components
│   │   ├── common/               # Atomic elements (Buttons, Inputs, Cards)
│   │   └── layout/               # Shell layout wrappers (Sidebar, Header)
│   │
│   ├── contexts/                 # Global state contexts (Auth, Theme)
│   │   ├── AuthContext.tsx
│   │   └── ThemeContext.tsx
│   │
│   ├── hooks/                    # Reusable React hooks
│   │   ├── useAuth.ts
│   │   └── useGpa.ts
│   │
│   ├── pages/                    # Route-level page layouts
│   │   ├── auth/                 # Sign-in, sign-up pages
│   │   ├── student/              # Student dashboard, courses, predictor
│   │   └── admin/                # Admin dashboards, student search
│   │
│   ├── router/                   # React Router configurations
│   │   ├── AppRouter.tsx
│   │   └── guards/               # Auth & Role route guards
│   │
│   ├── styles/                   # Tailwind config & style definitions
│   │   └── index.css
│   │
│   ├── types/                    # TypeScript interfaces
│   │   ├── auth.types.ts
│   │   └── course.types.ts
│   │
│   ├── utils/                    # Conversion logic & formatters
│   │   └── gradeConverter.ts
│   │
│   ├── App.tsx                   # Main app component
│   └── main.tsx                  # Application entry point
│
├── tailwind.config.ts            # Tailwind styling options
├── tsconfig.json                 # TypeScript compiler options
├── vite.config.ts                # Vite build pipeline configs
└── package.json                  # Package registry list
```

---

## 3. Directory Isolation Guidelines

To maintain a clean codebase, files must adhere to strict location rules:
*   **No API logic in pages**: Page files in `/pages` must not make direct HTTP requests using Axios or fetch. All API communication must be delegated to services in `/api`.
*   **Types Separation**: Component files must not define global data types. All core shared types must reside in the `/types` directory.
*   **Pure Utilities**: Files in `/utils` must contain pure, stateless functions (e.g. date formatting or number rounding) and have zero dependencies on React state or browser windows.

---

*End of Document — Project Structure*
