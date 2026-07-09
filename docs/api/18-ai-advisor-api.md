# 18 — AI Advisor API

> **Document ID**: API-AI-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: AI advisor integration endpoints and chat contracts

---

## 1. AI Advisor Endpoints Overview

These endpoints manage student interactions with the AI Academic Advisor. The system automatically fetches and appends the student's academic records to queries to provide context-aware study advice.

---

## 2. Endpoint Specifications

---

### 2.1 POST /ai/conversations
Starts a new conversation thread with the AI Advisor.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Request Body**:
    ```json
    {
      "title": "Query regarding Calculus improvement"
    }
    ```
*   **Validation Rules**:
    *   `title`: Required, string, 1 to 200 characters.
*   **Success Response** (`201 Created`):
    ```json
    {
      "success": true,
      "message": "Conversation started successfully.",
      "data": {
        "id": "c72fb33f-8461-460d-85fa-7c961e67fa8b",
        "title": "Query regarding Calculus improvement",
        "createdAt": "2026-06-24T13:30:00Z"
      }
    }
    ```
*   **Error Responses**:
    *   `422 Unprocessable Entity`: Student has exceeded the limit of 50 active conversations.

---

### 2.2 GET /ai/conversations
Retrieves a paginated list of the student's chat conversations.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Query Parameters**: Standard pagination (`page`, `pageSize`).
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "id": "c72fb33f-8461-460d-85fa-7c961e67fa8b",
          "title": "Query regarding Calculus improvement",
          "createdAt": "2026-06-24T13:30:00Z",
          "updatedAt": "2026-06-24T13:35:00Z"
        }
      ],
      "pagination": {
        "page": 1,
        "pageSize": 20,
        "totalItems": 1,
        "totalPages": 1
      }
    }
    ```

---

### 2.3 POST /ai/conversations/{id}/messages
Sends a message to the AI Advisor and returns the advisor's response.

*   **HTTP Method**: `POST`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target conversation ID.
*   **Request Body**:
    ```json
    {
      "message": "What scores do I need next semester to raise my GPA to 8.0?"
    }
    ```
*   **Validation Rules**:
    *   `message`: Required, 1 to 2000 characters.
*   **Business Rules**:
    *   **Rate Limiting**: Enforces a limit of 20 messages per hour per student. If exceeded, returns `429 Too Many Requests`.
    *   **Context Aggregation**: The backend automatically fetches the student's GPAs, completed credits, goals, and course history, anonymizes this data, and includes it in the prompt sent to the LLM.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": {
        "reply": "Dựa trên dữ liệu học tập hiện tại của bạn (GPA 7.42)...",
        "createdAt": "2026-06-24T13:35:05Z"
      }
    }
    ```

---

### 2.4 GET /ai/conversations/{id}/messages
Retrieves the message history for a conversation.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target conversation ID.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "role": "user",
          "content": "What scores do I need next semester to raise my GPA to 8.0?",
          "createdAt": "2026-06-24T13:35:00Z"
        },
        {
          "role": "assistant",
          "content": "Dựa trên dữ liệu học tập hiện tại của bạn (GPA 7.42)...",
          "createdAt": "2026-06-24T13:35:05Z"
        }
      ]
    }
    ```

---

### 2.5 DELETE /ai/conversations/{id}
Soft-deletes/archives a conversation.

*   **HTTP Method**: `DELETE`
*   **Authorization**: Bearer token (Student role)
*   **Path Parameters**:
    *   `id` (guid): Target conversation ID.
*   **Success Response** (`204 No Content`): No response body.

---

*End of Document — AI Advisor API*
