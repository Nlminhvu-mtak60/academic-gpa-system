# 01 — API Design Overview

> **Document ID**: API-OVER-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Web API specifications and communication standards

---

## 1. Document Purpose

This document outlines the design standards and protocols for the RESTful API of the Academic GPA Management System. It establishes the base URL, transport formats, standard payload wrappers, and header properties that define the client-server communication contract.

---

## 2. API Specifications

---

### 2.1 Base URL & Environment Isolation
*   **Local Development**: `https://localhost:7001/api/v1`
*   **Staging / QA**: `https://staging-api.gpaapp.com/api/v1`
*   **Production**: `https://api.gpaapp.com/api/v1`

---

### 2.2 Global Headers
Every HTTP Request must define the following headers:
*   `Accept`: `application/json`
*   `Content-Type`: `application/json` (for all state-modifying requests: `POST`, `PUT`, `PATCH`)
*   `Accept-Language`: `vi` or `en` (controls translation output for error strings and advisor prompts)
*   `Authorization`: `Bearer <JWT_ACCESS_TOKEN>` (for all protected routes)

---

### 2.3 Success and Error Response Envelopes
To simplify client parsing, all endpoints return standard JSON envelopes:

#### Success (Single Object / Command)
```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": { ... }
}
```

#### Success (Paginated Collection)
```json
{
  "success": true,
  "data": [ ... ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

#### Error (ProblemDetails Standards - RFC 7807)
```json
{
  "type": "https://errors.gpaapp.com/validation-error",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Failed to validate course credits input.",
  "instance": "/api/v1/courses",
  "errors": {
    "credits": [ "Credits must be an integer between 1 and 6." ]
  }
}
```

---

*End of Document — API Design Overview*
