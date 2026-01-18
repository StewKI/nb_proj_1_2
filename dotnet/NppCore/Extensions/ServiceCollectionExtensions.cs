using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NppCore.Configuration;
using NppCore.Services.Features.Auth;
using NppCore.Services.Features.Match;
using NppCore.Services.Features.Player;
using NppCore.Services.Persistence.Cassandra;
using NppCore.Services.Persistence.Redis;



namespace NppCore.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisConfiguration>(configuration.GetSection("Redis"));
        services.AddSingleton<IRedisService, RedisService>();
        return services;
    }

    public static IServiceCollection AddCassandraService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CassandraConfiguration>(configuration.GetSection("Cassandra"));
        services.AddSingleton<ICassandraService, CassandraService>();
        return services;
    }

    public static IServiceCollection AddPlayerService(this IServiceCollection services)
    {
        services.AddScoped<IPlayerService, PlayerService>();
        return services;
    }

    public static IServiceCollection AddAuthService(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        return services;
    }

   public static IServiceCollection AddPlayerMatchService(this IServiceCollection services)
    {
        services.AddScoped<IPlayerMatchesService, PlayerMatchesService>();
        return services;
    }
   
}
