# 16 — Background Services

> **Document ID**: ARC-BE-BG-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Background task executors and worker schedules

---

## 1. Document Purpose

This document specifies the background task execution architecture, scheduling framework, and thread safety guidelines for the system.

---

## 2. Background Task Framework (Quartz.NET)

The system uses **Quartz.NET** integrated into the ASP.NET Core host process to run scheduled maintenance and background tasks.

---

## 3. Scheduled Maintenance Jobs

---

### 3.1 PurgeExpiredNotificationsJob
*   **Purpose**: Cleans up the notifications table.
*   **Execution Schedule**: Daily at 02:00 AM UTC.
*   **Business Logic**: Identifies and deletes read notifications older than 1 year. Unread notifications are preserved.

---

### 3.2 PurgeExpiredSharedTranscriptsJob
*   **Purpose**: Inactivates expired public sharing links.
*   **Execution Schedule**: Hourly.
*   **Business Logic**: Finds `SharedTranscripts` records where `ExpiresAt` is in the past and sets `IsRevoked = 1`.

---

## 4. Resolving Scoped Services in Background Workers

Quartz jobs are registered as singletons, which means they cannot inject scoped dependencies (like `ApplicationDbContext`) directly into their constructors. Doing so would cause dependency capture errors.

### 4.1 Implementation Rule
*   Jobs inject the `IServiceProvider` interface.
*   During execution, the job creates a temporary DI scope to resolve the DbContext:
    ```csharp
    public async Task Execute(IJobExecutionContext context)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            // Execute background query...
        }
    }
    ```

---

*End of Document — Background Services*
