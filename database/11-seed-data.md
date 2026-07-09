# 11 — Seed Data

> **Document ID**: DB-SEED-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Database seed records and system configuration values

---

## 1. Document Purpose

This document specifies the initial system data, default settings, and administrative accounts that must be seeded into the database during deployment.

---

## 2. Seeding Strategy

The system utilizes two database seeding strategies:
1.  **EF Core Migration Seeding (`HasData`)**: Used for immutable, system-critical configuration records. These records are managed in code and deployed automatically as part of migrations.
2.  **SQL Script Seeding**: Used to provision default admin accounts and set up initial settings in staging and production environments.

---

## 3. Seed Datasets

---

### 3.1 Global System Settings (`SystemSettings` Table)
These default configuration settings must be seeded during database initialization:

| SettingKey | SettingValue | Description |
|:---|:---|:---|
| `MaintenanceMode` | `'false'` | If `'true'`, blocks non-admin users from accessing the API. |
| `MaxFailedLogins` | `'10'` | Number of failed login attempts before an account is auto-locked. |
| `LockoutDurationMinutes`| `'15'` | Length of lock period after exceeding login attempts. |
| `AccessTokenExpiryMinutes`| `'15'` | Expiration length for JWT Access tokens. |
| `RefreshTokenExpiryDays`| `'7'` | Expiration length for Refresh tokens. |
| `AiMessageLimitPerHour` | `'20'` | Rate limit for student queries to the AI advisor. |

---

### 3.2 Default Administrator Account
For security, production admin accounts must be created using environment-specific variables during deployment. For local development and staging environments, a default admin account is seeded:

*   **Id**: `33a25d2c-80a5-4089-9a2c-f60897f2c253`
*   **Email**: `admin@gpa.domain.com`
*   **PasswordHash**: BCrypt hash of `Admin@123456`
*   **FirstName**: `System`
*   **LastName**: `Administrator`
*   **Role**: `'Admin'`
*   **IsActive**: `1` (Active)
*   **IsEmailVerified**: `1` (Verified)

---

### 3.3 Grading Scale Calibrations (Reference Data)
The system assumes standard Vietnamese grading scales. These rules are implemented in the C# domain layer for the MVP, but can be loaded dynamically in future releases:

| Score Range | Letter Grade | GPA-4 Value | Classification (VN) |
|:---|:---:|:---:|:---|
| 9.00 – 10.00 | A+ | 4.00 | Xuất sắc |
| 8.50 – 8.99 | A | 3.70 | Giỏi |
| 8.00 – 8.49 | B+ | 3.50 | Khá giỏi |
| 7.00 – 7.99 | B | 3.00 | Khá |
| 6.50 – 6.99 | C+ | 2.50 | Trung bình khá |
| 5.50 – 6.49 | C | 2.00 | Trung bình |
| 5.00 – 5.49 | D+ | 1.50 | Trung bình yếu |
| 4.00 – 4.99 | D | 1.00 | Yếu |
| 0.00 – 3.99 | F | 0.00 | Không đạt |

---

*End of Document — Seed Data*
