# 01 - Deployment Guide (Phase 11)

This guide details the global deployment strategy, server host prerequisites, software requirements, and step-by-step instructions to deploy the Academic GPA Management System to a production-grade infrastructure.

---

## 1. System Topology Overview

The application features a containerized architecture containing four main layers:

```
                  ┌───────────────────────┐
                  │   Client Web Browser  │
                  └───────────┬───────────┘
                              │ Port 80 / 443
                              ▼
                  ┌───────────────────────┐
                  │    React + Nginx      │ (Frontend Container)
                  └───────────┬───────────┘
                              │
               ┌──────────────┴──────────────┐
               │ /api                        │ / (Static Files)
               ▼                             ▼
  ┌───────────────────────┐       ┌───────────────────────┐
  │   ASP.NET Core API    │       │ Static HTML/JS/CSS    │
  └────┬──────────────┬───┘       └───────────────────────┘
       │              │
       │ Port 1433    │ Port 8000
       ▼              ▼
  ┌──────────┐   ┌───────────────────────┐
  │ SQL Server│   │  Python FastAPI AI    │
  └──────────┘   └───────────────────────┘
```

---

## 2. Server Host Requirements

### Hardware Prerequisites
- **CPU**: Dual-core (minimum), Quad-core (recommended)
- **RAM**: 4 GB (minimum), 8 GB (recommended) for SQL Server container.
- **Storage**: 20 GB free disk space (SSD recommended) for database volume persistence.

### Operating System & Dependencies
- **OS**: Ubuntu Linux 22.04 LTS (recommended) or any OS supporting Docker.
- **Dependencies**:
  - Docker Engine >= 24.0.0
  - Docker Compose >= 2.20.0

---

## 3. Step-by-Step Installation Instructions

### Step 1: Clone the Repository & Configure Env
Copy the system files onto the target host and create a `.env` file in the root directory:
```bash
cp .env.example .env
nano .env
```
*(Define values for DB_PASSWORD, JWT_SECRET, and AI_SERVICE_API_KEY)*

### Step 2: Build & Start Containers
Launch the stack using the production docker-compose profile:
```bash
docker compose -f docker-compose.prod.yml up -d --build
```

### Step 3: Database Initialization
Verify database tables seed and migrations execute successfully:
```bash
docker compose -f docker-compose.prod.yml logs backend
```

### Step 4: Access Verification
Open a browser and navigate to the server's public IP address. The application login page should load, and status checks to `/api/health` should return `200 OK`.
