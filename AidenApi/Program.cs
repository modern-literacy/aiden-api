using System.Net;
using AidenApi.Controllers;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ─────────────────────────────────────────────────────────────
var engineUrl = builder.Configuration["ENGINE_URL"]
    ?? Environment.GetEnvironmentVariable("ENGINE_URL")
    ?? "http://localhost:3000";

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AIDEN API", Version = "v1" });
});

// HttpClient forwarding to the TypeScript engine
builder.Services.AddHttpClient("engine", client =>
{
    client.BaseAddress = new Uri(engineUrl);
    client.Timeout = TimeSpan.FromSeconds(120);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// CORS — tighten in production
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Health checks
builder.Services.AddHealthChecks();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AIDEN API v1"));
}

app.UseCors("default");

// Request ID propagation
app.Use(async (context, next) =>
{
    var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();
    context.Response.Headers["X-Request-ID"] = requestId;
    context.Items["RequestId"] = requestId;
    await next();
});

app.MapHealthChecks("/health");
app.MapControllers();

// Root redirect to Swagger in dev
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.Run($"http://0.0.0.0:{builder.Configuration["PORT"] ?? "5000"}");
