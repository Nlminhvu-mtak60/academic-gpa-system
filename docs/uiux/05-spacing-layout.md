# 05 — Spacing & Layout

> **Document ID**: UX-SPACE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Spatial grid layouts and responsive breakpoints

---

## 1. Spatial Grid System

To ensure layouts remain consistent, the system uses an **8px grid** for spacing, padding, margins, and layout offsets.

---

## 2. Spacing Tokens

| Token | Multiplier | Value (px / rem) | Application |
|:---|:---|:---:|:---|
| `space-xxs` | 0.5 $\times$ 8px | `4px / 0.25rem` | Badge padding, micro gaps |
| `space-xs` | 1 $\times$ 8px | `8px / 0.5rem` | Label-to-input gap, inline element margins |
| `space-sm` | 1.5 $\times$ 8px | `12px / 0.75rem` | Card padding (Mobile), input field padding |
| `space-md` | 2 $\times$ 8px | `16px / 1.0rem` | Standard card padding, list item gaps |
| `space-lg` | 3 $\times$ 8px | `24px / 1.5rem` | Content container padding, dashboard card gaps |
| `space-xl` | 4 $\times$ 8px | `32px / 2.0rem` | Main layout outer margins |
| `space-xxl` | 6 $\times$ 8px | `48px / 3.0rem` | Hero block spacing, section separators |

---

## 3. Responsive Breakpoints

The client application adapts dynamically using these responsive breakpoints:

| Breakpoint | Width (Min) | Columns | Layout Margins | Gutter |
|:---|:---|:---:|:---:|:---:|
| **Mobile (`sm`)** | `375px` | 4 | `16px` | `12px` |
| **Tablet (`md`)** | `640px` | 8 | `24px` | `16px` |
| **Desktop (`lg`)** | `1024px` | 12 | `32px` | `24px` |
| **Large Monitor (`xl`)** | `1440px` | 12 | `48px` | `24px` |

---

## 4. Z-Index Elevations Scale

To prevent rendering overlapping issues with dropdowns, tooltips, overlays, and modals, the system uses a standardized Z-Index scale:

*   `z-below`: `-10` $\rightarrow$ Background decorations.
*   `z-base`: `0` $\rightarrow$ Standard content, cards, inputs.
*   `z-sticky`: `100` $\rightarrow$ Top headers, sticky navigation bars.
*   `z-dropdown`: `200` $\rightarrow$ Dropdown lists, context menus.
*   `z-overlay`: `300` $\rightarrow$ Modal backgrounds, blackouts.
*   `z-modal`: `400` $\rightarrow$ Modal containers, dialog overlays.
*   `z-toast`: `500` $\rightarrow$ In-app toast alerts, success banners.
*   `z-tooltip`: `600` $\rightarrow$ Context tooltips.

---

*End of Document — Spacing & Layout*
