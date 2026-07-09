# 20 — Accessibility

> **Document ID**: UX-ACC-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Accessibility guidelines and compliance checklists

---

## 1. Document Purpose

This document defines the accessibility standards and keyboard navigation requirements for the Academic GPA Management System, ensuring compliance with **WCAG 2.1 Level AA** guidelines.

---

## 2. Accessibility Guidelines

The interface must implement the following controls:

---

### 2.1 Color Contrast Compliance
*   **Text Contrast**: Small body text ($<18\text{px}$) must maintain a contrast ratio of at least **4.5:1** against backgrounds in both Light and Dark modes. Large headings ($\ge 18\text{px}$) must maintain a ratio of at least **3.0:1**.
*   **Non-Text Elements**: Graphical objects, border indicators, and input boundaries must maintain a minimum contrast ratio of **3.0:1** against surrounding backgrounds.

---

### 2.2 Keyboard Navigation & Focus Indicators
Users must be able to navigate and interact with all elements using only a keyboard:

1.  **Logical Tab Sequence**: Interactive components must follow a logical reading order from top-to-bottom, left-to-right. Skip links (`Skip to main content`) must be provided for screen readers.
2.  **Visible Focus Indicators**: Custom keyboard focus styles must be visible on active elements:
    ```css
    *:focus-visible {
      outline: 2px solid #6366F1; /* Indigo 500 */
      outline-offset: 2px;
    }
    ```
3.  **Dropdowns & Modals**: Modals must trap keyboard focus inside the dialog when opened, and pressing the `Escape` key must close the active modal immediately.

---

### 2.3 Screen Reader Integration (ARIA)
*   **Form Elements**: All input fields must define matching HTML `<label>` elements. Interactive controls must define explicit label references (`aria-label` or `aria-labelledby`).
*   **Status Alerts**: Dynamic panels (such as AI chat responses or score recalculation previews) must configure live regions (`aria-live="polite"`) to notify screen readers of content updates automatically.
*   **Hiding Decorative Elements**: Non-interactive icons and graphic backgrounds must configure `aria-hidden="true"` to prevent screen reader noise.

---

### 2.4 Scalable Font Layouts
*   **Relative Units**: Layout properties (padding, margins, font sizes) must be defined using relative units (`rem` or `em`) instead of static pixels (`px`). This ensures layouts scale gracefully if a user increases their browser's default font size.

---

*End of Document — Accessibility*
