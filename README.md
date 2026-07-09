# Academic GPA Management System

A web application designed for university students to calculate, plan, and optimize their academic performance, featuring an AI Academic Advisor and Admin locks controls.

---

## ⚡ Quick Start (Chạy nhanh)

### 🔹 Backend
```bash
cd backend
dotnet run --project src/AcademicGPA.API
```

### 🔹 Frontend
```bash
cd frontend
npm run dev
```

### 🔹 AI Advisor Service
```bash
cd ai-service
# Kích hoạt môi trường ảo nếu có: venv\Scripts\activate
uvicorn main:app --port 8000 --reload
```

---

## 🏛️ Project Directory Structure

```
aiiii/ (Workspace Root)
├── backend/                              # .NET 10 Clean Architecture API
│   ├── AcademicGPASystem.sln
│   ├── src/
│   │   ├── AcademicGPA.Domain/          # Core entities (User, RefreshTokens)
│   │   ├── AcademicGPA.Application/     # MediatR commands, validators
│   │   ├── AcademicGPA.Infrastructure/  # EF Core, DbContext, JWT, BCrypt
│   │   └── AcademicGPA.API/             # Web API Controllers, program bootstrapper
│   └── tests/
│       └── AcademicGPA.UnitTests/       # xUnit test cases
│
├── frontend/                             # React, TS, Vite, Tailwind client SPA
│   ├── src/
│   │   ├── contexts/                    # Auth, Theme, Language context providers
│   │   ├── api/                         # Axios client, auth API client
│   │   ├── components/layout/           # Student and Guest page layouts
│   │   └── pages/                       # Login, Register, Forgot Password
│   └── package.json
│
├── ai-service/                           # Python FastAPI AI Advisor gateway
│   ├── main.py
│   └── requirements.txt
│
└── docs/                                 # Analysis, UI/UX, database design documents
```

---

## 🚀 Execution Instructions

### 1. Running the Backend API
The backend requires the .NET SDK (version 10 or later) and LocalDB / SQL Server.

1. Navigate to the backend directory:
   ```bash
   cd backend
   ```
2. Build the project solution:
   ```bash
   dotnet build AcademicGPA.slnx
   ```
3. Run the API project:
   ```bash
   dotnet run --project src/AcademicGPA.API
   ```
4. Access the Swagger endpoint in your web browser:
   `https://localhost:5001/swagger/index.html` or `http://localhost:5000/swagger/index.html`

### 2. Running the AI Advisor Service
Requires Python 3.11+.

1. Navigate to the AI service directory:
   ```bash
   cd ai-service
   ```
2. Create and activate a virtual environment (optional):
   ```bash
   python -m venv venv
   # On Windows:
   venv\Scripts\activate
   ```
3. Install dependencies:
   ```bash
   pip install -r requirements.txt
   ```
4. Launch the application:
   ```bash
   uvicorn main:app --port 8000 --reload
   ```

### 3. Running the Frontend SPA
Requires Node.js and npm (version 18+).

1. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```
2. Install client dependencies:
   ```bash
   npm install
   ```
3. Launch Vite dev server:
   ```bash
   npm run dev
   ```
4. Open the link displayed in your terminal (typically `http://localhost:5173/`).

---

## 🧪 Running Automated Tests

To execute the unit tests verifying JWT creation, password hashing, and validator rules:

```bash
dotnet test backend/AcademicGPA.slnx
```
