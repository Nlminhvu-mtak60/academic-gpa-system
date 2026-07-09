# 02 — Entity-Relationship Diagram (ERD)

> **Document ID**: DB-ERD-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Mermaid crow's foot diagram notation

---

## 1. Relational Entity Relationship Diagram

The diagram below maps all tables, keys, column data types, and relational cardinalities in the database.

```mermaid
erDiagram
    Users {
        uniqueidentifier Id PK
        nvarchar_100 Email UK
        nvarchar_256 PasswordHash
        nvarchar_50 FirstName
        nvarchar_50 LastName
        nvarchar_20 Role
        bit IsActive
        bit IsEmailVerified
        nvarchar_500 AvatarUrl
        nvarchar_10 PreferredLanguage
        nvarchar_10 PreferredTheme
        nvarchar_100 GoogleId
        datetime2 CreatedAt
        datetime2 UpdatedAt
        datetime2 LastLoginAt
        datetime2 LockedAt
        nvarchar_200 LockReason
    }

    RefreshTokens {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK
        nvarchar_500 Token
        nvarchar_500 ReplacedByToken
        datetime2 ExpiresAt
        datetime2 CreatedAt
        datetime2 RevokedAt
        nvarchar_100 CreatedByIp
    }

    StudentProfiles {
        uniqueidentifier Id PK
        uniqueidentifier UserId FK "1-to-1"
        nvarchar_50 StudentCode UK
        nvarchar_200 UniversityName
        nvarchar_200 MajorName
        int EnrollmentYear
        int TotalRequiredCredits
    }

    AcademicYears {
        uniqueidentifier Id PK
        uniqueidentifier StudentProfileId FK
        nvarchar_20 YearName
        int StartYear
        int EndYear
        int SortOrder
        bit IsDeleted
        datetime2 CreatedAt
    }

    Semesters {
        uniqueidentifier Id PK
        uniqueidentifier AcademicYearId FK
        nvarchar_50 SemesterName
        int SortOrder
        bit IsDeleted
        datetime2 CreatedAt
    }

    Courses {
        uniqueidentifier Id PK
        uniqueidentifier SemesterId FK
        nvarchar_20 CourseCode
        nvarchar_200 CourseName
        int Credits
        bit IsRetake
        uniqueidentifier OriginalCourseId FK "Self-Reference"
        bit IsDeleted
        datetime2 CreatedAt
        datetime2 UpdatedAt
    }

    Scores {
        uniqueidentifier Id PK
        uniqueidentifier CourseId FK "1-to-1"
        decimal_5_2 AttendanceScore
        decimal_5_2 ContinuousScore
        decimal_5_2 FinalExamScore
        decimal_5_2 CourseScore
        nvarchar_5 LetterGrade
        decimal_3_2 Gpa4Value
        datetime2 CalculatedAt
        datetime2 UpdatedAt
    }

    ScoreAuditLogs {
        uniqueidentifier Id PK
        uniqueidentifier ScoreId FK
        nvarchar_50 FieldChanged
        nvarchar_20 OldValue
        nvarchar_20 NewValue
        datetime2 ChangedAt
    }

    GpaGoals {
        uniqueidentifier Id PK
        uniqueidentifier StudentProfileId FK
        decimal_5_2 TargetCumulativeGpa10
        decimal_3_2 TargetCumulativeGpa4
        nvarchar_500 Notes
        bit IsAchieved
        datetime2 CreatedAt
        datetime2 UpdatedAt
    }

    SharedTranscripts {
        uniqueidentifier Id PK
        uniqueidentifier StudentProfileId FK
        uniqueidentifier ShareToken UK
        datetime2 ExpiresAt
        bit IsRevoked
        int ViewCount
        datetime2 CreatedAt
    }

    AiConversations {
        uniqueidentifier Id PK
        uniqueidentifier StudentProfileId FK
        nvarchar_200 Title
        datetime2 CreatedAt
        datetime2 UpdatedAt
    }

    AiMessages {
        uniqueidentifier Id PK
        uniqueidentifier ConversationId FK
        nvarchar_10 Role
        nvarchar_max Content
        datetime2 CreatedAt
    }

    Notifications {
        uniqueidentifier Id PK
        uniqueidentifier RecipientId FK
        uniqueidentifier SenderId FK
        nvarchar_200 Title
        nvarchar_2000 Message
        nvarchar_20 Type
        bit IsRead
        datetime2 ReadAt
        bit IsBroadcast
        datetime2 CreatedAt
    }

    SystemSettings {
        uniqueidentifier Id PK
        nvarchar_100 SettingKey UK
        nvarchar_max SettingValue
        nvarchar_500 Description
        datetime2 UpdatedAt
    }

    %% Relationship Cardinalities
    Users ||--o{ RefreshTokens : "authenticates session via"
    Users ||--o| StudentProfiles : "contains profile"
    StudentProfiles ||--o{ AcademicYears : "manages"
    AcademicYears ||--o{ Semesters : "contains"
    Semesters ||--o{ Courses : "contains"
    Courses ||--|| Scores : "has scores"
    Scores ||--o{ ScoreAuditLogs : "tracks changes in"
    Courses |o--o| Courses : "retakes (self-reference)"
    StudentProfiles ||--o{ GpaGoals : "sets target"
    StudentProfiles ||--o{ SharedTranscripts : "shares academic transcript via"
    StudentProfiles ||--o{ AiConversations : "interacts with advisor via"
    AiConversations ||--o{ AiMessages : "contains message exchange history"
    Users ||--o{ Notifications : "receives direct or system-wide messages"
```

---

## 2. Cardinality Rules & Design Decisions

1.  **Users to StudentProfiles (1-to-0/1)**: A generic user can exist without a student profile (e.g., an Admin account). However, a student profile must link to exactly one User record.
2.  **StudentProfiles to AcademicYears (1-to-many)**: A student can manage multiple academic years. Academic years are owned by a single student profile.
3.  **AcademicYears to Semesters (1-to-many)**: Each academic year contains multiple semesters (enforced to a maximum of 3 by application logic).
4.  **Semesters to Courses (1-to-many)**: A semester contains multiple courses. A course belongs to exactly one semester.
5.  **Courses to Scores (1-to-1)**: Every course has exactly one linked score record. Keeping scores in a separate table keeps the `Courses` table clean and allows isolation of audit logging processes.
6.  **Courses to Courses (0/1-to-0/1 Self-Reference)**: Represents retaken courses. A retaken course points to its `OriginalCourseId`.

---

*End of Document — ERD*
