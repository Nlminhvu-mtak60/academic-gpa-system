# 08 — Form Management

> **Document ID**: ARC-FE-FORM-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Form control state patterns and data bindings

---

## 1. Form Management Framework

The frontend uses **React Hook Form** to manage form states. This library minimizes unnecessary re-renders by using uncontrolled input elements, keeping the interface responsive on low-end devices.

---

## 2. Integration Architecture

Forms are configured by linking React Hook Form with Zod schemas using resolvers:

```
[User Input Event]
   └── React Hook Form captures change (uncontrolled binding)
          └── Passes dirty states to validation resolver (Zod check)
                 ├── Invalid: Sets local field error state and renders error badge
                 └── Valid: Enables form submission handler
```

---

## 3. Core Form Implementations

### 3.1 Score Input Form (Inline / Modal)
*   **Fields**: `attendanceScore`, `continuousScore`, `finalExamScore`.
*   **Behavior**:
    *   Fields bind to the React Hook Form controller.
    *   Real-time validations check ranges (`0.0` - `10.0`).
    *   Submitting the form triggers the `saveCourseScores` action, displaying a loading state inside the save button.

### 3.2 Registration Form
*   **Fields**: `email`, `password`, `firstName`, `lastName`.
*   **Behavior**: Enforces live password strength checks, updating the strength bar and rule list as the user types.

---

## 4. UI Error Indicators

*   **Field Highlight**: Input fields with validation errors are styled with red borders (`border-red-500`).
*   **Helper Text**: Error messages are rendered directly below the input field in small red text (`text-red-500 text-xs mt-1`).
*   **Focus Management**: Submitting an invalid form automatically focuses on the first field containing an error.

---

*End of Document — Form Management*
