# 15 — Notification Inbox Pages

> **Document ID**: UX-NOTIF-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Notification dashboard list panels and card definitions

---

## 1. Document Purpose

This document defines page layouts and UI behaviors for the Notification Inbox module.

---

## 2. Notification Page Layouts

---

### 2.1 Notification Inbox Page
The main workspace where students view and manage notifications.

```
+-------------------------------------------------------------+
| Notifications Inbox                                         |
+-------------------------------------------------------------+
|  [ Tabs: Unread (2) ]   [ All ]               [ Mark all read]
|                                                             |
|  +-------------------------------------------------------+  |
|  | (•) AI System Upgrade Scheduled                       |  |
|  |     System Alert - 24/06/2026 10:00 AM                |  |
|  |     We will perform scheduled updates on LLM...       |  |
|  |     [ Mark as read ]                         [ Delete ]  |
|  +-------------------------------------------------------+  |
|  | (•) GPA Milestone Achieved!                           |  |
|  |     Milestone - 23/06/2026 04:30 PM                   |  |
|  |     Congratulations! Your GPA reached 8.05...         |  |
|  |     [ Mark as read ]                         [ Delete ]  |
|  +-------------------------------------------------------+  |
|  |     Welcome to Academic GPA Management                |  |
|  |     Welcome - 20/06/2026 09:00 AM                     |  |
|  |     Your account is verified and ready...             |  |
|  |                                              [ Delete ]  |
|  +-------------------------------------------------------+  |
|                                                             |
|  Pagination: < Page 1 of 1 >                                 |
+-------------------------------------------------------------+
```

*   **Status Indicators**: Unread items display a blue dot indicator (`•`) in the left margin.
*   **Actions**:
    *   Clicking a notification card expands the details and marks the item as read.
    *   Hovering over an unread item reveals a quick "Mark as read" check button.

---

### 2.2 Notification Dropdown Panel
A dropdown window that opens when clicking the header notification bell icon.

*   **Dimensions**: Width: `320px`, Max Height: `400px` (with vertical scroll).
*   **Header**: Displays "Recent Notifications" and a "Mark all read" link.
*   **Item Rows**: Displays the 5 most recent unread notifications (compact title and relative timestamp).
*   **Footer**: A button to "View All Notifications" that redirects the user to the main Inbox page.

---

### 2.3 System Alert Cards vs. Admin Broadcast Messages
*   **System Alert Card**: Colored icon prefix (e.g. green circle checkmark for achievements) and system-formatted text.
*   **Admin Broadcast Card**: User avatar icon of the sending admin, subject line, and HTML-rendered markdown content box.

---

*End of Document — Notification Inbox Pages*
