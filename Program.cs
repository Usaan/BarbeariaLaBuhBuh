using BarbeariaLaBuhBuh.Data;
using BarbeariaLaBuhBuh.Extensions;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Banco de dados
builder.Services.AddDbContext<BarbeariaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers
builder.Services.AddControllers();

// OpenAPI / documentação
builder.Services.AddOpenApi();

// Serviços da aplicação
builder.Services.AddApplicationServices();

// CORS para permitir acesso do frontend (Next.js)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

// OpenAPI apenas em desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// habilita CORS
app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// necessário para rodar em Docker/Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();