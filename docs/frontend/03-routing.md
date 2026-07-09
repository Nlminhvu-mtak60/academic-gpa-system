# 03 — Routing

> **Document ID**: ARC-FE-ROUT-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Client routing maps and route guard specifications

---

## 1. Client Routing Architecture

Frontend routing is managed using **React Router DOM** (using the data router API: `createBrowserRouter`). 

---

## 2. Lazy Loading & Code Splitting

To optimize initial page load times, route-level page components are loaded lazily using `React.lazy()`:
*   **Suspense fallback**: While a page component loads, a loading skeleton is displayed using `React.Suspense` (e.g. rendering table placeholders while the Semesters page bundle is fetched).

---

## 3. Route Guard Specifications

The router enforces access control using three route wrappers:

### 3.1 Public Route Guard (`PublicRoute`)
*   **Enforcement**: Applied to guest pages (`/login`, `/register`, `/forgot-password`).
*   **Behavior**:
    *   If the user is authenticated, redirects them to `/dashboard` (for students) or `/admin/dashboard` (for admins).
    *   If not authenticated, renders the request guest view.

### 3.2 Protected Route Guard (`ProtectedRoute`)
*   **Enforcement**: Applied to all student and admin portal pages.
*   **Behavior**:
    *   If the user session is active, renders the requested page.
    *   If not authenticated, redirects the user to `/login`, preserving the requested URL in a `returnUrl` query parameter.

### 3.3 Role Guard (`RoleRoute`)
*   **Enforcement**: Applied to admin routes (`/admin/*`).
*   **Behavior**:
    *   Verifies that the authenticated user claim role matches `'Admin'`.
    *   If unauthorized, redirects the user to a `403 Forbidden` page.

---

## 4. Route Map Configuration

| Path | Layout | Route Guard | Page Component |
|:---|:---|:---|:---|
| `/login` | GuestLayout | `PublicRoute` | `LoginPage` |
| `/register` | GuestLayout | `PublicRoute` | `RegisterPage` |
| `/shared/:token` | GuestLayout | None (Public) | `SharedTranscriptPage` |
| `/dashboard` | StudentLayout | `ProtectedRoute` | `StudentDashboardPage` |
| `/semesters` | StudentLayout | `ProtectedRoute` | `SemestersPage` |
| `/semesters/:id` | StudentLayout | `ProtectedRoute` | `SemesterDetailPage` |
| `/ai-advisor` | StudentLayout | `ProtectedRoute` | `AiAdvisorPage` |
| `/admin/dashboard` | AdminLayout | `RoleRoute` (Admin) | `AdminDashboardPage` |
| `/admin/students` | AdminLayout | `RoleRoute` (Admin) | `AdminStudentsPage` |

---

*End of Document — Routing*
