# AgileSync

An agile project management platform built with .NET 10 (LTS) microservices, SolidJS, and MongoDB.

## Architecture

```
AgileSync.sln
├── AgileSync.AppHost              → .NET Aspire orchestrator (single run)
├── AgileSync.ServiceDefaults      → Shared health checks, telemetry, resilience
├── AgileSync.ApiGateway           → YARP reverse proxy
├── AgileSync.ProjectService       → Projects, Boards, Sprints, Work Items
├── AgileSync.IdentityService      → Users, Auth
├── AgileSync.Shared               → MongoDB abstractions, base models, repositories
├── AgileSync.WebApp               → SolidJS + Vite
└── tests/
    ├── AgileSync.Shared.Tests
    ├── AgileSync.ProjectService.Tests
    └── AgileSync.IdentityService.Tests
```

## Prerequisites

- [.NET 10 SDK (LTS)](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [MongoDB Atlas](https://www.mongodb.com/atlas) cluster (or local MongoDB)

## Setup

### 1. Configure Secrets

Secrets are managed via [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) — never stored in source control.

```bash
# Identity Service
cd AgileSync.IdentityService
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://<user>:<pass>@cluster0.xxxxx.mongodb.net/"
dotnet user-secrets set "SuperAdmin:Username" "superadmin"
dotnet user-secrets set "SuperAdmin:Password" "<your-password>"

# Project Service
cd ../AgileSync.ProjectService
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://<user>:<pass>@cluster0.xxxxx.mongodb.net/"
```

### 2. Install frontend dependencies

```bash
cd AgileSync.WebApp
npm install
```

### 3. Run everything (single command)

```bash
dotnet run --project AgileSync.AppHost
```

This starts the **Aspire dashboard** with all services and the frontend orchestrated together. The dashboard gives you real-time logs, traces, and health checks for every service.

### Alternative: Run services individually

```bash
# Each in a separate terminal
dotnet run --project AgileSync.ApiGateway
dotnet run --project AgileSync.ProjectService
dotnet run --project AgileSync.IdentityService
cd AgileSync.WebApp && npm run dev
```

## API Routes (via Gateway on port 5000)

| Route                         | Service          |
|-------------------------------|------------------|
| `/api/projects/**`            | ProjectService   |
| `/api/identity/**`            | IdentityService  |
| `/health`                     | Gateway health   |

## Tech Stack

- **Backend**: .NET 10 (LTS), Minimal APIs, YARP
- **Orchestration**: .NET Aspire (single-command run, dashboard, telemetry)
- **Frontend**: SolidJS, Vite, TypeScript
- **Database**: MongoDB (via MongoDB.Driver 3.x)
- **Auth**: BCrypt password hashing (JWT coming soon)
