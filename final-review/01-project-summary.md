# Project Summary — Academic GPA Management System

The Academic GPA Management System is a commercial-grade, multi-tier web application designed to help university students track their academic progress, set GPA goals, predict final exam outcomes, and receive AI-driven recommendations.

---

## Tech Stack Overview

The system is built on a modern, decoupled, and containerized architecture:

### 1. Frontend Client
- **Framework**: React 18 with TypeScript, scaffolded using Vite.
- **Routing**: React Router DOM (v6).
- **Styling**: Vanilla CSS with curated dark and light mode themes.
- **State & Contexts**: React Context API for authentication, settings, and notifications.
- **Localization**: Built-in dynamic Vietnamese (VI) and English (EN) translation dictionaries.

### 2. Backend API
- **Framework**: ASP.NET Core 10.0 following **Clean Architecture** patterns.
- **Database Access**: Entity Framework Core 10.0 with LINQ.
- **MediatR**: CQRS (Command Query Responsibility Segregation) pattern for business workflows.
- **Security**: JWT Authentication (Access & Refresh tokens) and Role-Based Access Control (Admin / Student).
- **Logging**: Serilog file-based rolling logging system.

### 3. AI Service
- **Framework**: FastAPI (Python 3.11).
- **Capabilities**:
  - GPA Trend Analysis & Learning Patterns
  - Target Goal Feasibility Evaluation
  - Required Final Exam Score Predictions
  - Custom Academic Recommendations

### 4. Database Server
- **Engine**: Microsoft SQL Server 2022.
- **Key Schemas**: Users, Profiles, Semesters, Course Grades, Settings, Notifications, AI Conversations, and Messages.

---

## Deployment & Environments
- **Containerization**: Single command startup via `docker-compose.yml` for developers and `docker-compose.prod.yml` for isolated production hosts.
- **Reverse Proxy**: Nginx container acts as the web host and routes `/api` directly to the backend to eliminate CORS issues.
