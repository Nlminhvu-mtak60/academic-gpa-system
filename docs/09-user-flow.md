# 09 — User Flow

> **Document ID**: SRS-UF-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Textual workflows and Mermaid sequence/activity diagrams

---

## 1. Document Purpose

This document outlines the end-to-end user flows for both **Student** and **Admin** actors within the Academic GPA Management System. It tracks how users navigate typical business processes, illustrating decision points, error paths, and data modifications.

---

## 2. Student Flows

### Flow SF-1: Registration, Verification, and Profile Setup
This flow details how a new student joins the platform, verifies their account, and completes their profile.

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant UI as Client Web App
    participant API as ASP.NET Core API
    participant DB as SQL Server
    participant Email as SMTP Server

    Student->>UI: Enter Registration Info (Email, Password, Name)
    UI->>API: POST /api/v1/auth/register
    API->>DB: Check email uniqueness
    alt Email already exists
        DB-->>API: Conflict (Email exists)
        API-->>UI: 409 Conflict Response
        UI-->>Student: Display error: "An account with this email already exists."
    else Email is unique
        API->>DB: Create User record (IsActive=true, IsEmailVerified=false)
        API->>API: Generate 24-hour verification token
        API->>Email: Send verification email with token
        API-->>UI: 201 Created Response
        UI-->>Student: Display success message: "Please check your email to verify your account."
    end

    Student->>Email: Click verification link
    Email->>UI: Redirect to verify page with token
    UI->>API: POST /api/v1/auth/verify-email { token }
    API->>DB: Find token, verify validity
    alt Token expired/invalid
        API-->>UI: 400 Bad Request
        UI-->>Student: Display error: "Verification link expired or invalid."
    else Token valid
        API->>DB: Set IsEmailVerified=true
        API-->>UI: 200 OK Response
        UI-->>Student: Redirect to Login Page
    end

    Student->>UI: Login & Access profile setup
    UI->>API: PUT /api/v1/students/profile { StudentCode, UniversityName, MajorName, EnrollmentYear, TotalRequiredCredits }
    API->>DB: Create/Update StudentProfile record
    API-->>UI: 200 OK Response
    UI-->>Student: Onboarding complete, redirect to Dashboard
```

---

### Flow SF-2: Academic Record Entry & Calculation
This flow represents the typical lifecycle of entering grades and viewing recalculated GPAs.

```mermaid
activityDiagram
    start
    :User navigates to Semesters Page;
    if (Academic Year exists?) then (no)
        :Create Academic Year (e.g., "2024-2025");
        :Save Year to Database;
    endif
    if (Semester exists in selected Year?) then (no)
        :Create Semester (e.g., "Semester 1");
        :Save Semester to Database;
    endif
    :Select Semester & Click "Add Course";
    :Enter Course Code, Name, Credits (1-6);
    :Save Course;
    :Input Component Scores (Attendance, Continuous, Final Exam);
    if (All scores entered?) then (yes)
        :Apply nearest 0.5 rounding to each component;
        :Calculate Course Score: A*0.1 + C*0.3 + F*0.6;
        :Round Course Score to 1 decimal place;
        :Determine Letter Grade & GPA-4 Value;
        :Save Scores;
        :Trigger GPA Recalculation Engine;
        :Update Semester GPA (10-scale and 4-scale);
        :Update Cumulative GPA;
        :Update Academic Classification;
        :Display updated GPAs on UI;
    else (no)
        :Save partial scores;
        :Course Score and GPA remain null ("-");
        :Display scores on UI;
    endif
    stop
