# Performance Summary — Optimizations, Caching, & Loading

Performance reviews confirm that the platform is optimized for low latency and high concurrent load.

---

## 1. Database Query Tuning

- **Indexing Strategy**:
  - Main foreign keys (`UserId` on profiles, `SemesterId` on courses, and `CourseId` on scores) are indexed.
  - Queries are written using select projections (`.Select()`) to load only the required fields instead of whole tables.
- **Query Tracking**:
  - Global read-only queries (e.g., dashboard stats, semester summaries) use Entity Framework Core's `.AsNoTracking()` method, reducing memory allocation.

---

## 2. Server Resource Scalability

- **Non-Blocking Asynchronous Code**:
  - All DB commands, external AI service requests, and email logs utilize standard `async`/`await` patterns.
  - Helps prevent thread pool starvation under high concurrent requests.
- **Server-Side Pagination**:
  - Admin searches and student notifications lists implement pagination.
  - Prevents database resource exhaustion when the user base grows.

---

## 3. Client & Network Bandwidth Optimization

- **Vite Bundling**:
  - Executes code tree-shaking, removing dead code.
  - Groups chunks to enable parallel browser loading and local caching.
- **Static Nginx Compression**:
  - Nginx uses `gzip` compression for text, JSON, CSS, and JS file transmissions.
  - Compresses transfer payloads by up to 70%, improving page load speeds on mobile networks.
- **Lazy Rendering**:
  - Tabulated grade data and notification logs load on-demand, reducing initial page render times.
