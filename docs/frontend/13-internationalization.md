# 13 — Internationalization (i18n)

> **Document ID**: ARC-FE-I18N-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Translation dictionaries and language switcher configs

---

## 1. Document Purpose

This document specifies the internationalization (i18n) architecture, translation keys, and language switching configurations for the application.

---

## 2. Localization Configuration (`react-i18next`)

The application uses **i18next** and `react-i18next` to manage translations:

*   **Initialization**: The library is configured in `/src/utils/i18n.ts`. It loads translations from static JSON assets stored in `/public/locales/{en|vi}/translation.json`.
*   **Default Language**: Defaults to `vi` (Vietnamese). Falls back to `en` (English) if the user's browser language is not supported.
*   **Key Nesting**: Translation keys are nested by feature area (e.g. `auth.login.title`, `gpa.semester.credits`) to keep translation dictionaries organized.

---

## 3. Translation Key Structures

#### Example: `vi/translation.json`
```json
{
  "common": {
    "save": "Lưu",
    "cancel": "Hủy"
  },
  "dashboard": {
    "title": "Bảng điều khiển",
    "creditsCompleted": "Tín chỉ hoàn thành"
  }
}
```

---

## 4. Language Switcher State Management

*   **State Persistence**: Selected languages are saved in `localStorage` and sent to the API as a preference setting.
*   **Bilingual Sync**: When the user switches languages, the application updates i18next's active language configuration (`i18n.changeLanguage(lang)`), updating all text elements across the app instantly.
*   **API Localization Header**: The active language code is injected in the `Accept-Language` header of all outgoing Axios requests.

---

*End of Document — Internationalization (i18n)*
