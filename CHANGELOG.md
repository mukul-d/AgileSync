# Changelog

All notable changes to this project are documented here.

## 2026-02-12

### Added

#### Infrastructure & Orchestration
- .NET Aspire AppHost orchestrating all services and the SolidJS frontend
- Shared ServiceDefaults with OpenTelemetry, health checks, service discovery, and HTTP resilience
- YARP-based API Gateway reverse proxy routing to Identity and Project services
- Aggregated Swagger documentation across all services
- CORS configuration for cross-origin frontend requests

#### Identity Service
- User registration and login endpoints
- Admin login with separate authentication flow
- Organization (tenant) creation and management
- Tenant admin provisioning endpoint
- BCrypt password hashing for secure credential storage
- FluentValidation for all request DTOs

#### Project Service
- Full CRUD for projects, boards, work items, and sprints
- MongoDB-backed data access via generic repository pattern
- FluentValidation for all request DTOs

#### Shared Library
- Generic `IRepository<T>` and `MongoRepository<T>` for MongoDB data access
- `BaseEntity` with auto-generated ID and timestamps
- `BaseResponse<T>` standardized API response wrapper
- `ValidationFilter<T>` endpoint filter for FluentValidation integration
- MongoDB service registration extensions

#### Frontend (SolidJS)
- User login page
- Admin login page
- Admin tenants management page
- Dark/light theme toggle with system preference detection
- Progressive Web App with service worker and offline caching
- API proxy configuration for development

#### Testing
- 169 backend tests covering endpoints, validators, and shared utilities
- 29 frontend tests covering components, pages, and utilities
- Release build gate enforcing all tests pass via Directory.Build.targets

#### Security
- Migrated MongoDB connection strings and SuperAdmin credentials to .NET User Secrets
- Appsettings files contain only empty placeholders — no secrets committed to source control
- Test factories use dummy connection strings independent of real credentials

### Fixed
- Release build gate now fires only from AppHost (prevents race condition with unbuilt test DLLs)

### Changed
- Restructured project layout from nested `src/` folders to flat Aspire-convention layout
- Renamed frontend project to `AgileSync.WebApp` for naming consistency
- Migrated Vitest config to import `defineConfig` from `vitest/config`
