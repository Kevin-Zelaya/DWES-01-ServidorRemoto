using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

public class DependencyProvider
{
    public static ServiceProvider Configure()
    {
        var services = new ServiceCollection();

        // Refir
        services.AddHttpClient("jasonplaceholder", client =>
        {
            client.BaseAddress = new Uri(AppConfig.ApiUrl);
            client.DefaultRequestHeaders.Add("Acept", "application/jon");
        })
        .AddRefitClient<IUserApi>();

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(AppConfig.ConnectionString);
        });
        services.AddScoped<IUserApiService, UserApiService>();
        services.AddScoped<UserService>();
        services.AddScoped<IRepository, SqliteRepository>();
        services.AddScoped<UnitOfWork>();
        services.AddHostedService<UserSyncBackgroundService>();
        

        return services.BuildServiceProvider();
    }
}