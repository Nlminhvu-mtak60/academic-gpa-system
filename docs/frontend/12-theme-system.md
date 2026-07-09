# 12 — Theme System

> **Document ID**: ARC-FE-THEME-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Styling class transitions and theme context specs

---

## 1. Theme Configuration Overview

The application supports Light and Dark modes. Styling configurations use Tailwind's class-based theme selector.

---

## 2. Dynamic Theme Toggling

Theme configurations are managed by a custom context:

*   **Initialization**: On startup, the theme manager reads the default theme from `localStorage`. If no setting is found, it falls back to the user's operating system preferences (using `window.matchMedia('(prefers-color-scheme: dark)')`).
*   **Root Class Injection**: Applying a theme adds or removes the `.dark` class from the `html` document root, updating the active CSS variables.
*   **State Persistence**: Selected themes are saved in `localStorage` and synced with the user's settings profile via the API after login.

---

## 3. Tailwind Configuration Specification

The `tailwind.config.ts` file is configured to use the class-based dark mode selector:

```typescript
// Configuration rules (non-implementation code)
export default {
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        bg: 'var(--color-bg-app)',
        card: 'var(--color-bg-card)',
        border: 'var(--color-border)',
      }
    }
  }
}
```

This configuration maps Tailwind utility classes (e.g. `bg-card`) directly to our CSS variables, ensuring theme changes are applied instantly across all styled elements.

---

*End of Document — Theme System*
