using Microsoft.OpenApi.Models;
using TecniHub.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Registrar servicios de Infraestructura (DbContext Neon, Repositorios y Servicios de Seguridad)
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Registrar servicios propios de la API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TecniHub API",
        Version = "v1",
        Description = "API de TecniHub para gestión de servicios técnicos, refacciones y perfiles"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' [espacio] y luego su token JWT.\r\nEjemplo: \"Bearer eyJhbGciOi...\""
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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
