# 05 — Component Architecture

> **Document ID**: ARC-FE-COMP-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Component design patterns and modular specifications

---

## 1. Component Classification

Components are organized into three tiers to ensure reusability and simplify testing:

1.  **Shared Components (`/components/common`)**: Stateless, generic UI elements styled with Tailwind utilities. They do not depend on global contexts or call API services directly (e.g. `Button`, `Modal`, `Input`).
2.  **Organism/Feature Components**: Dynamic layouts tied to specific features (e.g. `WhatIfSimulator.tsx`, `ChatWindow.tsx`). These can access global contexts and use custom hooks.
3.  **Pages (`/pages`)**: Route-level wrapper components that coordinate feature components and handle page loading states.

---

## 2. Shared Components Specifications

---

### 2.1 Button Component
*   **Properties**: `variant` (primary, secondary, outline, text), `size` (sm, md, lg), `isLoading` (boolean), `disabled` (boolean), `icon` (ReactNode).
*   **Behavior**: Disables clicks and displays a loading spinner when `isLoading` is true.

---

### 2.2 Modal Component
*   **Properties**: `isOpen` (boolean), `onClose` (callback function), `title` (string), `children` (ReactNode), `size` (sm, md, lg).
*   **Behavior**:
    *   Uses a Portal (`createPortal`) to render outside the parent DOM hierarchy, preventing layout nesting issues.
    *   **Focus Lock**: Restricts tab navigation to within the modal container when open. Closes automatically on pressing `Escape`.

---

### 2.3 Table Component
*   **Properties**: `columns` (array of header metadata), `data` (generic array `T[]`), `isLoading` (boolean), `pagination` (metadata object).
*   **Behavior**: Displays table pulsing skeletons when `isLoading` is true. Renders row-stacking lists on mobile screens.

---

## 3. Composition & Prop Drilling Rules

To keep the component tree clean and maintainable:
*   **Props Limitation**: Avoid prop drilling (passing props through more than 3 nested component levels). Use custom React Contexts or custom hooks to share state across deep components.
*   **TypeScript Enforcement**: Every component must declare explicit TypeScript interfaces defining its props. The use of `any` is prohibited.
*   **Styling Isolation**: Keep styles modular. Styles must be applied using utility classes within component files, avoiding custom CSS stylesheets.

---

*End of Document — Component Architecture*
