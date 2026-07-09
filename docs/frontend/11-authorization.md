# 11 — Client-Side Authorization

> **Document ID**: ARC-FE-AUTHZ-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Role-based component visibility and route security rules

---

## 1. Document Purpose

This document defines client-side authorization rules, component-level permission checks, and route protection strategies.

---

## 2. Component-Level Visibility Controls (Guards)

To prevent users from seeing actions they do not have permissions for, the system uses a conditional guard component:

### 2.1 Authorized Component Wrapper (`<Authorized />`)
*   **Purpose**: Dynamically hides or renders elements based on user roles and claims.
*   **Properties**: `allowedRoles` (array of strings, e.g. `['Admin']`), `children` (ReactNode).
*   **Behavior**:
    *   If the user's role is not in `allowedRoles`, the component returns null, hiding the nested UI elements.
    *   Example:
        ```tsx
        // Conceptual visibility check (non-implementation code)
        <Authorized allowedRoles={['Admin']}>
          <button>Send Broadcast Announcement</button>
        </Authorized>
        ```

---

## 3. Core Route Protection Schemes

*   **Student Dashboard**: Accessible only if `role === 'Student'`. Administrative accounts navigating to `/dashboard` are redirected to `/admin/dashboard`.
*   **Admin Console**: Accessible only if `role === 'Admin'`. Student accounts navigating to `/admin/*` are blocked and redirected to the `403 Forbidden` page.

---

## 4. UI Resource Protection

*   **Action Disabling**: In addition to hiding components, buttons that trigger restricted actions (e.g. deleting a course) are disabled at the element level during pending operations.
*   **Route Interceptors**: If an API call fails with a `403 Forbidden` response, the global Axios interceptor catches the error and redirects the user to the `403 Forbidden` page.

---

*End of Document — Client-Side Authorization*
