# 07 — API Architecture

> **Document ID**: ARC-API-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: HTTP REST API design specifications and transport contracts

---

## 1. API Design Principles

The application's API acts as the interface between the presentation client and the underlying business handlers. It is designed to be stateless, uniform, structured, and easy to maintain.

---

## 2. API Design Conventions

### 2.1 URI Resource Structure
*   **Plural Nouns**: Resource paths must use plural nouns (e.g. `/api/v1/semesters`, `/api/v1/courses`). Action verbs must be avoided in routes unless mapping custom actions (e.g. `/api/v1/transcripts/share` is allowed for trigger actions).
*   **Hierarchy**: Hierarchical sub-resource endpoints must follow logical containment paths:
    *   `GET /api/v1/semesters/{semesterId}/courses` (Retrieves courses inside a specific semester).
*   **Case Sensitivity**: URIs are lowercase, separation uses hyphens (spinal-case) for readability: `/api/v1/academic-years`.

### 2.2 Versioning
*   **URL Versioning**: The application uses explicit path versioning.
*   **Route Prefix**: Every API route starts with `/api/v{version}` (e.g. `/api/v1/auth/login`).
*   **Backward Compatibility**: Breaking route or request model updates require incrementing the version prefix (e.g. migrating to `/api/v2`).

---

## 3. Request-Response Specifications

### 3.1 Standard Response Envelopes
Every API response returns a predictable JSON structure:

#### Success Response (Single Object / Command)
```json
{
  "success": true,
  "message": "Course score updated successfully.",
  "data": {
    "courseId": "c62fb33f-8461-460d-85fa-7c961e67fa8b",
    "courseScore": 8.5,
    "letterGrade": "A",
    "gpa4Value": 3.7
  }
}
```

#### Success Response (Paginated Collection)
```json
{
  "success": true,
  "data": [
    { "id": "uuid-1", "name": "Mathematics" },
    { "id": "uuid-2", "name": "Computer Science" }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 45,
    "totalPages": 5
  }
}
```

#### Error Response (RFC 7807 ProblemDetails Standard)
```json
{
  "type": "https://errors.gpaapp.com/validation-error",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Failed to validate course input request details.",
  "instance": "/api/v1/courses",
  "errors": {
    "credits": [ "Credits must be an integer between 1 and 6." ]
  }
}
```

---

## 4. Query Parameter Protocols (Pagination, Filter, Sort)

To protect the server from memory bloat and keep network payloads small, all endpoints returning arrays must support pagination, sorting, and filtering parameters:

### 4.1 Pagination
*   **Parameters**: `page` (default: 1) and `pageSize` (default: 20, max limit: 100).
*   **Implementation**: Done in EF Core using `Skip((page - 1) * pageSize).Take(pageSize)`.

### 4.2 Filtering
*   **Parameter**: `search` string parameter.
*   **Behavior**: Handlers map this string to field comparisons (e.g. `c.CourseName.Contains(search) || c.CourseCode.Contains(search)`).

### 4.3 Sorting
*   **Parameters**: `sortBy` (field name) and `sortOrder` (`asc` or `desc`).
*   **Behavior**: Default sort field is `createdAt` in descending order.

---

## 5. HTTP Status Code Mapping

The API maps operations to standard HTTP status codes:

| Code | Status | Application Usage Scenario |
|---|---|---|
| **200** | OK | Success response for `GET`, `PUT`, or actions returning objects. |
| **201** | Created | Success response for `POST` (includes a `Location` header to locate the resource). |
| **204** | No Content | Success response for `DELETE` operations or actions returning no data. |
| **400** | Bad Request | The input payload contains syntax errors or invalid formats. |
| **401** | Unauthorized| Missing or expired JWT access token. |
| **403** | Forbidden | The user is authenticated but does not have the required role (e.g., student calling admin endpoints). |
| **404** | Not Found | The requested resource (User, Course, Goal) does not exist in the system. |
| **422** | Unprocessable Entity| The request is syntactically correct, but violates a business rule (e.g., adding a 4th semester to a year). |
| **429** | Too Many Requests| The rate limit threshold has been exceeded. |
| **500** | Internal Server Error| An unexpected server error occurred. |

---

*End of Document — API Architecture*
