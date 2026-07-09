# 17 — Frontend Testing Strategy

> **Document ID**: ARC-FE-TEST-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Frontend test configurations and verification rules

---

## 1. Document Purpose

This document defines the testing frameworks, testing slices, and verification guidelines for the React client application.

---

## 2. Test Runner & Libraries

*   **Test Runner**: **Vitest** (configured to run within the Vite pipeline).
*   **Component Rendering**: **React Testing Library (RTL)** (verifies component behaviors by querying user-facing elements, matching screen-reader behaviors).
*   **API Mocking**: **Mock Service Worker (MSW)**. Intercepts network requests and returns mock JSON payloads, allowing tests to verify API integration without calling the real backend.

---

## 3. Testing Slices

The testing strategy is divided into three test layers:

### 3.1 Unit Testing (Utilities & Converters)
*   **Target**: Pure javascript/typescript functions (e.g. `/src/utils/gradeConverter.ts`).
*   **Goal**: Verifies GPA and letter-grade conversion rules directly, aiming for 100% test coverage.

### 3.2 Component Testing (Shared Elements)
*   **Target**: Shared UI components inside `/components/common`.
*   **Goal**: Verifies that components render correctly across different states (e.g., confirming a button disables clicks when in a loading state, or checking modal close triggers).

### 3.3 Integration Testing (Routed Pages)
*   **Target**: Complete routed views inside `/pages`.
*   **Goal**: Verifies workflows (e.g. logging in, entering grades, or sending announcements) by simulating user actions and mocking API calls using MSW.

---

## 4. Querying Best Practices

To ensure tests remain resilient to structural layout changes, DOM queries must prioritize user-accessible attributes over CSS classes or testing IDs:
*   **Priority 1**: Query by accessible role (`screen.getByRole('button', { name: /save/i })`).
*   **Priority 2**: Query by input label text (`screen.getByLabelText(/email/i)`).
*   **Priority 3**: Query by text content (`screen.getByText(/gpa tích lũy/i)`).

---

*End of Document — Frontend Testing Strategy*
