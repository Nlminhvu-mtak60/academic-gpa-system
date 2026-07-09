# 16 — Glossary

> **Document ID**: SRS-GLOS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Dictionary format with terms, definitions, and contextual examples

---

## 1. Document Purpose

This document provides a single source of truth for terminology, abbreviations, and acronyms used throughout the Academic GPA Management System's documentation. It ensures clear communication among business analysts, database architects, developers, and QA engineers.

---

## 2. Academic Terminology

### Attendance Score (Điểm chuyên cần)
*   **Definition**: The component score representing a student's class attendance and participation.
*   **Context**: Accounts for exactly 10% of the overall course grade in the standard formula. Must be between 0.0 and 10.0 inclusive.

### Continuous Assessment Score (Điểm quá trình / Điểm giữa kỳ)
*   **Definition**: The component score aggregating homework, quizzes, midterm exams, and laboratory activities.
*   **Context**: Accounts for exactly 30% of the overall course grade in the standard formula. Must be between 0.0 and 10.0 inclusive.

### Final Exam Score (Điểm thi kết thúc học phần)
*   **Definition**: The score earned on the comprehensive exam at the end of a semester.
*   **Context**: Accounts for exactly 60% of the overall course grade in the standard formula. Must be between 0.0 and 10.0 inclusive.

### Course Score (Điểm học phần)
*   **Definition**: The final calculated grade for a course on a 10-point scale, combining component scores.
*   **Context**: Calculated as: `Attendance * 0.1 + Continuous * 0.3 + Final * 0.6`. Rounded to 1 decimal place.

### Credits (Tín chỉ)
*   **Definition**: The weight or volume of academic load assigned to a course.
*   **Context**: Restricted to an integer between 1 and 6 inclusive in this system.

### Letter Grade (Điểm chữ)
*   **Definition**: The alphabetical representation of a student's performance in a course.
*   **Context**: Mapped from the 10-scale course score. Tiers include A+ (Excellent), A, B+, B, C+, C, D+, D (Passing), and F (Fail).

### GPA-10 Scale (Thang điểm 10)
*   **Definition**: The primary academic scoring system used in Vietnamese universities, spanning from 0.0 to 10.0.
*   **Context**: Used to enter course scores and calculate the primary cumulative average.

### GPA-4 Scale (Thang điểm 4)
*   **Definition**: The international standard grading scale, spanning from 0.0 to 4.0.
*   **Context**: Calculated alongside the 10-scale. Used for scholarship audits and study abroad applications.

### Semester GPA (GPA Học kỳ)
*   **Definition**: The weighted average of all course scores earned within a specific semester.
*   **Context**: Calculated by dividing the sum of (Course Score * Credits) by the total credits of the semester.

### Cumulative GPA (GPA Tích lũy)
*   **Definition**: The weighted average of all course scores earned since the student's enrollment, excluding deleted or overwritten courses.
*   **Context**: When courses are retaken, only the highest score counts toward this metric.

### Academic Classification (Xếp loại học lực)
*   **Definition**: The designation of a student's academic standing based on cumulative GPA.
*   **Context**: Categories include Excellent (Xuất sắc), Very Good (Giỏi), Good (Khá), Average (Trung bình), Below Average (Yếu), and Fail (Kém).

---

## 3. Technical Terminology

### JWT (JSON Web Token)
*   **Definition**: An open standard (RFC 7519) that defines a compact and self-contained way for securely transmitting information between parties as a JSON object.
*   **Context**: Used as the access token in this system, containing user claims, with a short expiration of 15 minutes.

### Refresh Token Rotation (RTR)
*   **Definition**: A security practice where a new refresh token is issued alongside every new access token during token refresh requests.
*   **Context**: Helps prevent reuse attacks and session hijacking by invalidating the previously used refresh token.

### Google OAuth 2.0
*   **Definition**: An authorization framework that enables third-party applications to obtain limited access to user accounts on an HTTP service.
*   **Context**: Used to authenticate students using their university or personal Google credentials.

### UUID (Universally Unique Identifier)
*   **Definition**: A 128-bit label used for information in computer systems.
*   **Context**: UUID v4 is used as primary keys in the database and as share tokens for transcript links to prevent URL guessing.

### SPA (Single Page Application)
*   **Definition**: A web application or website that interacts with the user by dynamically rewriting the current web page with new data from the web server, instead of loading entire new pages.
*   **Context**: The frontend client is built as an SPA using React and Vite.

### REST (Representational State Transfer)
*   **Definition**: An architectural style for providing standards between computer systems on the web.
*   **Context**: The API protocol defining communication between the React frontend and the ASP.NET Core backend.

### LLM (Large Language Model)
*   **Definition**: A type of artificial intelligence program designed to recognize, summarize, translate, predict and generate text.
*   **Context**: The AI core (e.g. Google Gemini or OpenAI GPT) powering the AI Academic Advisor.

### PII (Personally Identifiable Information)
*   **Definition**: Any information that can be used to distinguish or trace an individual's identity.
*   **Context**: Must be stripped from database contexts before sending data payloads to external AI servers.

### EF Core (Entity Framework Core)
*   **Definition**: A modern object-database mapper (ORM) for .NET.
*   **Context**: The ORM used by the backend to map C# domain entities to Microsoft SQL Server tables.

### FastAPI
*   **Definition**: A modern, fast (high-performance), web framework for building APIs with Python 3.8+.
*   **Context**: The web framework chosen to wrap the AI LLM connectors and prompt formatting utilities.

---

*End of Document — Glossary*
