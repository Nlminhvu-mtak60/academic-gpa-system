# 10 — Dashboard Design

> **Document ID**: UX-DASH-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Dashboard interface wireframes and layout structures

---

## 1. Document Purpose

This document provides wireframe layouts and interface designs for both the **Student Portal Dashboard** and the **Admin Management Console Dashboard**.

---

## 2. Student Portal Dashboard Wireframe

The student landing page focuses on key academic metrics, performance trends, and goal tracking.

```
+---------------------------------------------------------------------------------+
| Sidebar   | [Menu] Student Dashboard                      [VI/EN] [Theme] [Bell]|
|           +---------------------------------------------------------------------+
|           |                                                                     |
|           |  ROW 1: KEY PERFORMANCE INDICATOR CARDS (3 Columns)                 |
|           |  +-------------------+ +-------------------+ +-------------------+  |
|           |  | Cumulative GPA    | | Credit Progress   | | Active Goal Status|  |
|           |  |      7.42 / 10    | |  45 / 120 credits | | Target: 8.00 GPA  |  |
|           |  |      3.05 / 4     | |  [=====>------]  | | Required: 8.35   |  |
|           |  +-------------------+ +-------------------+ +-------------------+  |
|           |                                                                     |
|           |  ROW 2: VISUAL ANALYTICS PANELS (2 Columns - 2:1 Split)             |
|           |  +---------------------------------+ +---------------------------+  |
|           |  | GPA Trend Line Chart            | | Grade Distribution Pie    |  |
|           |  | (Sem 1 -> Sem 2 -> Sem 3)       | | [ Donuts: A+, A, B+, B ]  |  |
|           |  | [ Chart Canvas Area ]           | |                           |  |
|           |  +---------------------------------+ +---------------------------+  |
|           |                                                                     |
|           |  ROW 3: RECENT ACTIVITIES & QUICK LINKS (1 Column)                  |
|           |  +---------------------------------------------------------------+  |
|           |  | Current Semester Courses Overview                             |  |
|           |  | [Code]   [Course Name]      [Credits]   [Final Grade]  [Status]  |  |
|           |  | CS101    Intro to Comp Sci  3 credits   8.5 (A)        Passed    |  |
|           |  | MATH101  Calculus 1         4 credits   5.5 (C)        Passed    |  |
|           |  +---------------------------------------------------------------+  |
|           |                                                                     |
+---------------------------------------------------------------------------------+
```

---

## 3. Admin Console Dashboard Wireframe

The administrative dashboard provides system-wide telemetry, user logs, and quick access to account configurations.

```
+---------------------------------------------------------------------------------+
| Sidebar   | [Menu] Admin Dashboard                        [VI/EN] [Theme]       |
|           +---------------------------------------------------------------------+
|           |                                                                     |
|           |  ROW 1: SYSTEM TELEMETRY CARDS (4 Columns)                          |
|           |  +---------+ +---------+ +-----------------------+ +-------------+  |
|           |  | Total   | | Active  | | System Average GPA | | Pending     |  |
|           |  | Students| | Accounts| | 10-scale: 7.15      | | Broadcasts  |  |
|           |  |  1,420  | |  1,395  | | 4-scale:  2.85      | | 2 active  |  |
|           |  +---------+ +---------+ +-----------------------+ +-------------+  |
|           |                                                                     |
|           |  ROW 2: ANALYTICS & REGISTRATIONS (2 Columns - 1:1 Split)           |
|           |  +---------------------------------+ +---------------------------+  |
|           |  | GPA Cohort Distribution         | | Daily Student Logins      |  |
|           |  | (Histogram count bins)          | | (Daily chart activity)    |  |
|           |  | [ Histogram Chart Area ]        | | [ Bar Chart Area ]        |  |
|           |  +---------------------------------+ +---------------------------+  |
|           |                                                                     |
|           |  ROW 3: STUDENT SEARCH & ACCESS GRID (1 Column)                     |
|           |  +---------------------------------------------------------------+  |
|           |  | Student Account Access Registry                               |  |
|           |  | [MSSV]   [Student Name]     [Email]         [Status]   [Actions]  |  |
|           |  | 2024001  Nguyen Van A       a@domain.com    Active     [Lock]     |  |
|           |  | 2024002  Tran Thi B         b@domain.com    Locked     [Unlock]   |  |
|           |  +---------------------------------------------------------------+  |
|           |                                                                     |
+---------------------------------------------------------------------------------+
```

---

## 4. Layout Placement & Responsive Rules

1.  **Dashboard Content Width**: Capped at `1440px` to prevent layouts from stretching on wide monitors.
2.  **Collapsible Rows**: On mobile devices ($<640\text{px}$), columns wrap and stack vertically. Line and pie charts stack to fit mobile screens.
3.  **Metrics Font**: Uses `Outfit Bold` for numeric displays (`28px` to `36px`), styled to stand out on the page.

---

*End of Document — Dashboard Design*
