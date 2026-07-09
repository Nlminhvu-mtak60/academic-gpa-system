# 19 — Global Error Codes

> **Document ID**: API-ERROR-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Error registry table and error payload formats

---

## 1. Document Purpose

This document catalogs all application-specific error codes returned by the API. These codes enable frontend applications to identify errors precisely and render localized recovery messages for users.

---

## 2. Global Error Code Catalog

---

### 2.1 Authentication & Security Errors
| Error Code | HTTP Status | Title / Message (Vietnamese) | Message (English) |
|:---|:---:|:---|:---|
| **AUTH_INVALID_CREDENTIALS** | `401` | Email hoặc mật khẩu không hợp lệ. | Invalid email or password. |
| **AUTH_EMAIL_EXISTS** | `409` | Email này đã được đăng ký. | This email is already registered. |
| **AUTH_EMAIL_NOT_VERIFIED** | `401` | Vui lòng xác thực email trước khi đăng nhập. | Please verify your email before logging in. |
| **AUTH_TOKEN_EXPIRED** | `401` | Phiên làm việc đã hết hạn. | Authentication token has expired. |
| **AUTH_ACCOUNT_LOCKED** | `403` | Tài khoản đã bị khóa. | Your account has been locked. |

---

### 2.2 Academic Records & Validations Errors
| Error Code | HTTP Status | Title / Message (Vietnamese) | Message (English) |
|:---|:---:|:---|:---|
| **ACAD_SEMESTER_LIMIT** | `422` | Tối đa 3 học kỳ trong một năm học. | Maximum of 3 semesters per academic year. |
| **ACAD_CREDITS_RANGE** | `400` | Số tín chỉ phải nằm trong khoảng từ 1 đến 6. | Course credits must be between 1 and 6. |
| **ACAD_SCORE_RANGE** | `400` | Điểm thành phần phải từ 0.0 đến 10.0. | Component scores must be between 0.0 and 10.0. |
| **ACAD_DUPLICATE_CODE** | `409` | Mã môn học đã tồn tại trong học kỳ. | Course code already exists in this semester. |

---

### 2.3 Goal Planning & Analytics Errors
| Error Code | HTTP Status | Title / Message (Vietnamese) | Message (English) |
|:---|:---:|:---|:---|
| **GOAL_UNACHIEVABLE** | `422` | Mục tiêu không khả thi với số tín chỉ hiện có. | Goal is not achievable with the specified credits. |
| **GOAL_NOT_FOUND** | `404` | Không tìm thấy mục tiêu GPA. | GPA target goal not found. |

---

### 2.4 AI Advisor & Rate Limiting Errors
| Error Code | HTTP Status | Title / Message (Vietnamese) | Message (English) |
|:---|:---:|:---|:---|
| **AI_RATE_LIMIT_EXCEEDED** | `429` | Bạn đã đạt giới hạn 20 tin nhắn mỗi giờ. | You have reached the limit of 20 messages per hour. |
| **AI_CONV_LIMIT** | `422` | Tối đa 50 cuộc hội thoại với AI. | Maximum of 50 active AI conversations reached. |

---

### 2.5 Shared Transcripts Errors
| Error Code | HTTP Status | Title / Message (Vietnamese) | Message (English) |
|:---|:---:|:---|:---|
| **SHARE_LINK_EXCEEDED** | `422` | Tối đa 10 liên kết chia sẻ đang hoạt động. | Maximum of 10 active share links reached. |
| **SHARE_LINK_EXPIRED** | `404` | Liên kết chia sẻ này đã hết hạn hoặc bị thu hồi. | This shared transcript has expired or is revoked. |

---

*End of Document — Global Error Codes*
