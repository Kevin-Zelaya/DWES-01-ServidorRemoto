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
        services.AddScoped<UserService>();
        // Logs
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(AppConfig.Config)
            .WriteTo.Console(
            theme: AnsiConsoleTheme.Code,
            outputTemplate:
            "{Timestamp:HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        )
            .CreateLogger(); 
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
        

        
        
        // Gestiona las transacciones,
        services.AddScoped<UnitOfWork>();
        // Background service
        services.AddHostedService<UserSyncBackgroundService>();
    }
}