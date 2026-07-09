# 20 — API Conventions

> **Document ID**: API-CONV-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Interface routing standards and HTTP conventions

---

## 1. Document Purpose

This document establishes the global REST conventions, parameter structures, datatypes formats, and rate-limiting policies for the Academic GPA Management System API.

---

## 2. API Design & Format Conventions

---

### 2.1 JSON Payload Naming Strategy
*   **CamelCase**: All JSON request and response payloads must use **camelCase** for properties (e.g. `studentCode`, `attendanceScore`), aligning with JavaScript naming conventions.
*   **Immutable Types**: API request inputs map to immutable C# records, ensuring request states cannot be modified during execution.

---

### 2.2 Date-Time Representation
*   **ISO 8601 UTC**: All date and time values are transmitted as strings in ISO 8601 format, using UTC time zones with the `Z` offset:
    *   Example: `2026-06-24T13:30:00Z`
*   **Time Resolution**: Use `datetime2` SQL mapping to preserve millisecond accuracy when auditing score updates.

---

### 2.3 Pagination Query Structure
Any endpoint that returns a list must support these query parameters:

*   `page`: The page index, starting at 1. Defaults to `1` if omitted or $\le 0$.
*   `pageSize`: The number of items to return. Defaults to `20`. Restricted to a maximum of `100` to prevent memory issues.
*   **Metadata Response**: Collection lists are wrapped in a response object containing a `pagination` envelope:
    ```json
    "pagination": {
      "page": 1,
      "pageSize": 20,
      "totalItems": 145,
      "totalPages": 8
    }
    ```

---

### 2.4 Sorting & Searching parameters
*   `sortBy`: Field name to sort by (e.g. `'courseCode'`, `'createdAt'`). Field names must map to the DTO property names.
*   `sortOrder`: Sort direction. Allowed values: `asc` (ascending) and `desc` (descending). Defaults to `desc` if omitted.
*   `search`: A string parameter used to filter results (e.g. `?search=Calculus` matches course codes or names containing "Calculus").

---

### 2.5 Security & Rate Limiting Policies
*   **Route Protections**: Endpoints are secure by default, requiring a valid JWT in the `Authorization: Bearer <token>` header, unless marked with the `[AllowAnonymous]` attribute.
*   **IP-Based Rate Limiting**: Managed at the gateway layer:
    *   *General API*: 100 requests per minute per IP.
    *   *Authentication*: 5 login attempts per 15 minutes per IP.
    *   *AI Chat*: 20 messages per hour per student.

---

*End of Document — API Conventions*
