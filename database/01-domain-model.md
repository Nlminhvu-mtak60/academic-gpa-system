# 01 — Domain Model

> **Document ID**: DB-DM-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Domain entities and boundary definitions

---

## 1. Document Purpose

This document defines the core domain models and logical boundaries of the Academic GPA Management System. It translates the business requirements of the GPA management platform into structural database entities, detailing the purpose and scope of each domain model.

---

## 2. Domain Entities & Boundaries

The database is divided into 5 logical subject areas: **Identity & Access**, **Academic Records**, **GPA & Planning**, **AI & Notifications**, and **System & Auditing**.

```
System Domain Subject Areas
├── [Identity & Access]
│   ├── User (Account access credentials & metadata)
│   └── RefreshToken (Token rotation & session state)
├── [Academic Records]
│   ├── StudentProfile (University specific student registry)
│   ├── AcademicYear (Timeline bounds for semesters)
│   ├── Semester (Term subdivision containing courses)
│   ├── Course (Credit volume, course code, retake links)
│   └── Score (Component grades and calculation outputs)
├── [GPA & Planning]
│   ├── GpaGoal (Target GPA milestones and achievability status)
│   └── SharedTranscript (Public viewing tokens and validation dates)
├── [AI & Notifications]
│   ├── AiConversation (Advisor chat thread details)
│   ├── AiMessage (Individual message exchanges)
│   └── Notification (System and admin notifications)
└── [System & Auditing]
    ├── ScoreAuditLog (Trace log tracking grade modifications)
    └── SystemSetting (Application parameter flags)
```

---

## 3. Entity Definitions

### 3.1 Identity & Access
*   **User**: Represents any person authenticated by the system. It holds login details (hashed password, Google unique identity ID), active status flags, locking metadata, and user interface preferences (Language, Dark Mode toggle).
*   **RefreshToken**: Secures stateless sessions. Every user can have multiple active refresh tokens linked to their devices. It tracks rotation history (`ReplacedByToken`) and expiration.

### 3.2 Academic Records
*   **StudentProfile**: Holds university-specific information for users with the Student role. Links the generic User table to their Student Code (MSSV), Major, University Name, Enrollment Year, and Total Required Credits.
*   **AcademicYear**: Logical container grouping semesters (e.g. "2024-2025"). Owned by a specific Student Profile.
*   **Semester**: Logical division of an academic year (e.g. "Semester 1", "Summer"). Constrained to a maximum of 3 semesters per academic year.
*   **Course**: An academic class taken by a student. Holds credit volume (1-6) and tracks if it is a retake of a previous course attempt (`OriginalCourseId`).
*   **Score**: Holds raw component scores (Attendance, Continuous, Final Exam) and final calculated outputs (Course Score, Letter Grade, GPA-4 value). It maintains a 1:1 relationship with its parent Course.

### 3.3 GPA & Planning
*   **GpaGoal**: Represents a student's active or historic academic target. Tracks target cumulative GPAs and calculates the feasibility based on completed and remaining credits.
*   **SharedTranscript**: Governs public transcript sharing. It contains cryptographically random token links (UUIDs), expiration dates, and view counts.

### 3.4 AI & Notifications
*   **AiConversation**: Represents a chat session with the AI Academic Advisor.
*   **AiMessage**: Individual chat bubbles (user prompts or AI responses) linked to a conversation thread.
*   **Notification**: Handles transactional notifications (e.g., target GPA achievements) and admin announcements.

### 3.5 System & Auditing
*   **ScoreAuditLog**: An insert-only trace log tracking modifications to the `Scores` table for accountability and grading integrity.
*   **SystemSetting**: Key-value settings store for global parameters (e.g., system-wide maintenance mode flags).

---

*End of Document — Domain Model*
