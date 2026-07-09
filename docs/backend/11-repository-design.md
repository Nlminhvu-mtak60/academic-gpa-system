# 11 — Repository Design

> **Document ID**: ARC-BE-REPO-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Repository abstractions and Unit of Work interfaces

---

## 1. Repository Pattern Overview

The Application Layer accesses database layers through abstract repository interfaces. This decouples the core business logic from Entity Framework Core query mechanisms, simplifying database testing and future migrations.

---

## 2. Generic Repository Interface (`IGenericRepository<T>`)

Exposes standard CRUD queries for entities:

*   **Methods**:
    *   `GetByIdAsync(Guid id)`: Fetches an entity by its primary key.
    *   `ListAllAsync()`: Returns a list of all active records.
    *   `AddAsync(T entity)`: Tracks a new entity in the context.
    *   `Update(T entity)`: Updates an existing entity.
    *   `Delete(T entity)`: Removes or soft-deletes an entity.

---

## 3. Specific Repositories (Aggregate Queries)

For complex queries requiring optimizations or data aggregation, the system defines custom repository interfaces that inherit from the generic repository:

1.  **IStudentRepository**:
    *   `GetProfileWithAcademicHistoryAsync(Guid userId)`: Loads a student profile along with their full academic history (years, semesters, courses, and scores) in a single database query.
2.  **ICourseRepository**:
    *   `GetCourseWithScoresAsync(Guid courseId)`: Loads a course along with its associated score components and audit logs.
3.  **ISemesterRepository**:
    *   `GetActiveSemesterSummaryAsync(Guid semesterId)`: Fetches GPA calculations and completed credit metrics for a semester.

---

## 4. Unit of Work Pattern (`IUnitOfWork`)

Coordinates transactions across multiple repositories, ensuring database updates are committed as a single unit.

*   **Interface Definition**:
    ```csharp
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        ICourseRepository Courses { get; }
        IGenericRepository<GpaGoal> Goals { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
    ```
*   **Enforcement**: Use case handlers must modify entities through repositories and call `SaveChangesAsync()` at the end of the transaction. This ensures all updates are executed within a single SQL transaction.

---

*End of Document — Repository Design*
