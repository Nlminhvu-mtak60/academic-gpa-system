# 11 — Student Module Pages

> **Document ID**: UX-STUDENT-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Student portal view designs and modal layouts

---

## 1. Document Purpose

This document defines page designs, layouts, and modals for the Student module.

---

## 2. Pages Design & Layouts

---

### 2.1 Semesters & Courses Directory View
Displays a list of academic years. Selecting a card reveals the semesters and courses within that year.

*   **Year Selector Grid**: Displays academic years as Cards containing start/end years, total credits completed, and cumulative GPA.
*   **Semester Accordion Panel**: Clicking a year card expands an accordion showing its semesters.
*   **Course Data Table**: Inside each expanded semester accordion:
    *   Columns: `Code`, `Course Name`, `Credits`, `Attendance`, `Continuous`, `Final Exam`, `Grade`, `Actions`.
    *   Action Buttons: "Edit Scores" (opens a dialog) and "Delete Course" (triggers a confirmation modal).

---

### 2.2 Course Score Input Modal
A modal dialog opened by clicking "Edit Scores" in a course row.

```
+-------------------------------------------------------------+
| Edit Component Scores: CS101 - Intro to Computer Science    |
+-------------------------------------------------------------+
|                                                             |
|  Attendance Assessment (Weight: 10%)                        |
|  [ 9.0      ]  (Valid input range: 0.0 - 10.0)              |
|                                                             |
|  Continuous Assessment (Weight: 30%)                        |
|  [ 8.5      ]                                               |
|                                                             |
|  Final Exam Score (Weight: 60%)                             |
|  [ 7.5      ]                                               |
|                                                             |
|  ---------------------------------------------------------  |
|  Calculated Estimation: 7.9 (B) - 3.0 GPA                   |
|                                                             |
|  [ Cancel ]                                  [ Save Scores ]|
+-------------------------------------------------------------+
```

*   **Real-time Validation**: Focus states trigger error tooltips if entries exceed `10.0`.
*   **Real-time Output Preview**: The modal footer previews the calculated score and letter grade dynamically as the student types.

---

### 2.3 Goal Planner & Prediction Panel
A two-column dashboard layout for tracking target GPAs:

*   **Left Column (Goal Progress Ring)**:
    *   Displays a large SVG progress ring.
    *   Shows the target GPA in the center (e.g. `8.00`), the current GPA (`7.42`), and the remaining credits indicator.
    *   **Achievability Alert**: Shows semantic status messages (e.g., "Achievable: You need an average GPA of 8.35 in your remaining semesters").
*   **Right Column ("What-If" Simulator)**:
    *   Allows students to simulate hypothetical grades.
    *   Provides horizontal slider controls for Attendance, Continuous, and Final scores.
    *   As sliders are adjusted, the simulated semester GPA updates instantly.

---

### 2.4 Transcript Share Link Dialog
A modal dialog for managing public share links.

```
+-------------------------------------------------------------+
| Share Public Academic Transcript                            |
+-------------------------------------------------------------+
|                                                             |
|  Link Expiry Duration                                       |
|  ( ) 7 Days    (o) 30 Days    ( ) 90 Days    ( ) Never      |
|                                                             |
|  [ Generate Secure Share Link ]                             |
|                                                             |
|  Generated URL:                                             |
|  [ https://gpa.app/shared/f3a8b2c4-84d2-45e0-b999... ] [Copy]|
|                                                             |
|  ---------------------------------------------------------  |
|  Active Share Links History:                                |
|  - Token f3a8... Created: 24/06/2026 Expires: 24/07/2026 [Revoke]|
|                                                             |
|                                                     [Close] |
+-------------------------------------------------------------+
```

*   **Actions**:
    *   Clicking "Copy" updates the button label to "Copied!" for 2 seconds.
    *   Clicking "Revoke" displays a confirmation alert to invalidate access immediately.

---

*End of Document — Student Module Pages*
