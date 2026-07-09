# 04 — Layout System

> **Document ID**: ARC-FE-LAY-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Responsive layout shells and outlet configurations

---

## 1. Layout Shell Architecture

The application uses three layout templates to wrap routed page views: **GuestLayout**, **StudentLayout**, and **AdminLayout**. These templates use React Router's `<Outlet />` element to render nested routes inside structural page wireframes.

---

## 2. Layout Layout Templates

---

### 2.1 GuestLayout
Provides a minimal, centered container for guest authentication forms.

*   **Structure**:
    ```tsx
    // Logical hierarchy (non-implementation code)
    <GuestContainer>
      <LanguageSwitcher />
      <GlassmorphicCard>
        <Outlet />
      </GlassmorphicCard>
    </GuestContainer>
    ```
*   **Visual Parameters**: Uses background transitions, centering flex alignments, and responsive paddings.

---

### 2.2 StudentLayout
The main shell for authenticated student operations, providing sidebar navigation and top headers.

*   **Structure**:
    ```tsx
    <LayoutWrapper>
      <Sidebar />
      <ContentArea>
        <Header />
        <MainScrollable>
          <Outlet />
        </MainScrollable>
      </ContentArea>
      <MobileTabBar />
    </LayoutWrapper>
    ```
*   **Visual Parameters**:
    *   `Sidebar`: Fixed left positioning (`fixed w-64 h-full`). Collapses into an overlay drawer on mobile viewports.
    *   `Header`: Sticky top positioning (`sticky top-0 h-16 z-30`).
    *   `MobileTabBar`: Sticky bottom positioning (`fixed bottom-0 z-30`). Visible only on mobile screens ($<640\text{px}$).

---

### 2.3 AdminLayout
The administrative layout shell, optimized for table grids and directory navigation.

*   **Structure**:
    *   Contains the Admin Sidebar (Student directory links, notifications broadcasts, settings configuration) and the Main Scrollable area.
    *   Does not render the mobile tab bar, as admin actions are optimized for desktop use.

---

*End of Document — Layout System*
