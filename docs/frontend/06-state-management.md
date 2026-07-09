# 06 — State Management

> **Document ID**: ARC-FE-STATE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Client-side state managers and context wrappers

---

## 1. State Management Architecture

To keep the frontend lightweight, the application uses **React Context** to manage global state sections. Volatile, page-specific states (e.g. active search inputs or modal open toggles) are kept inside local component states (`useState`, `useReducer`).

---

## 2. Global Context Modules

The application defines three global context providers to coordinate state:

---

### 2.1 Authentication State (`AuthContext`)
*   **Properties**:
    *   `user` (User profile details: email, initials, role).
    *   `accessToken` (Current JWT string stored in-memory).
    *   `isAuthenticated` (Boolean flag).
*   **Actions**:
    *   `login(credentials)`: Calls the sign-in API, updates the access token in memory, and triggers navigation.
    *   `logout()`: Calls the logout API, invalidates the local token, and clears the secure HttpOnly cookie.
    *   `refreshToken()`: Coordinates the token refresh flow.

---

### 2.2 GPA Calculation State (`GpaContext`)
*   **Properties**:
    *   `academicYears` (List of years and GPAs).
    *   `activeYear` / `activeSemester` (GUID references of the current student view).
    *   `activeCourses` (List of courses in the selected semester).
    *   `cumulativeGpa` (Overall cumulative GPA data).
*   **Actions**:
    *   `loadAcademicHistory()`: Fetches academic records from the API and updates local stats.
    *   `saveCourseScores(courseId, scores)`: Dispatches score updates to the API and triggers a GPA recalculation.
    *   `triggerLocalRecalculation()`: Calculates GPAs locally for immediate visual feedback before the API confirms writes.

---

### 2.3 Preferences & System State (`PreferencesContext`)
*   **Properties**:
    *   `language` (`'vi'` | `'en'`).
    *   `theme` (`'light'` | `'dark'`).
    *   `activeToasts` (Array of active toast alerts).
*   **Actions**:
    *   `setLanguage(lang)`: Updates the i18next language configuration and saves user preferences.
    *   `toggleTheme()`: Toggles the CSS theme variables and saves the active theme to `localStorage`.
    *   `addToast(message, type)`: Appends an alert to the toast list, triggering a 5-second automatic dismiss timer.

---

*End of Document — State Management*
