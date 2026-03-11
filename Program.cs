using BarbeariaLaBuhBuh.Data;
using BarbeariaLaBuhBuh.Extensions;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// =========================
// DATABASE
// =========================
builder.Services.AddDbContext<BarbeariaDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// =========================
// CONTROLLERS
// =========================
builder.Services.AddControllers();

// =========================
// OPENAPI / DOCUMENTATION
// =========================
builder.Services.AddOpenApi();

// =========================
// APPLICATION SERVICES
// =========================
builder.Services.AddApplicationServices();

// =========================
// CORS (Next.js frontend)
// =========================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000", // Next.js local
            "https://seu-frontend.vercel.app" // alterar quando subir o front
        )
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

// =========================
// HEALTH CHECK (deploy / monitoramento)
// =========================
builder.Services.AddHealthChecks();

var app = builder.Build();


// =========================
// DOCUMENTATION
// =========================
app.MapOpenApi();
app.MapScalarApiReference();


// =========================
// HTTPS (apenas local)
// =========================
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


// =========================
// CORS
// =========================
app.UseCors("AllowFrontend");


// =========================
// AUTH / AUTHORIZATION
// =========================
app.UseAuthorization();


// =========================
// ROUTES
// =========================
app.MapControllers();


// =========================
// HEALTH CHECK ENDPOINT
// =========================
app.MapHealthChecks("/health");


// =========================
// ROOT ENDPOINT (teste rápido)
// =========================
app.MapGet("/", () => Results.Ok(new
{
    message = "API BarbeariaLaBuhBuh rodando 🚀",
    docs = "/scalar",
    health = "/health"
}));


// =========================
// PORT (Render / Docker)
// =========================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();