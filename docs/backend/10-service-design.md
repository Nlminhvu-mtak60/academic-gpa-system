# 10 — Service Layer Design

> **Document ID**: ARC-BE-SVC-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Application service interfaces and engine specifications

---

## 1. Core Services Architecture

The system segregates business logic into dedicated application services. These services are defined as interfaces in the Application layer and implemented in the Infrastructure layer, keeping the core codebase decoupled from framework details.

---

## 2. Core Service Interfaces

---

### 2.1 GPA Calculation Engine (`IGpaCalculator`)
The central engine responsible for grade conversions and GPA calculations.

*   **Methods**:
    *   `CalculateCourseScore(decimal attendance, decimal continuous, decimal final)`: Applies nearest 0.5 rounding to components, calculates the final score, and rounds it to 1 decimal place.
    *   `MapToGradeResult(decimal courseScore)`: Maps a 10-scale course score to its letter grade (A+ to F) and GPA-4 equivalent (4.0 to 0.0).
    *   `CalculateSemesterGpa(IEnumerable<Course> courses)`: Aggregates course scores weighted by credits.
    *   `CalculateCumulativeGpa(IEnumerable<Semester> semesters)`: Calculates overall GPA, ensuring that only the highest attempt of retaken courses is counted.

---

### 2.2 JWT Management Service (`IJwtService`)
Manages stateless sessions and token rotation security checks.

*   **Methods**:
    *   `GenerateAccessToken(User user)`: Generates a signed access token (signed with HS256, valid for 15 minutes).
    *   `GenerateRefreshToken(string ipAddress)`: Generates a secure random refresh token UUID.
    *   `ValidateAccessToken(string token)`: Verifies token signature validity and expiry.

---

### 2.3 Email Service (`IEmailService`)
Manages transactional email communications.

*   **Methods**:
    *   `SendEmailAsync(string to, string subject, string body)`: Dispatches transactional emails.
    *   `SendVerificationEmailAsync(string to, string token)`: Sends account verification links.
    *   `SendPasswordResetEmailAsync(string to, string token)`: Sends password recovery links.

---

### 2.4 AI Advisor Service (`IAiAdvisorService`)
Bridges the database with the FastAPI AI advisor microservice.

*   **Methods**:
    *   `GetAiResponseAsync(Guid conversationId, string userMessage)`: Fetches chat messages, compiles the student's academic history, anonymizes this data, and forwards it to the FastAPI service.
    *   `GetFinalExamPredictionAsync(Guid courseId, string targetGrade)`: Calls the AI service to predict the required final exam score for a course.

---

*End of Document — Service Layer Design*
