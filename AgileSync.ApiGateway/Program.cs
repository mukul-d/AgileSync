// ── AgileSync API Gateway ────────────────────────────────
// YARP reverse proxy aggregating Identity and Project services
// with service discovery and Swagger documentation.

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseCors();

/// Sample health-check endpoint.
app.MapGet("/api/sample", () => Results.Ok(new
{
    Message = "Hello from AgileSync API!",
    Timestamp = DateTime.UtcNow,
    Version = "1.0"
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/identity/v1/swagger.json", "Identity Service v1");
        c.SwaggerEndpoint("/swagger/projects/v1/swagger.json", "Project Service v1");
    });
}
app.MapReverseProxy();

app.Run();
