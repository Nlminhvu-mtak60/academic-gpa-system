# 03 — Color System

> **Document ID**: UX-COLOR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Color palette tokens and contrast specifications

---

## 1. Color Strategy & Palettes

The Academic GPA Management System uses a premium, high-contrast color palette designed to look professional, trustworthy, and modern. 

```
Color Theme Roles:
├── Primary (Indigo): Brand Identity, Main Actions, Headers
├── Secondary (Cyan): AI Highlights, Focus Elements, Interactive Indicators
├── Neutrals (Slate): Backgrounds, Borders, Body Text
└── Semantic Colors: Success (Green), Warning (Amber), Danger (Red)
```

---

## 2. Core Color Swatches

### 2.1 Brand & Semantic Colors (Shared across modes)

| Role | Token | HEX | HSL | Application |
|:---|:---|:---:|:---:|:---|
| **Primary Base** | `indigo-600` | `#4F46E5` | `243, 76%, 59%` | Primary buttons, active sidebar links |
| **Primary Light**| `indigo-500` | `#6366F1` | `239, 84%, 66%` | Hover states (Light), active states (Dark) |
| **Secondary Base**| `cyan-600` | `#0891B2` | `192, 91%, 38%` | AI elements, chart secondary series |
| **Secondary Light**| `cyan-500` | `#06B6D4` | `189, 94%, 43%` | AI chats and prediction indicators |
| **Success Base** | `emerald-600`| `#059669` | `162, 94%, 30%` | High GPA badges (A/A+), success alerts |
| **Warning Base** | `amber-500` | `#F59E0B` | `38, 92%, 50%` | Target GPA warnings, C/D grade badges |
| **Danger Base** | `red-600` | `#DC2626` | `0, 72%, 50%` | Failed course alerts (F), delete actions |

---

## 3. Light Mode vs. Dark Mode Surface Mappings

Themes are applied by modifying CSS variables at the document root:

### 3.1 Light Mode Token Mapping
```css
:root {
  --color-bg-app: #F8FAFC;          /* Slate 50 - Main background */
  --color-bg-card: #FFFFFF;         /* Pure white - Surface containers */
  --color-border: #E2E8F0;          /* Slate 200 - Borders & dividers */
  --color-text-primary: #0F172A;    /* Slate 900 - Headings & focus text */
  --color-text-secondary: #475569;  /* Slate 600 - Subtitles & captions */
  --color-text-muted: #94A3B8;      /* Slate 400 - Disabled placeholders */
  --color-shadow: rgba(15, 23, 42, 0.05); /* Soft dark shadow */
}
```

### 3.2 Dark Mode Token Mapping
```css
:root.dark {
  --color-bg-app: #0F172A;          /* Slate 900 - Main background */
  --color-bg-card: #1E293B;         /* Slate 800 - Surface containers */
  --color-border: #334155;          /* Slate 700 - Borders & dividers */
  --color-text-primary: #F8FAFC;    /* Slate 50 - Headings & focus text */
  --color-text-secondary: #94A3B8;  /* Slate 400 - Subtitles & captions */
  --color-text-muted: #64748B;      /* Slate 500 - Disabled placeholders */
  --color-shadow: rgba(0, 0, 0, 0.3);      /* Deeper shadow for elevation */
}
```

---

## 4. Contrast Requirements & Testing

*   **WCAG Compliance**: All text colors must pass WCAG 2.1 Level AA requirements.
*   **Disabled Elements**: Elements in a disabled state (e.g. disabled buttons) are excluded from contrast minimums, but must be styled clearly to indicate they are inactive.

---

*End of Document — Color System*
