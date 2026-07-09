# 14 — Settings & Profile Pages

> **Document ID**: UX-SETT-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Settings form wireframes and interface preferences selectors

---

## 1. Document Purpose

This document defines page layouts and settings configurations for student profiles and preferences.

---

## 2. Settings Layouts & Panels

---

### 2.1 Profile Details Editor

Allows students to update their personal and academic information.

```
+-------------------------------------------------------------+
| Personal & Academic Settings                                |
+-------------------------------------------------------------+
|  Avatar Image                                               |
|  [ (NV) ] Nguyen Van A                                      |
|  [ Change Avatar ]   [ Remove ]                             |
|                                                             |
|  Student Identification Number (MSSV)                       |
|  [ 2024001                                               ]  |
|                                                             |
|  University Name                                            |
|  [ University of Technology                              ]  |
|                                                             |
|  Major Study Field                                          |
|  [ Computer Science                                      ]  |
|                                                             |
|  Enrollment Year                                            |
|  [ 2024                                                  ]  |
|                                                             |
|  [ Cancel Changes ]                         [ Save Profile ]|
+-------------------------------------------------------------+
```

*   **Behavior**: Validation restricts input ranges (e.g. Enrollment Year between 2000 and 2100).

---

### 2.2 UI Preferences Panel
Allows users to configure theme and language settings:

*   **Bilingual Switcher**:
    *   Horizontal button group: `[ Tiếng Việt ]` / `[ English ]`.
    *   Clicking a language applies translations instantly across all UI components and requests a language preference update from the API.
*   **Theme Switcher**:
    *   Horizontal button group: `[ Light Mode ]` / `[ Dark Mode ]`.
    *   Changes styling classes at the root document level, modifying color tokens dynamically.

---

### 2.3 Password Management
A standalone form card on the settings page:

*   **Inputs**:
    *   `Current Password`
    *   `New Password`
    *   `Confirm New Password`
*   **Validation**: Verifies that the new password is different from the current password. Saves changes securely.

---

*End of Document — Settings & Profile Pages*
