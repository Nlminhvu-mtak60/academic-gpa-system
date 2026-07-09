# 09 — UI/UX User Flows

> **Document ID**: UX-FLOW-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: UI navigation flows and screen transitions

---

## 1. Document Purpose

This document maps out screen-to-screen user flows and state transitions for the primary features of the application.

---

## 2. Core User Journey Flows

---

### Flow 1: Registration & Profile Onboarding

```
[Start]
  │
  ▼
[Register Page] ──► Input invalid entries ──► Display input validations
  │
  ├─► Click "Register" (Valid inputs)
  ▼
[Email Verify Link Page] ──► Send verification email
  │
  ├─► User clicks verification link
  ▼
[Login Page] ──► Authenticate user
  │
  ├─► First Login Redirect
  ▼
[Onboarding Sheet] ──► Enter student code, major, required credits
  │
  ├─► Save Profile details
  ▼
[Student Dashboard] (Onboarding complete)
```

#### Steps:
1.  **Register Page**: Student enters name, email, and password. Real-time Zod validations check inputs.
2.  **Email Verify**: Registration redirects to a pending screen, and the API sends a verification email.
3.  **Onboarding Sheet**: After logging in for the first time, a full-screen onboarding sheet appears, prompting the student to enter their university, major, student code (MSSV), and graduation credit target.
4.  **Dashboard Access**: Saving the profile redirects the user to their main dashboard.

---

### Flow 2: Score Entry & GPA Recalculation

```
[Student Dashboard]
  │
  ├─► Click "Semesters" link in Sidebar
  ▼
[Semesters Directory Page]
  │
  ├─► Click "Semester Card" or "Add Semester"
  ▼
[Semester Detail View] ──► Course Grid Table
  │
  ├─► Click "Add Course" Button
  ▼
[Course Input Modal] ──► Enter Course code, name, credits (1-6)
  │
  ├─► Save Course
  ▼
[Semester Detail View] (New course added to table)
  │
  ├─► Click "Input Scores" in Course Table row
  ▼
[Score Entry Panel] ──► Enter component scores (Attendance, Continuous, Final Exam)
  │
  ├─► Click "Save Scores"
  ▼
[Recalculation Trigger] ──► Rounded scores, final grade, letter grade display
```

#### Steps:
1.  **Semesters Directory**: The student clicks "Semesters" in the sidebar to view their academic history grouped by year.
2.  **Semester Detail**: Selecting a semester opens a card grid of courses with input tables.
3.  **Add Course Modal**: Prompts for course details (Code, Name, Credits).
4.  **Score Input**: The student enters attendance, continuous, and exam scores. Real-time validation limits entries to `0.0` - `10.0`.
5.  **Recalculation**: Clicking "Save" calculates the final grade, letter grade, and GPA, updating all metrics on the page.

---

### Flow 3: Target Setting & Goal Simulation

1.  **Navigate**: The student clicks "Goal Planner" in the sidebar.
2.  **Goal Setup Card**: Displays the current goal (if set) and goal progress metrics.
3.  **Simulation Panel**: The student enters a target GPA (e.g. `8.0`). The system checks feasibility and displays the required average GPA for remaining semesters.
4.  **"What-If" Sliders**: The student can adjust slider values to simulate grade scenarios for the current semester.

---

### Flow 4: AI Advisor Chat Integration

1.  **Access Chat**: The student clicks "AI Advisor" in the sidebar.
2.  **Sidebar Threads**: Lists past conversations (up to 50 threads). The student can click a thread to resume chatting or click "New Chat".
3.  **Anonymized Context**: The API fetches the student's academic history, anonymizes it, and sends it to the AI advisor.
4.  **Chat Interface**: The student type their questions. Streaming text responses make the conversation feel fast and fluid.

---

*End of Document — UI/UX User Flows*
