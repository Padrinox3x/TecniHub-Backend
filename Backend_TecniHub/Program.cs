using Microsoft.EntityFrameworkCore;
using TecniHub.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.

// TecniHub - despliegue CI/CD Prueba 2

// Registrar DbContext con PostgreSQL
builder.Services.AddDbContext<TecniHubDbContext>(options =>
    options.UseNpgsql(connectionString));

// Registrar los demás servicios (Controllers, Swagger, etc.)
builder.Services.AddControllers();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
