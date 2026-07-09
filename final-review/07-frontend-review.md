# Frontend Review — Client UX, Themes, & Translations

The frontend client is a Single Page Application (SPA) built using React, Vite, and TypeScript.

---

## 1. Responsive Layout & Design System

- **Layout Structure**:
  - Leverages standard CSS Grid and CSS Flexbox for layouts, ensuring absolute responsiveness from small mobile screens (320px) up to ultra-wide displays.
  - Separates student dashboard interfaces from complex administrative tabular grids with adaptive panels that fold into drawer menus on mobile.
- **Design Aesthetic**:
  - Follows premium aesthetic principles.
  - Subtle micro-animations on interactive cards, smooth state transitions, and high-quality charts representing GPA progressions.

---

## 2. Dynamic Theme System (Light & Dark Mode)

- **Implementation**:
  - CSS Variables define core color palettes (e.g., `--bg-primary`, `--text-primary`, `--accent-color`).
  - [UserSettingsContext](file:///d:/aiiii/frontend/src/contexts/UserSettingsContext.tsx) manages the client settings state.
  - Switching themes applies the `.dark-mode` or `.light-mode` class directly to the `<html>` or `<body>` element, allowing instant style shifts.

---

## 3. Localization & Language System (i18n)

- **Languages Supported**: Vietnamese (VI) and English (EN).
- **Implementation**:
  - Native JSON translation dictionaries are stored in the client application.
  - A custom hook translates text keys in real-time, providing immediate translation without page reloads.
  - Covers all validation messages, dates format, chart legends, and navigation headers.

---

## 4. UI States & User Experience Patterns

- **Loading States**:
  - Skeleton placeholders for tables, cards, and charts prevent layout shifts during async API fetches.
  - Micro-spinners are displayed inside buttons during form submissions to prevent double actions.
- **Empty States**:
  - Clean, illustrative empty state views appear when no semesters, courses, goals, or notifications exist, advising the student on the next action (e.g. "Click 'Add Course' to begin tracking").
- **Error Boundaries & Alerts**:
  - HTTP status codes are translated to friendly error messages.
  - Error states feature a "Retry" trigger button to refetch failed resource requests.
