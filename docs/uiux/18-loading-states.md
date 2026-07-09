# 18 — Loading States

> **Document ID**: UX-LOAD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Loading skeleton blueprints and spinner specifications

---

## 1. Document Purpose

This document defines the user interface configurations for loading skeletons, spinners, and page transition states.

---

## 2. Skeleton Screens (Pulsing Placeholders)

Instead of displaying generic loading spinners, the system uses **Skeleton Screens** to show a preview of page elements. This improves perceived performance.

*   **Pulsing Animation**: Skeletons use Tailwind's `animate-pulse` class, transitioning opacity from $100\%$ to $50\%$ on a $1.5$-second loop.
*   **Colors**:
    *   *Light Mode*: Background placeholder color: `slate-200`.
    *   *Dark Mode*: Background placeholder color: `slate-700`.

---

## 3. Skeleton Blueprints

### 3.1 Metric Card Skeleton
Used for loading KPI widgets on the dashboard.

```
+-----------------------------------+
|  [ Pulsing Circle - Icon area ]   |
|  [ Pulsing Bar - Title ]          |
|  [ Pulsing Large Bar - Value ]    |
+-----------------------------------+
```

### 3.2 Data Table Row Skeleton
Used for loading lists of courses, notifications, and student logs.

```
+-------------------------------------------------------------+
| Header Row (Static grey blocks)                             |
+-------------------------------------------------------------+
| [ Pulsing Bar ]  [ Pulsing Bar ]  [ Pulsing Bar ]           |
| [ Pulsing Bar ]  [ Pulsing Bar ]  [ Pulsing Bar ]           |
| [ Pulsing Bar ]  [ Pulsing Bar ]  [ Pulsing Bar ]           |
+-------------------------------------------------------------+
```

### 3.3 Chart Container Skeleton
Used for loading dashboard trends and histograms.

```
+-------------------------------------------------------------+
| [ Pulsing Card Title Header ]                               |
|                                                             |
|   +-----------------------------------------------------+   |
|   |                  Pulsing Graph Block                |   |
|   +-----------------------------------------------------+   |
|                                                             |
+-------------------------------------------------------------+
```

---

## 4. Spinners & Overlay Blockers

For quick actions (e.g. clicking "Save" on a modal or logging in), the system uses focused indicators:

1.  **Button Spinners**: Replaces button text with a spinning circle (`border-2 border-t-transparent animate-spin w-5 h-5`). The button remains disabled during the loading state to prevent double submission.
2.  **Full-Screen Blockers**: Used during initial authentication checks. Displays a clean, centered loading animation (pulsing brand logo) over a background matching the active theme.

---

*End of Document — Loading States*
