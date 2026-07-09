# Future Roadmap — Post-Release Enhancements

The Academic GPA Management System is designed to support future functional growth. The following roadmap outlines the strategic phases of the platform's evolution.

---

## Phase 1: Performance Tuning & Notification Channels (Q3 2026)
- **Redis Cache Integration**:
  - Implement a caching layer for static data (such as academic years, letter grade scales) and user dashboard statistics to reduce SQL database queries.
- **Extended Notification Channels**:
  - Integrate SMTP email notification and SMS/Push notifications via Firebase (FCM) or Twilio to alert students when goals are achieved or grades are updated.
- **Multi-Factor Authentication (MFA)**:
  - Add optional TOTP authentication options for student and administrator accounts.

---

## Phase 2: Native Mobile Client & Institutional Integration (Q1 2027)
- **Mobile Application Development**:
  - Build native mobile clients using React Native, reusing API clients and TypeScript types.
- **Institutional APIs**:
  - Expose API endpoints allowing universities to push semester grades directly into student profiles, eliminating manual entry.
- **AI Recommendation Tuning**:
  - Enhance the AI Advisor FastAPI service to load specialized local LLMs, reducing external API costs and improving response times.

---

## Phase 3: Multi-Tenancy & Institutional Scale (Q3 2027)
- **Multi-Tenant Architecture**:
  - Partition databases to host multiple academic institutions on a single software-as-a-service (SaaS) instance.
- **Custom Grading Scales Builder**:
  - Allow administrators to define custom grading scales (e.g. 100-point scale, GPA 5.0 scale, ECTS) to support international universities.
- **Bulk Imports**:
  - Implement file parsing handlers (CSV/Excel) for university registrars to import student rosters and course catalogs in batch mode.
