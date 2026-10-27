using TecniHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// TecniHub - despliegue CI/CD Prueba 2

// 1. Registrar servicios de Infraestructura (DbContext Neon, Repositorios y Servicios de Seguridad)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Registrar servicios propios de la API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar la tubería de peticiones HTTP (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// El orden de los middlewares de seguridad es crítico
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();