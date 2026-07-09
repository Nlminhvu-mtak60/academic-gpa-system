# 06 — Core UI Components

> **Document ID**: UX-COMP-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Design component definitions and state configurations

---

## 1. Document Purpose

This document defines the structural parameters, borders, interactive behaviors, and states for the core UI components in the design system.

---

## 2. Component Design Specs

---

### 2.1 Buttons
Buttons trigger actions and submit forms. The system uses three variants:

```
Button Design Specs:
├── Primary Button: Solid color, brand background, high contrast text
├── Secondary Button: Outlined border, neutral background, primary text
└── Icon Button: Square surface, neutral hover, transparent background
```

#### Specifications:
*   **Sizing**:
    *   `Padding`: `10px` vertical, `16px` horizontal.
    *   `Height`: `40px` (Desktop), `44px` (Touch target on Mobile).
    *   `Border Radius`: `8px` (`rounded-lg`).
*   **States**:
    *   **Default**: Background: `indigo-600` (Primary), Border: `slate-300` (Secondary).
    *   **Hover**: Background: `indigo-500` (Primary), Background: `slate-50` (Secondary). Transition length: `150ms`.
    *   **Focus**: Enforces a `2px` focus outline wrapper: `outline-offset-2 outline-indigo-500`.
    *   **Disabled**: Background: `slate-100`, Text: `slate-400`, Cursor: `not-allowed`.

---

### 2.2 Input Fields
Form input elements capture user entries.

#### Specifications:
*   **Sizing**:
    *   `Padding`: `12px` horizontal, `10px` vertical.
    *   `Height`: `42px`.
    *   `Border Radius`: `6px` (`rounded-md`).
    *   `Font`: `Inter Regular`, `14px`.
*   **States**:
    *   **Default**: Border: `slate-200`, Background: `white` (Light) or `slate-800` (Dark).
    *   **Focus**: Border: `indigo-500`, Shadow: `0 0 0 3px rgba(99, 102, 241, 0.15)`.
    *   **Invalid (Validation Error)**: Border: `red-500`, Icon: Warning flag inside input suffix.
    *   **Disabled**: Background: `slate-50` (Light) or `slate-900` (Dark).

---

### 2.3 Cards
Cards are content containers used for displaying metrics, charts, and lists.

#### Specifications:
*   **Border Radius**: `12px` (`rounded-xl`).
*   **Padding**: `20px` (`p-5`) for standard layouts, `24px` (`p-6`) for larger sections.
*   **Shadow**:
    *   `Light Mode`: `0 1px 3px 0 rgba(0, 0, 0, 0.05), 0 1px 2px 0 rgba(0, 0, 0, 0.03)`
    *   `Dark Mode`: No shadows. Border: `1px solid slate-700` for container definition.

---

### 2.4 Data Tables
Used for displaying course lists, GPA aggregates, and audit logs.

#### Specifications:
*   **Header Row**: Height: `44px`, Background: `slate-50` (Light) or `slate-900` (Dark). Text: `Inter Medium`, `12px`, uppercase, `slate-500` color.
*   **Body Row**: Height: `48px`. Border Bottom: `1px solid slate-100` (Light) or `slate-700` (Dark). Hover State: Background: `slate-50/50` (Light) or `slate-800/50` (Dark).
*   **Cell Typography**: `Inter Regular`, `14px`, text color: `slate-900` (Light) or `slate-100` (Dark).

---

### 2.5 Badges (Status Indicators)
Used for displaying course letter grades and academic classification statuses.

#### Specifications:
*   **Sizing**: Padding: `4px` vertical, `8px` horizontal. Border Radius: `9999px` (fully rounded capsule).
*   **Variant Styles**:
    *   **Success (A / A+ / Excellent)**: Text: `emerald-700`, Background: `emerald-50` (Light).
    *   **Warning (C / B / Good)**: Text: `amber-700`, Background: `amber-50` (Light).
    *   **Danger (F / Fail)**: Text: `red-700`, Background: `red-50` (Light).

---

### 2.6 Toast Notifications
Temporary in-app banners that slide in from the top-right corner to report success, warning, or error messages.

#### Specifications:
*   **Sizing**: Width: `360px`, Padding: `16px`, Border Radius: `8px`.
*   **Behavior**: Dismisses automatically after 5 seconds.
*   **Variants**:
    *   *Success*: Border-left: `4px solid emerald-500`. Icon: Success checkmark.
    *   *Error*: Border-left: `4px solid red-500`. Icon: Warning exclamation mark.

---

*End of Document — Core UI Components*
