using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TecniHub.Application.Common.Interfaces;
using TecniHub.Infrastructure.Persistence;
using TecniHub.Infrastructure.Persistence.Repositories;
using TecniHub.Infrastructure.Security;

namespace TecniHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<TecniHubDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Servicios de Seguridad
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Repositorios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        // Añadir en el método AddInfrastructureServices
        services.AddScoped<ISesionRepository, SesionRepository>();
        services.AddScoped<IIntentoLoginRepository, IntentoLoginRepository>();

        return services;
    }
}