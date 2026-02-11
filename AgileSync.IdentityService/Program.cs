using AgileSync.IdentityService.Endpoints;
using AgileSync.IdentityService.Models;
using AgileSync.Shared.Extensions;
using AgileSync.Shared.Repositories;
using FluentValidation;
using MongoDB.Driver;

// ── Identity Service ────────────────────────────────────
// Handles user registration, authentication, theme preferences,
// superadmin operations, and tenant organization management.

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCors(c => c.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5001").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "AgileSync Identity Service", Version = "v1" }));

builder.Services.AddScoped<IRepository<User>>(sp =>
    new MongoRepository<User>(sp.GetRequiredService<IMongoDatabase>(), "users"));
builder.Services.AddScoped<IRepository<Organization>>(sp =>
    new MongoRepository<Organization>(sp.GetRequiredService<IMongoDatabase>(), "organizations"));

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service v1"));
}

app.MapIdentityEndpoints();
app.MapAdminEndpoints();

app.Run();
