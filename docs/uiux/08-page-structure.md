# 08 — Page Structure

> **Document ID**: UX-PAGE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Interface layout blueprints and grid grid parameters

---

## 1. Document Purpose

This document defines the structural grid models and layout blueprints of the application.

---

## 2. Layout Blueprints

The application uses three layout wrappers: **Auth Layout**, **Main Portal Layout**, and **Admin Layout**.

---

### 2.1 Guest / Auth Layout
Used for non-authenticated actions (Login, Registration, Password recovery) and public transcript sharing.

```
+------------------------------------------+
|  [Language Toggle]                       |
|                                          |
|            +-------------------+         |
|            |    Brand Logo     |         |
|            |    Card Title     |         |
|            |  +-------------+  |         |
|            |  | Form Fields |  |         |
|            |  +-------------+  |         |
|            |    CTA Button     |         |
|            +-------------------+         |
|                                          |
+------------------------------------------+
```

*   **Grid Structure**: Centered flexbox container (`flex items-center justify-center min-h-screen`).
*   **Card Container**: Outer margins: `16px` on mobile. Fixed width: `440px` on desktop viewports.

---

### 2.2 Main Portal Layout (Student Portal)
Used for all authenticated student actions.

```
+-------------------------------------------------------------+
| Sidebar (260px)  | Top Header (64px)                        |
|                  | Title             [Lang] [Theme] [Bell]  |
|                  +------------------------------------------+
|                  | Main Scrollable Content Area             |
|                  |                                          |
|                  |  +------------------------------------+  |
|                  |  | Content Grid (Grid Row 1)          |  |
|                  |  +------------------------------------+  |
|                  |  | Content Grid (Grid Row 2)          |  |
|                  |  +------------------------------------+  |
|                  |                                          |
|                  |  Footer Details / Copyright              |
|                  |                                          |
+------------------+------------------------------------------+
```

*   **Sidebar Zone**: Fixed left position. Hidden on viewports $<1024\text{px}$, replacing it with a toggleable overlay drawer.
*   **Header Zone**: Sticky top position.
*   **Main Scrollable Content Area**: Flex column container with local scrolling.
*   **Content Grid Padding**:
    *   *Desktop*: `32px` padding on all sides (`p-8`).
    *   *Mobile*: `16px` padding on all sides (`p-4`).

---

### 2.3 Portal Content Grid Zones

To display data consistently, dashboard pages must follow standard layout row grids:

#### Row 1: Metrics Cards Grid
*   **Layout**: 3-column responsive layout (`grid grid-cols-1 md:grid-cols-3 gap-6`).
*   **Use Cases**: GPA summary cards, Goal progress rings, and credit progression trackers.

#### Row 2: Detail Panels Grid
*   **Layout**: 2-column unequal layout (`grid grid-cols-1 lg:grid-cols-3 gap-6`).
    *   *Left Panel* (Col-span: 2): Wide tables (e.g. current semester course lists).
    *   *Right Panel* (Col-span: 1): Tall vertical widgets (e.g. goal simulator input cards).

---

*End of Document — Page Structure*
