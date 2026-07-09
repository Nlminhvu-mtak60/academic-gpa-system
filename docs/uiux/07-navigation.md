# 07 — Navigation System

> **Document ID**: UX-NAV-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Layout navigation guides and interface component structures

---

## 1. Document Purpose

This document details the navigation architecture, layout structures, and switching elements of the application.

---

## 2. Navigation Layout Structures

---

### 2.1 Sidebar Navigation (Desktop)
*   **Location**: Fixed left side. Width: `260px` (`w-64`).
*   **Header Section**: Logo icon + Outfit bold brand name: "Academic GPA".
*   **Menu Section**:
    *   List of navigation links with icons (Dashboard, Semesters & Courses, Statistics, Goal Planner, Exam Prediction, AI Advisor, Public Sharing).
    *   **Link States**:
        *   *Default*: Text: `slate-600` (Light) or `slate-400` (Dark). Hover: Background: `slate-100` (Light) or `slate-800` (Dark).
        *   *Active (Selected)*: Background: `indigo-50` (Light) or `slate-800` (Dark). Text: `indigo-600` (Light) or `white` (Dark). Left border highlight: `3px solid indigo-600`.
*   **Footer Section**: User profile widget displaying the user's avatar, name, and a sign-out button.

---

### 2.2 Top Header Bar
*   **Location**: Fixed top of page, adjacent to the sidebar. Height: `64px` (`h-16`).
*   **Burger Menu**: Visible on mobile screens ($<1024\text{px}$) to toggle the sidebar drawer.
*   **Active Page Title**: Displays the current route name (e.g. "Goal Planner") in `Outfit Bold`, `20px`.
*   **Controls Container (Right-aligned)**:
    *   **Language Switcher**: Displays flag buttons (`VN` / `EN`) to toggle languages.
    *   **Theme Switcher**: A round toggle button showing sun/moon icons to switch themes.
    *   **Notification Bell**: Displays a red unread badge count. Clicking it opens a dropdown panel showing the 5 most recent unread messages and a link to "View All".

---

### 2.3 Mobile Navigation Tab Bar
*   **Location**: Fixed bottom of screen, visible only on viewports $<640\text{px}$ (`sm`). Height: `56px`.
*   **Layout**: 4 evenly-spaced tab buttons:
    1.  **Dashboard**: Home icon.
    2.  **Academic**: Calendar icon (direct link to Semesters list).
    3.  **AI Advisor**: Message icon.
    4.  **Profile**: Settings cog/avatar icon.
*   **Visual Styling**: Semi-transparent blurred container (`backdrop-blur-md bg-white/80` or `bg-slate-900/80`). Active tab elements are highlighted in `indigo-600`.

---

*End of Document — Navigation System*
