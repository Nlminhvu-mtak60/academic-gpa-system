# 16 — Performance Optimization

> **Document ID**: ARC-FE-PERF-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Performance optimization rules and build bundle parameters

---

## 1. Document Purpose

This document outlines the frontend performance optimization rules and bundle build parameters for the application.

---

## 2. React Rendering Optimizations (Memoization)

To keep the application highly responsive on lower-end devices, developers must follow strict rules to prevent unnecessary component re-renders:

### 2.1 useMemo
*   **Application**: Must be used when sorting or filtering large datasets in memory (e.g. searching the student grid on the admin dashboard).
*   **GPA Calculations**: Local what-if simulations must wrap the course GPA aggregations in `useMemo` to avoid running calculations on every keystroke.

### 2.2 useCallback
*   Callbacks passed as properties to complex child components (e.g. close indicators in modals or row updates in course tables) must be wrapped in `useCallback` to maintain reference equality between renders.

### 2.3 React.memo
*   Heavy, read-only UI components (such as line charts, distributions pie charts, and data grid rows) must be wrapped in `React.memo` to prevent re-renders when parent states change.

---

## 3. Bundle Build Optimizations (Vite Configs)

The `vite.config.ts` file configures manual chunk splitting to keep individual file sizes small:

*   **Vendor Chunks Split**: Heavy third-party packages (e.g. `react`, `react-router-dom`, `chart.js`) are split into a separate `vendor` chunk.
*   **Dynamic Imports**: Page routes are loaded dynamically using import statements, allowing browsers to fetch bundles only when a route is accessed.

---

## 4. Asset & Image Optimizations

*   **Image Formats**: Avatars must be saved in modern, compressed formats (`WebP` or `PNG`).
*   **Dynamic Resizing**: Avatars are requested with size parameters (e.g., `width=80&height=80`) to minimize download sizes on mobile layouts.

---

*End of Document — Performance Optimization*
