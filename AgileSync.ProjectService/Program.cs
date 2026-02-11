using AgileSync.ProjectService.Endpoints;
using AgileSync.ProjectService.Models;
using AgileSync.Shared.Extensions;
using AgileSync.Shared.Repositories;
using FluentValidation;
using MongoDB.Driver;

// ── Project Service ─────────────────────────────────────
// Handles CRUD for projects, boards, work items, and sprints.

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddMongoDb(builder.Configuration);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddCors(c => c.AddDefaultPolicy(p => p.WithOrigins("http://localhost:5001").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "AgileSync Project Service", Version = "v1" }));

builder.Services.AddScoped<IRepository<Project>>(sp =>
    new MongoRepository<Project>(sp.GetRequiredService<IMongoDatabase>(), "projects"));
builder.Services.AddScoped<IRepository<Board>>(sp =>
    new MongoRepository<Board>(sp.GetRequiredService<IMongoDatabase>(), "boards"));
builder.Services.AddScoped<IRepository<WorkItem>>(sp =>
    new MongoRepository<WorkItem>(sp.GetRequiredService<IMongoDatabase>(), "workitems"));
builder.Services.AddScoped<IRepository<Sprint>>(sp =>
    new MongoRepository<Sprint>(sp.GetRequiredService<IMongoDatabase>(), "sprints"));

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Project Service v1"));
}

app.MapProjectEndpoints();
app.MapBoardEndpoints();
app.MapWorkItemEndpoints();
app.MapSprintEndpoints();

app.Run();
