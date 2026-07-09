# 15 — Loading Strategy

> **Document ID**: ARC-FE-LOAD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Interface loading strategies and lazy-loading skeletons

---

## 1. Document Purpose

This document specifies the loading state patterns, page transition guidelines, and lazy-loading skeletons for the application.

---

## 2. Suspense and Code Splitting Integration

To keep initial bundles small, route-level page components are loaded lazily. The router wraps these pages inside `React.Suspense` blocks:

```tsx
// Configuration structure (non-implementation code)
const SemestersPage = React.lazy(() => import('./pages/SemestersPage'));

const RouterConfig = {
  path: '/semesters',
  element: (
    <React.Suspense fallback={<TableSkeleton />}>
      <SemestersPage />
    </React.Suspense>
  )
};
```

---

## 3. UI Skeleton Screen Guidelines

To improve the user experience during API calls, the system uses pulsing skeletons instead of generic loading spinners:

### 3.1 Skeleton Components
*   **TableSkeleton**: Renders static gray header columns followed by pulsing rows. Used when loading list views (e.g. course lists).
*   **DashboardSkeleton**: Renders three pulsing metrics cards followed by two chart container blocks.
*   **FormSkeleton**: Renders pulsing gray rectangles in place of form labels and input fields.

---

## 4. Input Button Loading States

*   **Submitting Forms**: When a student submits a form (e.g. saving scores), the button enters a loading state (`isLoading = true`):
    *   Replaces the button label with a small spinning indicator.
    *   Disables the button to prevent duplicate submissions.
    *   **Focus Preservation**: Keeps the button focused to allow keyboard users to track the loading state.

---

*End of Document — Loading Strategy*
