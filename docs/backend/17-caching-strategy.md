# 17 — Caching Strategy

> **Document ID**: ARC-BE-CACHE-001  
> **Version**: 1.0  
> **Last Updated**: June 2026  
> **Status**: 🔄 In Review  
> **Format**: Caching patterns and invalidation strategies

---

## 1. Caching Strategy Overview

To improve API response times and reduce database load, the system implements a caching strategy. The MVP uses in-memory caching (`IMemoryCache`), built behind abstract interfaces to allow transitioning to distributed caching (Redis) in the future.

---

## 2. Caching Interface & Wrapper

Services access the cache layer through the `ICacheService` interface:

*   **Methods**:
    *   `GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration)`: Returns a cached value, or executes the factory delegate to retrieve, cache, and return the value if it is not cached.
    *   `RemoveAsync(string key)`: Invalidates a cached item by its key.

---

## 3. Cache Key Conventions

Keys are structured hierarchically using colons (`:`) to prevent name collisions:

*   `student:gpa:{studentId}` $\rightarrow$ Caches calculated semester and cumulative GPA values for a student.
*   `settings:global` $\rightarrow$ Caches global system configurations.
*   `locale:translation:{lang}` $\rightarrow$ Caches static dictionary translations.

---

## 4. Cache Invalidation & Lifetime Rules

---

### 4.1 Invalidation Triggers
Cached data must be invalidated immediately when changes are saved:
*   **GPA Invalidation**: If any score component, course, semester, or academic year is added, updated, or deleted, the system invalidates the cached GPA for that student (`student:gpa:{studentId}`). The next query will run the calculation engine and update the cache.
*   **Settings Invalidation**: Modifying system settings invalidates the `settings:global` cache key.

---

### 4.2 Expiration Policies
*   **Slide Expiration**: Settings and translations configure sliding expirations (e.g. 60 minutes). The cache resets its timer if the item is accessed before it expires.
*   **Absolute Expiration**: GPA cache cards configure a maximum absolute expiration of 2 hours, ensuring the database is queried regularly even if invalidation triggers fail.

---

*End of Document — Caching Strategy*
