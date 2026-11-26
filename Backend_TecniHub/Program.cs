using Microsoft.OpenApi;
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

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

// Permitir solicitudes desde Angular (Local y AWS Amplify)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return uri.Host == "localhost" || uri.Host.EndsWith(".amplifyapp.com");
                }
                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configurar la tubería de peticiones HTTP (Middleware)
app.UseSwagger(c =>
{
    c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Aplicar política CORS para Angular
app.UseCors("AngularFrontend");

// Middleware de seguridad
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();