```

---

### Flow SF-3: Goal Planning & simulation
This flow outlines how a student sets a target GPA and uses the system to plan their goals.

1. **Setting Target Goal**:
   - The student navigates to the **Goal Planner**.
   - Input target Cumulative GPA (e.g., `8.2` on a 10-scale).
   - The API evaluates the feasibility:
     - `Current Cumulative GPA` = $C$
     - `Credits Completed` = $W_c$
     - `Target Cumulative GPA` = $T$
     - `Estimated Remaining Credits` = $W_r$
     - Required GPA for remaining credits ($R$) is calculated as:
       $$R = \frac{(T \times (W_c + W_r)) - (C \times W_c)}{W_r}$$
     - If $R > 10.0$: Warn the student that the goal is statistically impossible.
     - If $R \le 0$: Advise student that goal is already met.
     - Otherwise: Show target required semester GPA.
   - Student saves the goal.

2. **Goal Progress Auditing**:
   - Every time a course score is updated, the GPA calculation engine updates the cumulative GPA.
   - If cumulative GPA matches/exceeds target GPA, update `GpaGoal.IsAchieved = true`.
   - Dispatch system notification to the student.

---

### Flow SF-4: AI Academic Advisor Interaction
This flow details how the student communicates with the AI Advisor securely.

```mermaid
sequenceDiagram
    autonumber
    actor Student
    participant UI as Client Web App
    participant API as ASP.NET Core API
    participant AI as FastAPI Service
    participant LLM as OpenAI/Gemini API

    Student->>UI: Open AI Advisor chat, type question
    UI->>API: POST /api/v1/ai-advisor/chat { conversationId, message }
    API->>API: Check rate limit (max 20 msgs/hour)
    alt Rate limit exceeded
        API-->>UI: 429 Too Many Requests
        UI-->>Student: Display "Message limit reached. Try again in X mins."
    else Within limit
        API->>API: Query student academic records (Anonymized)
        Note over API: Extracts course codes, credits, scores, GPA trends.<br/>No Student Names/Emails (PII) included.
        API->>AI: POST /ai/advisor/analyze { message, academicData, preferredLanguage }
        AI->>AI: Build system prompt with student context and rules
        AI->>LLM: Send prompt (formatted JSON)
        LLM-->>AI: Generate response content
        AI-->>API: Return AI response text
        API->>API: Save messages in AI_MESSAGES table
        API-->>UI: 200 OK Response { responseText }
        UI-->>Student: Display advisor message in chat interface
    end
```

---

### Flow SF-5: Transcript Sharing
This flow shows the creation and consumption of shared public transcripts.

1. **Link Generation**:
   - Student clicks "Share Transcript".
   - Selects Expiry Duration (e.g., 7 days, 30 days, or Never).
   - API creates `SHARED_TRANSCRIPTS` record, generating a cryptographically secure `ShareToken` (UUID v4).
   - API returns the URL: `https://gpa.domain.com/shared/{uuid}`.
   - Student copies link and shares it.

2. **Link Verification & Viewing**:
   - Anonymous Viewer (e.g., recruiter) navigates to URL.
   - Client sends token to API: `GET /api/v1/transcripts/shared/{uuid}`.
   - API checks if link exists, is not revoked, and is not expired:
     - If invalid/expired: Return `404 Not Found` with custom error message: "This shared transcript has expired or does not exist."
     - If valid: Increment `ViewCount`, retrieve student transcript data (academic years, semesters, courses, scores, GPAs).
   - API returns anonymized transcript (name replaced with initials or student code only, based on privacy settings).
   - UI renders premium read-only transcript dashboard.

---

## 3. Admin Flows

### Flow AF-1: Student Monitoring and Account Administration
Allows system operators to inspect and lock accounts violating terms.

```mermaid
sequenceDiagram
    autonumber
    actor Admin
    participant UI as Admin Portal
    participant API as ASP.NET Core API
    participant DB as SQL Server

    Admin->>UI: Search for student (by name, email, or StudentCode)
    UI->>API: GET /api/v1/admin/students?search=XYZ
    API->>DB: Query USERS + STUDENT_PROFILES with filters
    DB-->>API: Return filtered lists
    API-->>UI: 200 OK Response
    UI-->>Admin: Render student list grid

    Admin->>UI: Click on Student details
    UI->>API: GET /api/v1/admin/students/{id}
    API->>DB: Query academic records (read-only)
    DB-->>API: Return complete transcript & profile details
    API-->>UI: 200 OK Response
    UI-->>Admin: Show detailed read-only transcript view

    alt Lock Student Account
        Admin->>UI: Click "Lock Account" & Enter reason
        UI->>API: POST /api/v1/admin/students/{id}/lock { reason }
        API->>DB: Set IsActive=false, LockedAt=UtcNow, LockReason=reason
        API->>DB: Revoke all active Refresh Tokens for student
        API-->>UI: 200 OK Response
        UI-->>Admin: Account status updated to Locked
    end
```

---

### Flow AF-2: System Broadcast Notification
Allows administrators to send announcements or policy updates.

1. **Create Broadcast**:
   - Admin navigates to Admin Notifications dashboard.
   - Enters Title (e.g., "AI System Upgrade") and Message content.
   - Selects "All Students" as target.
   - Admin clicks "Send Broadcast".
2. **Delivery & Persistence**:
   - Admin Client sends `POST /api/v1/admin/notifications/broadcast` to API.
   - API writes a record to `NOTIFICATIONS` table with `IsBroadcast = true`.
   - The notification is marked as unread for each student when they poll.
   - Next time any active Student logs in or polls (every 30s in foreground), the Client requests `GET /api/v1/notifications/unread`.
   - API includes both individual messages and active broadcast messages.
   - Client displays real-time badge count and notification toast.

---

*End of Document — User Flow*
