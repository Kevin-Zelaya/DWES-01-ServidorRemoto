using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Cache.Common;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using StackExchange.Redis;

public class DependencyProvider
{
    public static void Configure(
        IServiceCollection services,
        IHostEnvironment enviroment
    )
    {
        Console.WriteLine($"Environment: {enviroment.EnvironmentName}");
    Console.WriteLine($"Redis: {AppConfig.Config["Redis:ConnectionString"]}");
        // Refit
        services.AddHttpClient("jasonplaceholder", client =>
        {
            client.BaseAddress = new Uri(AppConfig.ApiUrl);
            client.DefaultRequestHeaders.Add("Acept", "application/jon");
        })
        .AddRefitClient<IUserApi>();
        // Servicio se manejo de api
        services.AddScoped<IUserApiService, UserApiService>();
        // Orquestador del negocio
        services.AddScoped<IUserService, UserService>();
        // Unidad de trabajo
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        //if (enviroment.IsDevelopment())
        if (enviroment.IsProduction())
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(AppConfig.ConnectionString));
            services.AddScoped<IRepository, PostgreSqlRepository>();
            var redis = ConnectionMultiplexer.Connect(
                AppConfig.Config["Redis:ConnectionString"]!
            );
            services.AddSingleton<IConnectionMultiplexer>(redis);

            services.AddSingleton(typeof(ICache<>), typeof(RedisCache<>));
            Console.WriteLine("aqui mira");
        }
        else// if(enviroment.IsProduction())
        {
            // Uso de sqlite
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(AppConfig.ConnectionString);
            });
            // Inyección de repositorio sqlite para la interfaz de repositorios
            services.AddScoped<IRepository, SqliteRepository>();
            // Imemory cache
            services.AddMemoryCache();
            services.AddSingleton(typeof(ICache<>), typeof(InMemoryCache<>));
        }
        // Controler
        services.AddScoped<UserControllers>();

        
        // Gestiona las transacciones,
        services.AddScoped<UnitOfWork>();
        // Background service
        services.AddHostedService<UserSyncBackgroundService>();
    }
}