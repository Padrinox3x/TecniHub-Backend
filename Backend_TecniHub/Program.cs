using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
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

// Permitir solicitudes desde Angular (Amplify en producción y localhost en desarrollo)
var defaultOrigins = new[]
{
    "https://main.d30i6p1ps5qp33.amplifyapp.com",
    "http://localhost:4200"
};

var configuredOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

var origins = defaultOrigins
    .Concat(configuredOrigins)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularFrontend", policy =>
    {
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configurar la tubería de peticiones HTTP (Middleware)
app.UseCors("AngularFrontend");

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature is not null)
        {
            var logger = context.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("GlobalExceptionHandler");
            logger.LogError(
                exceptionFeature.Error,
                "Excepción no controlada procesando {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }

        // ExceptionHandlerMiddleware limpia las cabeceras de respuesta antes de ejecutar este handler;
        // re-aplicar la política CORS garantiza que las respuestas 500 conserven Access-Control-Allow-Origin.
        var corsService = context.RequestServices.GetService<ICorsService>();
        var corsPolicyProvider = context.RequestServices.GetService<ICorsPolicyProvider>();
        if (corsService is not null && corsPolicyProvider is not null)
        {
            var policy = await corsPolicyProvider.GetPolicyAsync(context, "AngularFrontend");
            if (policy is not null)
            {
                var corsResult = corsService.EvaluatePolicy(context, policy);
                corsService.ApplyResult(corsResult, context.Response);
            }
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            mensaje = "Ocurrió un error interno al procesar la solicitud."
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// El orden de los middlewares de seguridad es crítico
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();