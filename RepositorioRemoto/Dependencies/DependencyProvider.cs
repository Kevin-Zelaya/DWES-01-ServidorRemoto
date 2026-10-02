using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using RepositorioRemoto.Cache;
using RepositorioRemoto.Cache.Common;

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
        
        if (enviroment.IsDevelopment())
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