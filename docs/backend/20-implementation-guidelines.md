# 20 — Implementation Guidelines

> **Document ID**: ARC-BE-GUID-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Developer coding guidelines and code review requirements

---

## 1. Document Purpose

This document provides coding guidelines and quality standards for backend developers working on the Academic GPA Management System.

---

## 2. Core Coding Practices Checklist

Developers must adhere to the following principles:

*   **SOLID Principles**:
    *   *Single Responsibility*: Each class must have exactly one reason to change. Keep handlers small and focused on a single use case.
    *   *Interface Segregation*: Avoid fat interfaces. Split services into specific, smaller interfaces (e.g. splitting `IEmailService` from `ISmsService`).
*   **DRY (Don't Repeat Yourself)**: Avoid duplicating code. Move common logic (such as component score rounding formulas) into reusable Domain utilities.
*   **Async by Default**: Database calls and external API operations must use asynchronous programming (`async/await`) to prevent blocking thread pools.
*   **Strict Compiler Warning Checks**: Build warnings are treated as errors. The code must compile with zero warnings before being merged.

---

## 3. Git Branching & Pull Request (PR) Standards

*   **Branch Naming Rules**:
    *   Features: `feature/feature-name` (e.g., `feature/jwt-rotation`).
    *   Bug Fixes: `bugfix/issue-name` (e.g., `bugfix/banker-rounding`).
*   **PR Requirements**:
    *   The code must compile with zero errors and zero warnings.
    *   All automated unit and integration tests must pass.
    *   New features must include accompanying unit tests.
    *   The PR must be reviewed and approved by at least one Senior Software Architect before merging.

---

*End of Document — Implementation Guidelines*
