# 16 — Settings API

> **Document ID**: API-SETT-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: System settings configuration endpoint contracts

---

## 1. Settings Endpoints Overview

These endpoints allow system administrators to view and update system-wide configurations and parameters.

---

## 2. Endpoint Specifications

---

### 2.1 GET /admin/settings
Lists all global system settings.

*   **HTTP Method**: `GET`
*   **Authorization**: Bearer token (Admin only)
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "data": [
        {
          "settingKey": "MaintenanceMode",
          "settingValue": "false",
          "description": "If true, blocks non-admin users from accessing the API.",
          "updatedAt": "2026-06-24T10:00:00Z"
        },
        {
          "settingKey": "AiMessageLimitPerHour",
          "settingValue": "20",
          "description": "Rate limit for student queries to the AI advisor.",
          "updatedAt": "2026-06-24T10:00:00Z"
        }
      ]
    }
    ```

---

### 2.2 PUT /admin/settings/{key}
Updates the value of a system setting.

*   **HTTP Method**: `PUT`
*   **Authorization**: Bearer token (Admin only)
*   **Path Parameters**:
    *   `key` (string): The configuration setting key (e.g. `'MaintenanceMode'`).
*   **Request Body**:
    ```json
    {
      "settingValue": "true"
    }
    ```
*   **Validation Rules**:
    *   `settingValue`: Required, string format.
*   **Success Response** (`200 OK`):
    ```json
    {
      "success": true,
      "message": "System setting updated successfully.",
      "data": {
        "settingKey": "MaintenanceMode",
        "settingValue": "true"
      }
    }
    ```
*   **Error Responses**:
    *   `404 Not Found`: Configuration setting key does not exist.

---

*End of Document — Settings API*
