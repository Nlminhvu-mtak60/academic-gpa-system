# 16 — Error Pages

> **Document ID**: UX-ERR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Error screen layouts and recovery navigation

---

## 1. Document Purpose

This document defines page layouts and copy variations for system error pages.

---

## 2. Error Page Designs

---

### 2.1 404 — Page Not Found
Displayed when a user navigates to an invalid URL or attempts to access a deleted resource.

```
+-------------------------------------------------------------+
|                                                             |
|                       [ ! ] (404)                           |
|                                                             |
|                    Page Not Found                           |
|              Rất tiếc, trang này không tồn tại.             |
|                                                             |
|  We couldn't find the page you were looking for. It might   |
|  have been moved, deleted, or never existed.                |
|                                                             |
|  [ Back to Dashboard ]             [ Report an Issue ]      |
|                                                             |
+-------------------------------------------------------------+
```

*   **Illustration Placement**: Centers a custom SVG illustration of a student searching through a stack of folders.

---

### 2.2 403 — Access Denied (Forbidden)
Displayed when a student tries to access admin routes or view another student's private transcript.

```
+-------------------------------------------------------------+
|                                                             |
|                       [ X ] (403)                           |
|                                                             |
|                    Access Denied                            |
|             Bạn không có quyền truy cập trang này.          |
|                                                             |
|  You do not have the required permissions to view this      |
|  resource. Please verify your login credentials or contact  |
|  the system administrator.                                  |
|                                                             |
|  [ Back to Dashboard ]                 [ Contact Admin ]    |
|                                                             |
+-------------------------------------------------------------+
```

*   **Illustration Placement**: Centers a custom SVG illustration of a locked catalog drawer.

---

### 2.3 500 — Internal Server Error
Displayed when the API encounters an unhandled exception.

```
+-------------------------------------------------------------+
|                                                             |
|                       [ ? ] (500)                           |
|                                                             |
|                Internal Server Error                        |
|               Đã xảy ra lỗi hệ thống.                       |
|                                                             |
|  Something went wrong on our servers. We have logged the     |
|  issue and are working to fix it. Please try again later.   |
|                                                             |
|  [ Refresh Page ]                  [ Back to Dashboard ]    |
|                                                             |
+-------------------------------------------------------------+
```

*   **Illustration Placement**: Centers a custom SVG illustration of an academic advisor repairing a clockwork mechanism.

---

*End of Document — Error Pages*
