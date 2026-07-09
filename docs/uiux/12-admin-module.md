# 12 — Admin Module Pages

> **Document ID**: UX-ADMIN-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Admin dashboard layouts and management workflows

---

## 1. Document Purpose

This document defines page layouts and workflows for the Admin module.

---

## 2. Page Layouts & Workflows

---

### 2.1 Student Management Directory Page
The primary directory for system administrators.

```
+---------------------------------------------------------------------------------+
| Student Account Directory                                                       |
+---------------------------------------------------------------------------------+
|  [ Search by Name or Student ID...    ]   [ Filter Status: All/Active/Locked v ] |
|                                                                                 |
|  +---------------------------------------------------------------------------+  |
|  | Student ID | Full Name     | Email         | Status    | Actions          |  |
|  +------------+---------------+---------------+-----------+------------------+  |
|  | 2024001    | Nguyen Van A  | a@domain.com  | Active    | [View] [Lock]    |  |
|  | 2024002    | Tran Thi B    | b@domain.com  | Locked    | [View] [Unlock]  |  |
|  +---------------------------------------------------------------------------+  |
|                                                                                 |
|  Pagination: < Page 1 of 12 > (20 items per page)                               |
+---------------------------------------------------------------------------------+
```

*   **Search Behavior**: Triggers query requests automatically after 300ms of user typing (using a debounce hook).
*   **Access Controls**: Clicking "View" opens a read-only profile drawer. Clicking "Lock" opens the lock confirmation modal.

---

### 2.2 Student Detail Drawer (Read-Only View)
A slide-out drawer (`fixed right-0 h-full w-[600px]`) showing the student's profile.

*   **Student Header**: Displays the student's avatar, initials, MSSV, and status badge.
*   **Summary Statistics**: Shows cumulative GPAs, completed credits, and active goals.
*   **Transcript Accordion**: Lists semesters and courses in read-only tables. Administrators cannot edit, delete, or add courses.
*   **Account Controls**:
    *   "Lock/Unlock Account" toggle button.
    *   "Reset Password" button (generates a temporary password).

---

### 2.3 Account Lock Confirmation Modal
A confirmation dialog required before locking a student account.

```
+-------------------------------------------------------------+
| Lock Student Account: Nguyen Van A (2024001)                |
+-------------------------------------------------------------+
|                                                             |
|  Are you sure you want to lock this account?                |
|  Locked students will be logged out immediately and blocked  |
|  from signing in.                                           |
|                                                             |
|  Reason for Lock:                                           |
|  [ Input reason details here...                           ] |
|                                                             |
|  [ Cancel ]                                  [ Lock Account]|
+-------------------------------------------------------------+
```

*   **Validation**: The "Lock Account" button remains disabled until the administrator enters a lock reason ($\ge 10$ characters).

---

### 2.4 Broadcast Announcement Panel
A composer tool used to send system-wide notifications.

*   **Announcement Editor Form**:
    *   **Notification Title**: Text field (max 100 characters).
    *   **Notification Message**: Rich text editor (max 2000 characters).
    *   **Delivery Mode Selectors**: Toggles between in-app notification and email delivery.
*   **Preview Card**: Displays how the notification will look to students.
*   **Target Scope Toggles**: Selects target recipients (e.g. "All Students", "Struggling Students with GPA < 5.0").

---

*End of Document — Admin Module Pages*
