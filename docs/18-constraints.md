# 18 — Constraints

> **Document ID**: SRS-CONS-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Design constraint registry

---

## 1. Document Purpose

This document lists all technical, organizational, regulatory, and resource boundaries that restrict design and development choices for the Academic GPA Management System. Unlike requirements (which specify *what* to build), constraints restrict *how* the system must be constructed.

---

## 2. Categorized Constraints

---

### 2.1 Technical Constraints

1.  **Backend Framework**: The core API must be built using **ASP.NET Core Web API with .NET 8**. No other backend runtimes (Node.js, Go, Java) are permitted for the primary API.
2.  **Database Technology**: Relational database storage must use **Microsoft SQL Server 2022** (or Azure SQL Database). The database interaction layer must utilize **Entity Framework Core 8** as the ORM, utilizing Code-First migrations.
3.  **AI Microservice Language**: The Generative AI adapter and prompt parsing service must be written in **Python 3.11** using the **FastAPI** web framework.
4.  **Frontend Web Stack**: The client application must be built as a Single Page Application (SPA) using **React 18**, **TypeScript**, and **TailwindCSS**, bundled using **Vite**.
5.  **Clean Architecture Architecture**: The backend codebase must strictly follow **Clean Architecture** patterns, separating the application project into `Domain`, `Application`, `Infrastructure`, and `Presentation/API` boundaries.

---

### 2.2 Regulatory & Privacy Constraints

1.  **Personal Data Isolation (GDPR & Vietnam Decree 13/2023/ND-CP)**: The system must protect student privacy. Personal Identifying Information (PII) such as full names, student codes, email addresses, and passwords must never be stored in log files, passed to console outputs, or sent to external LLM endpoints.
2.  **Password Security**: Raw passwords must never be written to database fields or transmitted insecurely. They must be hashed using the **BCrypt** algorithm with a work factor (salt rounds) of $\ge 12$ prior to storage.
3.  **Cryptographic Share Tokens**: Shared transcript links must not expose database sequential keys (e.g., `/shared/15`). They must utilize cryptographically secure **UUID v4** tokens.

---

### 2.3 Resource & Operational Constraints

1.  **Hosting Environment Budget**: Deployment configurations must run on standard VM/container hosting services (e.g., Docker Compose on a single Ubuntu instance or Azure Container Apps on a Basic tier). The database and containers must operate within a combined host memory ceiling of **4GB RAM** during initial staging tests.
2.  **Third-Party API Expenses**: LLM token consumption must be capped to prevent budget exhaustion. High-cost models (e.g., GPT-4o) must be configured with fallback triggers or replaced with cost-effective alternatives (e.g., GPT-4o-mini, Gemini 1.5 Flash).
3.  **Rate Limit Enforcements**: Route access must be restricted to prevent server overload:
    *   Auth routes: 5 attempts/15 min per IP.
    *   AI requests: 20 calls/hour per student account.

---

### 2.4 Interface & Localization Constraints

1.  **Bilingual Requirement**: All client interface texts, tooltips, notification titles, and error messages must be translated. Translation strings must reside in client localization assets (JSON structures), never hardcoded directly in UI markup.
2.  **Responsiveness Limits**: The user interface must adapt dynamically, supporting devices ranging from mobile screens ($375\text{px}$ width) to high-resolution desktop displays ($1920\text{px}$ width) without horizontal overflow or clipped text elements.

---

*End of Document — Constraints*
