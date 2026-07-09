# 20 — Implementation Guidelines

> **Document ID**: ARC-FE-GUID-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Developer implementation workflows and checklist rules

---

## 1. Document Purpose

This document provides coding guidelines and quality standards for frontend developers working on the React client application.

---

## 2. Core Coding Practices Checklist

Developers must adhere to the following principles:

*   **Pure Component Design**: Keep components pure. Components must render their layout based on props and state, avoiding side effects during rendering (e.g. running math calculations or updating settings outside of `useEffect`).
*   **Encapsulate State in Custom Hooks**: Components must not manage complex async logic or state updates directly. All data fetching, caching, and state transitions must be encapsulated inside custom React hooks (e.g. `useGpa`).
*   **Responsive Styling Check**: Always test components at all responsive breakpoints (Mobile, Tablet, Desktop) before marked as complete.
*   **Bilingual Validation**: Check layouts with both English and Vietnamese translations, ensuring text does not overflow or clip when switched.

---

## 3. Git Branching & Pull Request (PR) Standards

*   **Branch Naming Rules**:
    *   Features: `feature/feature-name` (e.g. `feature/onboarding-wizard`).
    *   Bug Fixes: `bugfix/issue-name` (e.g. `bugfix/table-overflow`).
*   **PR Requirements**:
    *   The code must compile with zero TypeScript errors and pass all ESLint rules.
    *   All automated unit and integration tests must pass.
    *   The PR must be reviewed and approved by at least one Senior Frontend Architect before merging.

---

*End of Document — Implementation Guidelines*
