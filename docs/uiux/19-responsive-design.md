# 19 — Responsive Design

> **Document ID**: UX-RESP-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Responsive layout grids and adaptive interface rules

---

## 1. Document Purpose

This document details the responsive design strategies and adaptive behaviors implemented to support mobile, tablet, and desktop viewports.

---

## 2. Table-to-Card Layout Transitions (Mobile Viewports)

Detailed data tables (such as course lists or admin tables) do not fit on small mobile screens ($<640\text{px}$). The system implements a **Responsive Table-to-Card** pattern:

```
Table Grid View (Desktop / Tablet):
+-------------------------------------------------------------+
| Code    | Course Name         | Credits | Grade  | Actions  |
+---------+---------------------+---------+--------+----------+
| CS101   | Intro to Comp Sci   | 3       | 8.5    | [Edit]   |
+-------------------------------------------------------------+

Card Stack View (Mobile Viewports):
+-----------------------------------+
| CS101 - Intro to Comp Sci         |
| Credits: 3                        |
| Grade: 8.5 (A)                    |
| [ Edit Scores ]                   |
+-----------------------------------+
```

### 2.1 CSS Implementation Strategy
On viewports $<640\text{px}$, table header elements (`<thead>`) are hidden (`hidden`). Table rows (`<tr>`) are styled as individual cards with borders and shadows, while cells (`<td>`) are displayed as block elements with flex layout alignment (`flex justify-between`).

---

## 3. Sidebar Navigation Collapsing

*   **Desktop Viewport ($\ge 1024\text{px}$)**: The sidebar is fixed on the left side of the screen (`w-64`).
*   **Tablet Viewport ($640\text{px} - 1024\text{px}$)**: The sidebar collapses into a thin strip displaying only menu icons, maximizing space for the main content.
*   **Mobile Viewport ($<640\text{px}$)**: The sidebar is hidden. Clicking the header menu icon opens the navigation menu as a full-height overlay slide-out drawer (`fixed inset-y-0 left-0 w-80`).

---

## 4. Grid Wrapping & Column Stacking Rules

To ensure layouts adjust fluidly:
1.  **Dashboard Grids**: Grid columns use CSS grid with auto-fit utilities (`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6`), wrapping columns automatically as the screen size shrinks.
2.  **Form Layouts**: Multi-column forms (e.g. settings pages) stack fields into a single column on mobile viewports.
3.  **Horizontal Scrolling**: In modules where tables must preserve their row structure, the container must configure horizontal scrolling (`overflow-x-auto`) to prevent content overlap.

---

*End of Document — Responsive Design*
