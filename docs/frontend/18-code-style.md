# 18 — Frontend Code Style

> **Document ID**: ARC-FE-STYLE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: ESLint rules and Prettier configuration templates

---

## 1. Document Purpose

This document specifies the code styling configurations and quality checks for the React client application to ensure code consistency across developer environments.

---

## 2. Formatting Configuration (Prettier)

The project root contains a `.prettierrc` file specifying the formatting rules:

```json
{
  "printWidth": 100,
  "tabWidth": 2,
  "useTabs": false,
  "semi": true,
  "singleQuote": true,
  "trailingComma": "all",
  "bracketSpacing": true,
  "arrowParens": "always"
}
```

*   **Enforcement**: Prettier runs automatically as a pre-commit hook (using `husky` and `lint-staged`) to format files before they are committed to git.

---

## 3. Linter Configuration (ESLint)

The project root contains an `.eslintrc.json` file configuring linter rules:

*   **Enforced Rules**:
    *   `@typescript-eslint/no-unused-vars`: Warns or errors on unused imports or variables.
    *   `react-hooks/rules-of-hooks`: Enforces the rules of hooks.
    *   `react-hooks/exhaustive-deps`: Warns on missing hook dependencies.
    *   `@typescript-eslint/no-explicit-any`: Errors if `any` is used, requiring explicit types.

---

## 4. Import Ordering Standard

Imports must be grouped and sorted in a specific order:
1.  React and standard libraries (`react`, `react-router-dom`).
2.  Third-party packages (e.g. `lucide-react`, `recharts`).
3.  Application-internal hooks, contexts, and APIs (`@/hooks`, `@/api`).
4.  UI components (`@/components`).
5.  Stylesheets, assets, and types (`@/styles`, `@/types`).

---

*End of Document — Frontend Code Style*
