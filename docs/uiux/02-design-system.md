# 02 — Design System Overview

> **Document ID**: UX-SYS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Design system mapping and token structures

---

## 1. Document Purpose

This document outlines the design system architecture for the Academic GPA Management System. It establishes how style tokens, foundations, components, and templates are structured to ensure design consistency across the application.

---

## 2. Design System Architecture

The design system is structured using a modified atomic design methodology:

```
Design System Hierarchy
├── Foundations (Tokens)
│   ├── Color Palette (Light / Dark mode tokens)
│   ├── Typography (Font scale, weights, line heights)
│   └── Spacing & Grid (8px grid, layout margins, gaps)
├── Atoms (Basic Components)
│   ├── Button, Input field, Checkbox, Badge
│   └── Icon, Tooltip, Avatar, Spinner
├── Molecules (Composite Components)
│   ├── Form Field Group, Table Header, Card Wrapper
│   └── Chat Message Bubble, Toast Notification
├── Organisms (Complex Layout Blocks)
│   ├── Sidebar Navigation, Header Bar, Data Grid
│   └── GPA Goal Progress Ring, Chart Container
└── Templates (Page Shells)
    ├── GuestLayout (Centered Shell)
    └── DashboardLayout (Sidebar + Header + Body Grid)
```

---

## 3. UI Token Strategy

To maintain consistency and ease future styling changes, all properties are defined using semantic UI tokens:

1.  **Global Token Name**: Expressed as variables (e.g. `--color-primary`, `--font-size-body`).
2.  **Light/Dark Isolation**: Components must reference tokens, never static hex codes. Toggling themes updates the token mappings at the document root:
    *   `Light Mode`: `--color-background = #F8FAFC` (Slate 50)
    *   `Dark Mode`: `--color-background = #0F172A` (Slate 900)

---

## 4. Bilingual Support & Layout Considerations

The UI must adapt gracefully between English and Vietnamese:
*   **Text Expansion**: Vietnamese translations are often 20% to 30% longer than English strings (e.g., "Credits" $\rightarrow$ "Tín chỉ", "Attendance Score" $\rightarrow$ "Điểm chuyên cần"). Components must configure flexible sizing (e.g., `min-width` instead of fixed `width`) to prevent text clipping.
*   **Dynamic Date Formatting**: Timestamps must automatically update formats based on the selected language (`DD/MM/YYYY` for Vietnamese, `MM/DD/YYYY` for English).

---

*End of Document — Design System Overview*
