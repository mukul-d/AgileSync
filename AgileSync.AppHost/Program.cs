// ── AgileSync Aspire AppHost ─────────────────────────────
// Orchestrates all services, the API gateway, and the frontend.

var builder = DistributedApplication.CreateBuilder(args);

/// Project Service — manages projects, boards, work items, and sprints.
var projectService = builder.AddProject<Projects.AgileSync_ProjectService>("project-service")
    .WithUrlForEndpoint("http", ep => new() { Url = "/swagger", DisplayText = "Swagger" });

/// Identity Service — handles auth, users, organizations, and tenant admin.
var identityService = builder.AddProject<Projects.AgileSync_IdentityService>("identity-service")
    .WithUrlForEndpoint("http", ep => new() { Url = "/swagger", DisplayText = "Swagger" });

/// API Gateway — YARP reverse proxy that routes to downstream services.
var gateway = builder.AddProject<Projects.AgileSync_ApiGateway>("api-gateway")
    .WithReference(projectService)
    .WithReference(identityService)
    .WithUrlForEndpoint("http", ep => new() { Url = "/swagger", DisplayText = "Swagger" });

/// SolidJS Frontend — the main web application and admin panel.
var frontend = builder.AddNpmApp("frontend", "../AgileSync.WebApp", "dev")
    .WithReference(gateway)
    .WithUrl("http://localhost:3000")
    .WithUrl("http://localhost:3000/admin", "Admin");

builder.Build().Run();
