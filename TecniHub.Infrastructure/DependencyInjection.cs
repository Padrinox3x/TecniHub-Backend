
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Application.Services;
using TecniHub.Domain.Services;
using TecniHub.Infrastructure.Persistence;
using TecniHub.Infrastructure.Persistence.Repositories;
using TecniHub.Infrastructure.Security;

namespace TecniHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. DbContext - PostgreSQL
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<TecniHubDbContext>(options =>
            options.UseNpgsql(connectionString));

        // 2. Servicios de seguridad
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // 3. Repositorios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ITecnicoRepository, TecnicoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISesionRepository, SesionRepository>();
        services.AddScoped<IIntentoLoginRepository, IntentoLoginRepository>();
        services.AddScoped<IPublicacionServicioRepository, PublicacionServicioRepository>();
        services.AddScoped<IPublicacionPiezaRepository, PublicacionPiezaRepository>();

        // 4. Unidad de trabajo
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // 5. Servicios de dominio
        services.AddScoped<ReglasAsignacionService>();

        // 6. Servicios de aplicación
        services.AddScoped<IRegistroClienteService, RegistroClienteService>();
        services.AddScoped<IRegistroTecnicoService, RegistroTecnicoService>();
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<ITablonService, TablonService>();

        // 6. Autenticación JWT
        var jwtKey = configuration["JwtSettings:SecretKey"];

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException(
                "No se ha configurado Jwt:Key.");
        }

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["JwtSettings:Issuer"],
                        ValidAudience = configuration["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
            });

        services.AddAuthorization();

        return services;
    }
}
